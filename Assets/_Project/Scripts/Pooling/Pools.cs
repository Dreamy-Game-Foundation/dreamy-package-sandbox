using Dreamy.Core;
using Dreamy.Core.Pooling;
using UnityEngine;

namespace Dreamy.Template.Pooling
{
    public static class Pools
    {
        private static IPoolService Service => ServiceLocator.Get<IPoolService>();

        public static GameObject Spawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
        {
            return Service.Spawn(prefab, position, rotation, parent);
        }

        public static T Spawn<T>(
            T prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component
        {
            return Service.Spawn(prefab, position, rotation, parent);
        }

        public static GameObject Spawn(GameObject prefab, Transform parent)
        {
            return Service.Spawn(prefab, parent);
        }

        public static T Spawn<T>(T prefab, Transform parent)
            where T : Component
        {
            return Service.Spawn(prefab, parent);
        }

        // New APIs to spawn by GameObject prefab and automatically GetComponent<T>
        public static T Spawn<T>(GameObject prefab, Transform parent)
            where T : Component
        {
            return Service.Spawn<T>(prefab, parent);
        }

        public static T Spawn<T>(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Transform parent = null)
            where T : Component
        {
            return Service.Spawn<T>(prefab, position, rotation, parent);
        }


        public static GameObject Spawn(GameObject prefab) => Service.Spawn(prefab);
        public static T Spawn<T>(T prefab) where T : Component => Service.Spawn(prefab);
        public static T Spawn<T>(GameObject prefab) where T : Component => Service.Spawn<T>(prefab);
        public static GameObject Spawn(GameObject prefab, Vector3 position, Transform parent = null) => Service.Spawn(prefab, position, parent);
        public static T Spawn<T>(T prefab, Vector3 position, Transform parent = null) where T : Component => Service.Spawn(prefab, position, parent);
        public static T Spawn<T>(GameObject prefab, Vector3 position, Transform parent = null) where T : Component => Service.Spawn<T>(prefab, position, parent);
        public static GameObject Spawn(GameObject prefab, PoolSpawnOptions options) => Service.Spawn(prefab, options);
        public static T Spawn<T>(T prefab, PoolSpawnOptions options) where T : Component => Service.Spawn(prefab, options);
        public static T Spawn<T>(GameObject prefab, PoolSpawnOptions options) where T : Component => Service.Spawn<T>(prefab, options);
        public static GameObject SpawnAt(GameObject prefab, Transform target, Transform parent = null) => Service.SpawnAt(prefab, target, parent);
        public static T SpawnAt<T>(GameObject prefab, Transform target, Transform parent = null) where T : Component => Service.SpawnAt<T>(prefab, target, parent);
        public static T SpawnAt<T>(T prefab, Transform target, Transform parent = null) where T : Component => Service.SpawnAt(prefab, target, parent);
        public static GameObject SpawnLocal(GameObject prefab, Transform parent, Vector3 position = default, Quaternion? rotation = null, Vector3? scale = null)
            => Service.SpawnLocal(prefab, parent, position, rotation, scale);
        public static T SpawnLocal<T>(GameObject prefab, Transform parent, Vector3 position = default, Quaternion? rotation = null, Vector3? scale = null) where T : Component
            => Service.SpawnLocal<T>(prefab, parent, position, rotation, scale);
        public static T SpawnLocal<T>(T prefab, Transform parent, Vector3 position = default, Quaternion? rotation = null, Vector3? scale = null) where T : Component
            => Service.SpawnLocal(prefab, parent, position, rotation, scale);
        public static RectTransform SpawnUi(RectTransform prefab, Transform parent, Vector2 anchoredPosition = default, Vector3? scale = null)
            => Service.SpawnUi(prefab, parent, anchoredPosition, scale);
        public static bool TrySpawn(GameObject prefab, out GameObject instance) => Service.TrySpawn(prefab, out instance);
        public static bool TrySpawn(GameObject prefab, PoolSpawnOptions options, out GameObject instance) => Service.TrySpawn(prefab, options, out instance);
        public static bool TrySpawn<T>(GameObject prefab, PoolSpawnOptions options, out T instance) where T : Component => Service.TrySpawn(prefab, options, out instance);
        public static bool TrySpawn<T>(T prefab, PoolSpawnOptions options, out T instance) where T : Component => Service.TrySpawn(prefab, options, out instance);
        public static void Despawn(Component instance, float delay = 0f) => Service.Despawn(instance, delay);
        public static void Preload(GameObject prefab, int count, int capacity = 0, bool persistent = false) => Service.Preload(prefab, count, capacity, persistent);
        public static void Preload<T>(T prefab, int count, int capacity = 0, bool persistent = false) where T : Component => Service.Preload(prefab, count, capacity, persistent);

        public static void Despawn(GameObject instance, float delay = 0f)
        {
            Service.Despawn(instance, delay);
        }

        public static void DespawnAll()
        {
            Service.DespawnAll();
        }
    }
}
