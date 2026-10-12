using System;
using System.Collections.Generic;
using Dreamy.Core.Pooling;
using Lean.Pool;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Dreamy.Template.Pooling
{
    /// <summary>Owns dedicated Lean pools. Do not mix its instances with the static LeanPool API.</summary>
    public sealed class LeanPoolService : IPoolService, IDisposable
    {
        private readonly Dictionary<GameObject, LeanGameObjectPool> pools = new();
        private readonly Dictionary<GameObject, LeanGameObjectPool> owners = new();
        private readonly List<GameObject> scratch = new();
        private readonly Transform poolRoot;
        private bool disposed;

        public LeanPoolService()
        {
            var root = new GameObject("[PoolService]");
            if (Application.isPlaying) Object.DontDestroyOnLoad(root);
            poolRoot = root.transform;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
            => Spawn(prefab, PoolSpawnOptions.World(position, rotation, parent));
        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
            => PoolServiceExtensions.Spawn(this, prefab, PoolSpawnOptions.World(position, rotation, parent));
        public GameObject Spawn(GameObject prefab, Transform parent)
            => Spawn(prefab, new PoolSpawnOptions { Space = Space.Self, Parent = parent });
        public T Spawn<T>(T prefab, Transform parent) where T : Component
            => PoolServiceExtensions.Spawn(this, prefab, new PoolSpawnOptions { Space = Space.Self, Parent = parent });
        public T Spawn<T>(GameObject prefab, Transform parent) where T : Component
            => PoolServiceExtensions.Spawn<T>(this, prefab, new PoolSpawnOptions { Space = Space.Self, Parent = parent });
        public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null) where T : Component
            => PoolServiceExtensions.Spawn<T>(this, prefab, PoolSpawnOptions.World(position, rotation, parent));
        public GameObject Spawn(GameObject prefab, PoolSpawnOptions options)
            => TrySpawn(prefab, options, out var instance) ? instance : null;

        public bool TrySpawn(GameObject prefab, PoolSpawnOptions options, out GameObject instance)
        {
            ThrowIfDisposed();
            if (!prefab) throw new ArgumentNullException(nameof(prefab));
            if (options.Space != Space.World && options.Space != Space.Self)
                throw new ArgumentOutOfRangeException(nameof(options.Space));
            var pool = GetPool(prefab);
            var position = options.Position ?? (options.Space == Space.Self ? prefab.transform.localPosition : prefab.transform.position);
            var rotation = options.Rotation ?? (options.Space == Space.Self ? prefab.transform.localRotation : prefab.transform.rotation);
            if (options.Space == Space.World && options.Parent)
            {
                position = options.Parent.InverseTransformPoint(position);
                rotation = Quaternion.Inverse(options.Parent.rotation) * rotation;
            }
            instance = null;
            try
            {
                if (!pool.TrySpawn(ref instance, position, rotation, options.LocalScale ?? prefab.transform.localScale, options.Parent, false))
                    return false;
                owners[instance] = pool;
                instance.name = options.Name ?? prefab.name;
                options.Configure?.Invoke(instance);
            }
            catch
            {
                // Also handles exceptions from OnSpawn before TrySpawn returns.
                if (pool && instance) pool.Despawn(instance);
                throw;
            }
            return true;
        }

        public void Despawn(GameObject instance, float delay = 0f)
        {
            ThrowIfDisposed();
            if (float.IsNaN(delay) || float.IsInfinity(delay) || delay < 0f)
                throw new ArgumentOutOfRangeException(nameof(delay));
            if (!instance) return;
            var activePool = default(LeanGameObjectPool);
            if (LeanGameObjectPool.TryFindPoolByClone(instance, ref activePool) && pools.ContainsValue(activePool))
            {
                // Includes calls made by OnSpawn before TrySpawn has returned.
                activePool.Despawn(instance, delay);
                return;
            }
            if (owners.TryGetValue(instance, out var owner) && owner) return; // Already returned.
            throw new ArgumentException("Instance was not spawned by this pool service.", nameof(instance));
        }

        public void Preload(GameObject prefab, int count, int capacity = 0, bool persistent = false)
        {
            ThrowIfDisposed();
            if (!prefab) throw new ArgumentNullException(nameof(prefab));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (capacity < 0 || (capacity > 0 && count > capacity)) throw new ArgumentOutOfRangeException(nameof(capacity));
            var pool = GetPool(prefab, persistent);
            if (capacity > 0 && pool.Total > capacity)
                throw new ArgumentException("Capacity cannot be lower than the existing clone count.", nameof(capacity));
            // Persistent pools live under the service root; scene pools are scene roots.
            // Do not set Lean Persist after Awake: it only acts during Awake.
            pool.transform.SetParent(persistent ? poolRoot : null, false);
            if (!persistent) SceneManager.MoveGameObjectToScene(pool.gameObject, SceneManager.GetActiveScene());
            pool.Preload = count;
            pool.Capacity = capacity;
            pool.PreloadAll();
        }

        public void DespawnAll()
        {
            ThrowIfDisposed();
            foreach (var pool in pools.Values) if (pool) pool.DespawnAll(false);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            foreach (var pool in pools.Values)
            {
                if (!pool) continue;
                pool.DespawnAll(false); // Cancels delayed returns and reparents active clones into this pool.
                pool.enabled = false;
                Destroy(pool.gameObject);
            }
            if (poolRoot) Destroy(poolRoot.gameObject);
            pools.Clear();
            owners.Clear();
            scratch.Clear();
        }

        private LeanGameObjectPool GetPool(GameObject prefab, bool persistent = true)
        {
            RemoveDestroyedOwners();
            if (pools.TryGetValue(prefab, out var pool) && pool) return pool;
            var existing = default(LeanGameObjectPool);
            if (LeanGameObjectPool.TryFindPoolByPrefab(prefab, ref existing) && existing)
                throw new InvalidOperationException($"Prefab {prefab.name} already belongs to another Lean pool. Use one owner per prefab.");
            var go = new GameObject($"Pool - {prefab.name}");
            if (persistent) go.transform.SetParent(poolRoot, false);
            pool = go.AddComponent<LeanGameObjectPool>();
            pool.Prefab = prefab;
            pool.Notification = LeanGameObjectPool.NotificationType.IPoolable;
            pool.Recycle = false;
            pools[prefab] = pool;
            return pool;
        }

        private void OnSceneUnloaded(Scene scene) => RemoveDestroyedOwners();

        private void RemoveDestroyedOwners()
        {
            scratch.Clear();
            foreach (var pair in owners)
            {
                if (!pair.Key || !pair.Value)
                {
                    // Scene-owned pools may have active clones parented in another scene.
                    if (pair.Key) Destroy(pair.Key);
                    scratch.Add(pair.Key);
                }
            }
            foreach (var clone in scratch) owners.Remove(clone);
            scratch.Clear();
            foreach (var pair in pools) if (!pair.Key || !pair.Value) scratch.Add(pair.Key);
            foreach (var prefab in scratch) pools.Remove(prefab);
            scratch.Clear();
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(nameof(LeanPoolService));
        }

        private static void Destroy(GameObject go)
        {
            if (Application.isPlaying) Object.Destroy(go);
            else Object.DestroyImmediate(go);
        }
    }
}
