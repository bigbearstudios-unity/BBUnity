using System.Collections.Generic;

using UnityEngine;
using NUnit.Framework;

using BBUnity.Support.Testing;

namespace BBUnity.Pools.Tests {
    public class PoolBehaviorTests {

        private class TrackingPoolBehavior : PoolBehavior, IPoolBehavior {
            public readonly List<string> CallOrder = new List<string>();

            public override void OnCreate() { CallOrder.Add("virtual:OnCreate"); }
            public override void OnSpawn() { CallOrder.Add("virtual:OnSpawn"); }
            public override void OnReturn() { CallOrder.Add("virtual:OnReturn"); }

            void IPoolBehavior.OnCreate(PoolBehavior behavior) { CallOrder.Add("interface:OnCreate"); }
            void IPoolBehavior.OnSpawn(PoolBehavior behavior) { CallOrder.Add("interface:OnSpawn"); }
            void IPoolBehavior.OnReturn(PoolBehavior behavior) { CallOrder.Add("interface:OnReturn"); }
        }

        private class TestMonoBehaviour : MonoBehaviour {}

        private GameObject _prefab;

        [SetUp]
        public void SetUp() {
            _prefab = TestUtilities.CreateGameObject<TrackingPoolBehavior>(name: "Tracking Prefab Stand-in").gameObject;
        }

        [TearDown]
        public void TearDown() {
            TestUtilities.DestroyRootObjectsInScene();
        }

        private ObjectPoolReference CreateReference() {
            return new ObjectPoolReference(_prefab, startingSize: 1, maximumSize: 5);
        }

        [Test]
        public void Create_InvokesVirtualAndInterfaceOnCreate() {
            ObjectPoolReference reference = CreateReference();
            reference.InitializeInstances();

            var instance = (TrackingPoolBehavior)reference.Spawn();

            CollectionAssert.Contains(instance.CallOrder, "virtual:OnCreate");
            CollectionAssert.Contains(instance.CallOrder, "interface:OnCreate");
        }

        [Test]
        public void Spawn_InvokesVirtualAndInterfaceOnSpawn() {
            ObjectPoolReference reference = CreateReference();
            reference.InitializeInstances();

            var instance = (TrackingPoolBehavior)reference.Spawn();

            CollectionAssert.Contains(instance.CallOrder, "virtual:OnSpawn");
            CollectionAssert.Contains(instance.CallOrder, "interface:OnSpawn");
        }

        [Test]
        public void Spawn_ActivatesTheGameObject() {
            ObjectPoolReference reference = CreateReference();
            reference.InitializeInstances();

            var instance = reference.Spawn();

            Assert.IsTrue(instance.gameObject.activeSelf);
        }

        [Test]
        public void ReturnToPool_InvokesOnReturnAndDeactivates() {
            ObjectPoolReference reference = CreateReference();
            reference.InitializeInstances();
            var instance = (TrackingPoolBehavior)reference.Spawn();

            instance.ReturnToPool();

            CollectionAssert.Contains(instance.CallOrder, "virtual:OnReturn");
            CollectionAssert.Contains(instance.CallOrder, "interface:OnReturn");
            Assert.IsFalse(instance.gameObject.activeSelf);
        }

        [Test]
        public void OnSpawnEvent_FiresWhenInstanceIsReused() {
            ObjectPoolReference reference = CreateReference();
            reference.InitializeInstances();
            PoolBehavior instance = reference.Spawn();
            instance.ReturnToPool();

            bool fired = false;
            instance.Spawned += (_) => fired = true;

            reference.Spawn(); // only one instance exists, so this must reuse and re-fire Spawned on it

            Assert.IsTrue(fired);
        }

        [Test]
        public void ReturnToPool_OnReturnEvent_Fires() {
            ObjectPoolReference reference = CreateReference();
            reference.InitializeInstances();
            PoolBehavior instance = reference.Spawn();

            bool fired = false;
            instance.Returned += (_) => fired = true;

            instance.ReturnToPool();

            Assert.IsTrue(fired);
        }

        [Test]
        public void AddOnCreateEvent_MissingComponent_ThrowsMissingComponentException() {
            TestMonoBehaviour plainBehavior = TestUtilities.CreateGameObject<TestMonoBehaviour>(name: "No PoolBehavior Here");

            Assert.Throws<MissingComponentException>(() => PoolBehavior.AddOnCreateEvent(plainBehavior, (_) => {}));
        }

        [Test]
        public void AddOnSpawnEvent_MissingComponent_ThrowsMissingComponentException() {
            TestMonoBehaviour plainBehavior = TestUtilities.CreateGameObject<TestMonoBehaviour>(name: "No PoolBehavior Here");

            Assert.Throws<MissingComponentException>(() => PoolBehavior.AddOnSpawnEvent(plainBehavior, (_) => {}));
        }
    }
}
