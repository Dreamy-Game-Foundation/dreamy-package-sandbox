using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Dreamy.Core;
using Dreamy.Core.Pooling;
using Dreamy.Template.Pooling;
using Lean.Pool;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Dreamy.Template.Tests.Pooling
{
    public sealed class PoolServiceTests
    {
        private LeanPoolService service;
        private GameObject prefab;
        private readonly List<GameObject> objects = new();

        [SetUp] public void SetUp()
        {
            service = new LeanPoolService();
            prefab = Create("PoolFixture");
            prefab.transform.localScale = new Vector3(2, 3, 4);
        }

        [TearDown] public void TearDown()
        {
            service.Dispose();
            foreach (var go in objects) if (go) Object.DestroyImmediate(go);
            objects.Clear();
        }

        private GameObject Create(string name)
        {
            var go = new GameObject(name);
            objects.Add(go);
            return go;
        }

        [Test] public void ReuseResetsTransformScaleAndName()
        {
            IPoolService pool = service;
            var instance = pool.Spawn(prefab, new PoolSpawnOptions { LocalScale = Vector3.one, Name = "Custom" });
            pool.Despawn(instance);
            pool.Despawn(instance); // Duplicate return is harmless.
            var reused = pool.Spawn(prefab);
            Assert.That(reused, Is.SameAs(instance));
            Assert.That(reused.transform.localScale, Is.EqualTo(prefab.transform.localScale));
            Assert.That(reused.name, Is.EqualTo(prefab.name));
            Assert.That(reused.activeSelf, Is.True);
        }

        [Test] public void WorldSpawnUsesWorldPoseWithRotatedScaledParent()
        {
            var parent = Create("Parent").transform;
            parent.position = new Vector3(10, 20, 30);
            parent.rotation = Quaternion.Euler(0, 45, 0);
            parent.localScale = Vector3.one * 2;
            var position = new Vector3(4, 5, 6);
            var rotation = Quaternion.Euler(30, 60, 90);
            var instance = service.Spawn(prefab, position, rotation, parent);
            Assert.That(Vector3.Distance(instance.transform.position, position), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(instance.transform.rotation, rotation), Is.LessThan(0.001f));
        }

        [Test] public void LocalPoseIsAppliedBeforeOnSpawnAndConfiguration()
        {
            prefab.AddComponent<PoolPoseProbe>();
            var parent = Create("Parent").transform;
            parent.position = Vector3.one * 10;
            IPoolService pool = service;
            var expected = new Vector3(1, 2, 3);
            bool configured = false;
            var options = PoolSpawnOptions.Local(parent, expected, Quaternion.identity, Vector3.one * 5);
            options.Configure = go =>
            {
                Assert.That(go.GetComponent<PoolPoseProbe>().SpawnPosition, Is.EqualTo(expected));
                configured = true;
            };
            var instance = pool.Spawn(prefab, options);
            Assert.That(configured, Is.True);
            Assert.That(instance.transform.localPosition, Is.EqualTo(expected));
            Assert.That(instance.transform.localScale, Is.EqualTo(Vector3.one * 5));
            pool.Despawn(instance);
            var reused = pool.SpawnLocal(prefab, parent, Vector3.zero);
            Assert.That(reused.GetComponent<PoolPoseProbe>().SpawnPosition, Is.EqualTo(Vector3.zero));
        }

        [Test] public void SpawnAtAndComponentOverloadsUseTargetPose()
        {
            var target = Create("Target").transform;
            target.SetPositionAndRotation(new Vector3(3, 4, 5), Quaternion.Euler(0, 90, 0));
            IPoolService pool = service;
            var instance = pool.SpawnAt(prefab.transform, target);
            Assert.That(instance.position, Is.EqualTo(target.position));
            Assert.That(Quaternion.Angle(instance.rotation, target.rotation), Is.LessThan(0.001f));
            pool.Despawn(instance);
            Assert.That(pool.Spawn<Transform>(prefab), Is.SameAs(instance));
        }

        [Test] public void UiSpawnResetsAnchoredPositionOnReuse()
        {
            var ui = Create("UiPrefab").AddComponent<RectTransform>();
            var parent = Create("UiParent").AddComponent<RectTransform>();
            IPoolService pool = service;
            var instance = pool.SpawnUi(ui, parent, new Vector2(12, 34));
            Assert.That(instance.anchoredPosition, Is.EqualTo(new Vector2(12, 34)));
            pool.Despawn(instance);
            Assert.That(pool.SpawnUi(ui, parent).anchoredPosition, Is.EqualTo(Vector2.zero));
        }

        [Test] public void CapacityReturnsFalseWithoutStealingAnActiveClone()
        {
            service.Preload(prefab, 1, 1, true);
            Assert.That(service.TrySpawn(prefab, default, out var first), Is.True);
            Assert.That(service.TrySpawn(prefab, default, out var second), Is.False);
            Assert.That(second, Is.Null);
            Assert.That(service.Spawn(prefab, default(PoolSpawnOptions)), Is.Null);
            service.Despawn(first);
            IPoolService pool = service;
            Assert.That(pool.TrySpawn<Transform>(prefab, default, out var reused), Is.True);
            Assert.That(reused.gameObject, Is.SameAs(first));
        }

        [Test] public void MissingComponentDoesNotConsumeCapacity()
        {
            service.Preload(prefab, 1, 1, true);
            IPoolService pool = service;
            Assert.Throws<ArgumentException>(() => pool.Spawn<Rigidbody>(prefab));
            Assert.That(pool.TrySpawn(prefab, out _), Is.True);
        }

        [Test] public void ConfigurationFailureReturnsCloneAndPropagates()
        {
            service.Preload(prefab, 1, 1, true);
            var options = new PoolSpawnOptions { Configure = go => throw new InvalidOperationException("Configuration failed") };
            Assert.Throws<InvalidOperationException>(() => service.Spawn(prefab, options));
            Assert.That(service.TrySpawn(prefab, default, out _), Is.True);
        }

        [Test] public void SpawnNotificationFailureReturnsCloneAndPropagates()
        {
            prefab.AddComponent<PoolPoseProbe>().FailOnSpawn = true;
            service.Preload(prefab, 1, 1, true);
            Assert.Throws<InvalidOperationException>(() => service.Spawn(prefab, default(PoolSpawnOptions)));
            var pool = default(LeanGameObjectPool);
            Assert.That(LeanGameObjectPool.TryFindPoolByPrefab(prefab, ref pool), Is.True);
            Assert.That(pool.Spawned, Is.Zero);
            Assert.That(pool.Despawned, Is.EqualTo(1));
        }

        [Test] public void RejectsForeignObjectsAndInvalidPreloadArguments()
        {
            Assert.Throws<ArgumentException>(() => service.Despawn(prefab));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Preload(prefab, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Preload(prefab, 2, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Despawn(null, float.NaN));
        }

        [UnityTest] public IEnumerator DisposeOnlyCleansOwnedPoolsAndIsIdempotent()
        {
            var externalPrefab = Create("ExternalPrefab");
            var externalRoot = Create("ExternalPool");
            var external = externalRoot.AddComponent<LeanGameObjectPool>();
            external.Prefab = externalPrefab;
            var foreign = external.Spawn(Vector3.zero, Quaternion.identity);
            objects.Add(foreign);
            var own = service.Spawn(prefab, default(PoolSpawnOptions));
            service.Despawn(own, 20f);
            service.DespawnAll();
            Assert.That(foreign.activeSelf, Is.True);
            service.Dispose();
            service.Dispose();
            Assert.Throws<ObjectDisposedException>(() => service.Spawn(prefab, default(PoolSpawnOptions)));
            yield return null;
            Assert.That(own == null, Is.True);
            Assert.That(foreign && foreign.activeSelf, Is.True);
            Assert.That(external, Is.Not.Null);
        }

        [UnityTest] public IEnumerator DelayedReturnAndImmediateReturnDoNotAffectNextLease()
        {
            var instance = service.Spawn(prefab, default(PoolSpawnOptions));
            service.Despawn(instance, 0.03f);
            yield return new WaitForSeconds(0.06f);
            Assert.That(instance.activeSelf, Is.False);
            var reused = service.Spawn(prefab, default(PoolSpawnOptions));
            service.Despawn(reused, 0.03f);
            service.Despawn(reused);
            reused = service.Spawn(prefab, default(PoolSpawnOptions));
            yield return new WaitForSeconds(0.06f);
            Assert.That(reused.activeSelf, Is.True);
        }

        [UnityTest] public IEnumerator ScenePoolIsReleasedWhilePersistentPoolSurvives()
        {
            var previous = SceneManager.GetActiveScene();
            var scene = SceneManager.CreateScene("PoolSceneFixture");
            var persistentPrefab = Create("PersistentPrefab");
            service.Preload(persistentPrefab, 1, persistent: true);
            SceneManager.SetActiveScene(scene);
            service.Preload(prefab, 1, persistent: false);
            // Keep an active clone in the previous scene: service must still clean it.
            var crossSceneParent = Create("CrossSceneParent");
            SceneManager.MoveGameObjectToScene(crossSceneParent, previous);
            var crossSceneClone = service.Spawn(prefab, Vector3.zero, Quaternion.identity, crossSceneParent.transform);
            SceneManager.SetActiveScene(previous);
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null;
            Assert.That(crossSceneClone == null, Is.True);
            var persistentPool = default(LeanGameObjectPool);
            Assert.That(LeanGameObjectPool.TryFindPoolByPrefab(persistentPrefab, ref persistentPool), Is.True);
            Assert.That(persistentPool.Despawned, Is.EqualTo(1));
            Assert.That(service.Spawn(prefab, default(PoolSpawnOptions)), Is.Not.Null);
        }

        [Test] public void InstallerRejectsDuplicateAndPreservesReplacementRegistration()
        {
            ServiceLocator.TryGet<IPoolService>(out var previous);
            ServiceLocator.Unregister<IPoolService>();
            var go = Create("InactiveInstaller"); go.SetActive(false);
            var installer = go.AddComponent<GameInstaller>();
            var install = typeof(GameInstaller).GetMethod("InstallPoolService", BindingFlags.Instance | BindingFlags.NonPublic);
            var release = typeof(GameInstaller).GetMethod("ReleaseFeedbackAndPool", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                install.Invoke(installer, null);
                var owned = ServiceLocator.Get<IPoolService>();
                Assert.That(Assert.Throws<TargetInvocationException>(() => install.Invoke(installer, null)).InnerException, Is.TypeOf<InvalidOperationException>());
                ServiceLocator.Register<IPoolService>(service);
                release.Invoke(installer, null);
                Assert.That(ServiceLocator.Get<IPoolService>(), Is.SameAs(service));
                Assert.Throws<ObjectDisposedException>(() => owned.Spawn(prefab));
            }
            finally
            {
                release.Invoke(installer, null);
                ServiceLocator.Unregister<IPoolService>();
                if (previous != null) ServiceLocator.Register(previous);
            }
        }
    }

    public sealed class PoolPoseProbe : PooledBehaviour
    {
        public Vector3 SpawnPosition;
        public bool FailOnSpawn;
        public override void OnSpawn()
        {
            SpawnPosition = transform.localPosition;
            if (FailOnSpawn) throw new InvalidOperationException("OnSpawn failed");
        }
    }
}
