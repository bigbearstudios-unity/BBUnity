using System;
using NUnit.Framework;
using UnityEngine;

using BBUnity.Utilities;

namespace BBUnity.Utilities.Tests {
    public class GameObjectFactoryTests {

        private System.Collections.Generic.List<GameObject> _created = new();

        private GameObject Track(GameObject obj) {
            _created.Add(obj);
            return obj;
        }

        [TearDown]
        public void TearDown() {
            foreach (var obj in _created) {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            _created.Clear();
        }

        // --- no args (only name) ---

        [Test]
        public void GameObject_ShouldThrow_WhenNameIsNull() {
            Assert.Throws<ArgumentNullException>(() => GameObjectFactory.Create(null));
        }

        [Test]
        public void GameObject_ShouldCreateAtWorldOrigin() {
            var obj = Track(GameObjectFactory.Create("test"));
            Assert.AreEqual(Vector3.zero, obj.transform.position);
        }

        [Test]
        public void GameObject_ShouldCreateWithUniformScale() {
            var obj = Track(GameObjectFactory.Create("test"));
            Assert.AreEqual(Vector3.one, obj.transform.localScale);
        }

        [Test]
        public void GameObject_ShouldCreateWithGivenName() {
            var obj = Track(GameObjectFactory.Create("my-object"));
            Assert.AreEqual("my-object", obj.name);
        }

        // --- parent: named param ---

        [Test]
        public void GameObject_WithParent_ShouldBeChildOfParent() {
            var parent = Track(new GameObject("parent"));
            var child = Track(GameObjectFactory.Create("child", parent: parent.transform));
            Assert.AreEqual(parent.transform, child.transform.parent);
        }

        [Test]
        public void GameObject_WithParent_ShouldHaveLocalPositionAtOrigin() {
            var parent = Track(new GameObject("parent"));
            parent.transform.position = new Vector3(10, 20, 30);
            var child = Track(GameObjectFactory.Create("child", parent: parent.transform));
            Assert.AreEqual(Vector3.zero, child.transform.localPosition);
        }

        [Test]
        public void GameObject_WithParent_ShouldHaveLocalScaleOfOne() {
            var parent = Track(new GameObject("parent"));
            var child = Track(GameObjectFactory.Create("child", parent: parent.transform));
            Assert.AreEqual(Vector3.one, child.transform.localScale);
        }

        // --- components: named param ---

        [Test]
        public void GameObject_WithComponents_ShouldHaveComponentAttached() {
            var obj = Track(GameObjectFactory.Create("test", components: new[] { typeof(BoxCollider) }));
            Assert.IsNotNull(obj.GetComponent<BoxCollider>());
        }

        [Test]
        public void GameObject_WithComponents_ShouldCreateAtWorldOrigin() {
            var obj = Track(GameObjectFactory.Create("test", components: new[] { typeof(BoxCollider) }));
            Assert.AreEqual(Vector3.zero, obj.transform.position);
        }

        // --- parent + components: both named params ---

        [Test]
        public void GameObject_WithParentAndComponents_ShouldBeChildOfParentWithComponent() {
            var parent = Track(new GameObject("parent"));
            var child = Track(GameObjectFactory.Create("child", parent: parent.transform, components: new[] { typeof(BoxCollider) }));
            Assert.AreEqual(parent.transform, child.transform.parent);
            Assert.IsNotNull(child.GetComponent<BoxCollider>());
        }

        [Test]
        public void GameObject_WithParentAndComponents_ShouldHaveLocalPositionAtOrigin() {
            var parent = Track(new GameObject("parent"));
            parent.transform.position = new Vector3(5, 5, 5);
            var child = Track(GameObjectFactory.Create("child", parent: parent.transform, components: new[] { typeof(BoxCollider) }));
            Assert.AreEqual(Vector3.zero, child.transform.localPosition);
        }

        // --- InstantiatedGameObjectWithComponent ---

        [Test]
        public void InstantiatedGameObjectWithComponent_ShouldThrow_WhenPrefabIsNull() {
            Assert.Throws<ArgumentNullException>(() => GameObjectFactory.InstantiatedGameObjectWithComponent<BoxCollider>(null));
        }

        [Test]
        public void InstantiatedGameObjectWithComponent_ShouldReturnComponent() {
            var prefab = Track(new GameObject("prefab"));
            prefab.AddComponent<BoxCollider>();
            var result = GameObjectFactory.InstantiatedGameObjectWithComponent<BoxCollider>(prefab);
            Track(result.gameObject);
            Assert.IsNotNull(result);
        }

        // --- AddOrGetComponent (moved here from the removed GameObjectExtensions) ---

        [Test]
        public void AddOrGetComponent_ShouldThrow_WhenGameObjectIsNull() {
            Assert.Throws<ArgumentNullException>(() => GameObjectFactory.AddOrGetComponent<Rigidbody>(null));
        }

        [Test]
        public void AddOrGetComponent_ShouldAddComponent_WhenComponentIsNotPresent() {
            var obj = Track(new GameObject("test"));
            var result = GameObjectFactory.AddOrGetComponent<BoxCollider>(obj);
            Assert.IsNotNull(result);
            Assert.IsNotNull(obj.GetComponent<BoxCollider>());
        }

        [Test]
        public void AddOrGetComponent_ShouldReturnExistingComponent_WhenComponentIsAlreadyPresent() {
            var obj = Track(new GameObject("test"));
            var first = obj.AddComponent<BoxCollider>();
            var second = GameObjectFactory.AddOrGetComponent<BoxCollider>(obj);
            Assert.AreSame(first, second);
        }
    }
}
