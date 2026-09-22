using System;
using NUnit.Framework;
using UnityEngine;

using TestSupport;

namespace Tests {
    public class TestUtilitiesTests {

        /*
         * CreateGameObject
         */

        [Test]
        public void CreateGameObject_ShouldReturnANonNullGameObject() {
            UnityAssert.ChangeInSceneRootObjects(1, () => {
                GameObject obj = TestUtilities.CreateGameObject();
                Assert.NotNull(obj);
                GameObject.DestroyImmediate(obj);
            });
        }

        [Test]
        public void CreateGameObject_WithName_ShouldReturnAGameObjectWithTheGivenName() {
            UnityAssert.ChangeInSceneRootObjects(1, () => {
                GameObject obj = TestUtilities.CreateGameObject(name: "Test Object");
                Assert.AreEqual("Test Object", obj.name);
                GameObject.DestroyImmediate(obj);
            });
        }

        [Test]
        public void CreateGameObject_WithComponents_ShouldReturnAGameObjectWithTheGivenComponents() {
            UnityAssert.ChangeInSceneRootObjects(1, () => {
                GameObject obj = TestUtilities.CreateGameObject(components: new[] { typeof(Camera) });
                Assert.NotNull(obj.GetComponent<Camera>());
                GameObject.DestroyImmediate(obj);
            });
        }

        [Test]
        public void CreateGameObject_WithParent_ShouldReturnAGameObjectWithTheGivenParent() {
            GameObject parent = TestUtilities.CreateGameObject(name: "Parent");
            GameObject child  = TestUtilities.CreateGameObject(name: "Child", parent: parent.transform);
            Assert.AreEqual(parent.transform, child.transform.parent);
            GameObject.DestroyImmediate(parent);
        }

        /*
         * CreateGameObject<T>
         */

        [Test]
        public void CreateGameObjectGeneric_ShouldReturnTheRequestedComponent() {
            UnityAssert.ChangeInSceneRootObjects(1, () => {
                Camera camera = TestUtilities.CreateGameObject<Camera>();
                Assert.NotNull(camera);
                GameObject.DestroyImmediate(camera.gameObject);
            });
        }

        [Test]
        public void CreateGameObjectGeneric_WithName_ShouldReturnComponentOnNamedGameObject() {
            UnityAssert.ChangeInSceneRootObjects(1, () => {
                Camera camera = TestUtilities.CreateGameObject<Camera>(name: "Camera Object");
                Assert.AreEqual("Camera Object", camera.gameObject.name);
                GameObject.DestroyImmediate(camera.gameObject);
            });
        }

        /*
         * CreateThenDestroyGameObject
         */

        [Test]
        public void CreateThenDestroyGameObject_ShouldCreateAndDestroyTheGameObject() {
            UnityAssert.ChangeInSceneRootObjects(0, () => {
                TestUtilities.CreateThenDestroyGameObject(obj => {
                    Assert.NotNull(obj);
                });
            });
        }

        [Test]
        public void CreateThenDestroyGameObject_WithName_ShouldCreateGameObjectWithGivenName() {
            UnityAssert.ChangeInSceneRootObjects(0, () => {
                TestUtilities.CreateThenDestroyGameObject(obj => {
                    Assert.AreEqual("Test Object", obj.name);
                }, name: "Test Object");
            });
        }

        [Test]
        public void CreateThenDestroyGameObject_WithComponents_ShouldCreateGameObjectWithGivenComponents() {
            UnityAssert.ChangeInSceneRootObjects(0, () => {
                TestUtilities.CreateThenDestroyGameObject(obj => {
                    Assert.NotNull(obj.GetComponent<Camera>());
                }, components: new[] { typeof(Camera) });
            });
        }

        [Test]
        public void CreateThenDestroyGameObject_WithParent_ShouldCreateGameObjectWithGivenParent() {
            TestUtilities.CreateThenDestroyGameObject(parent => {
                TestUtilities.CreateThenDestroyGameObject(child => {
                    Assert.AreEqual(parent.transform, child.transform.parent);
                }, name: "Child", parent: parent.transform);
            }, name: "Parent");
        }

        /*
         * CreateThenDestroyGameObject<T>
         */

        [Test]
        public void CreateThenDestroyGameObjectGeneric_ShouldCreateAndDestroyWithComponent() {
            UnityAssert.ChangeInSceneRootObjects(0, () => {
                TestUtilities.CreateThenDestroyGameObject<Camera>(cam => {
                    Assert.NotNull(cam);
                });
            });
        }

        [Test]
        public void CreateThenDestroyGameObjectGeneric_WithName_ShouldCreateNamedObjectWithComponent() {
            UnityAssert.ChangeInSceneRootObjects(0, () => {
                TestUtilities.CreateThenDestroyGameObject<Camera>(cam => {
                    Assert.AreEqual("Camera Object", cam.gameObject.name);
                }, name: "Camera Object");
            });
        }

        [Test]
        public void CreateThenDestroyGameObjectGeneric_WithComponents_ShouldIncludeAllComponents() {
            UnityAssert.ChangeInSceneRootObjects(0, () => {
                TestUtilities.CreateThenDestroyGameObject<Camera>(cam => {
                    Assert.NotNull(cam);
                    Assert.NotNull(cam.gameObject.GetComponent<AudioListener>());
                }, components: new[] { typeof(Camera), typeof(AudioListener) });
            });
        }
    }
}
