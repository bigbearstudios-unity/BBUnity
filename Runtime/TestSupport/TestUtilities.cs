using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestSupport {
    public class TestUtilities {

        /// <summary>
        /// Returns the number of root objects in the scene. Note that this is more
        /// useful for checking the *change* in root objects rather than an absolute
        /// count, because Unity adds its own root objects to blank test scenes.
        /// </summary>
        public static int NumberOfRootObjectsInScene {
            get { return SceneManager.GetActiveScene().rootCount; }
        }

        public static void DestroyRootObjectsInScene() {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects()) {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        /*
         * Internal helpers
         */

        private static GameObject BuildGameObject(string name, Transform parent, Type[] components) {
            GameObject obj = (components != null && components.Length > 0)
                ? new GameObject(name, components)
                : new GameObject(name);
            if (parent != null) { obj.transform.SetParent(parent, false); }
            return obj;
        }

        private static T AddOrGetComponent<T>(GameObject obj) where T : Component {
            T component = obj.GetComponent<T>();
            return component != null ? component : obj.AddComponent<T>();
        }

        /*
         * CreateGameObject — returns the object; caller is responsible for destroying it.
         *
         *   GameObject obj = TestUtilities.CreateGameObject();
         *   GameObject obj = TestUtilities.CreateGameObject(name: "Player");
         *   Camera    cam  = TestUtilities.CreateGameObject<Camera>(name: "Cam");
         */

        public static GameObject CreateGameObject(
            string name = "Game Object",
            Transform parent = null,
            Type[] components = null)
        {
            return BuildGameObject(name, parent, components);
        }

        public static T CreateGameObject<T>(
            string name = "Game Object",
            Transform parent = null,
            Type[] components = null) where T : Component
        {
            return AddOrGetComponent<T>(BuildGameObject(name, parent, components));
        }

        /*
         * CreateThenDestroyGameObject — creates, runs the callback, then destroys.
         * Net scene root-object change is zero.
         *
         *   TestUtilities.CreateThenDestroyGameObject(obj => { ... });
         *   TestUtilities.CreateThenDestroyGameObject(obj => { ... }, name: "Player");
         *   TestUtilities.CreateThenDestroyGameObject<Camera>(cam => { ... }, name: "Cam");
         *   TestUtilities.CreateThenDestroyGameObject(obj => { ... },
         *       components: new[] { typeof(Rigidbody2D) });
         */

        public static void CreateThenDestroyGameObject(
            Action<GameObject> func,
            string name = "Game Object",
            Transform parent = null,
            Type[] components = null)
        {
            GameObject obj = BuildGameObject(name, parent, components);
            func(obj);
            GameObject.DestroyImmediate(obj);
        }

        public static void CreateThenDestroyGameObject<T>(
            Action<T> func,
            string name = "Game Object",
            Transform parent = null,
            Type[] components = null) where T : Component
        {
            GameObject obj = BuildGameObject(name, parent, components);
            func(AddOrGetComponent<T>(obj));
            GameObject.DestroyImmediate(obj);
        }

        /*
         * Coroutine helpers — require PlayMode tests (UnityTest attribute).
         *
         *   [UnityTest]
         *   public IEnumerator MyTest() {
         *       yield return TestUtilities.WaitForFrames(3);
         *       yield return TestUtilities.WaitForSeconds(0.5f);
         *   }
         */

        public static IEnumerator WaitForFrames(int frames) {
            for (int i = 0; i < frames; i++) {
                yield return null;
            }
        }

        public static IEnumerator WaitForSeconds(float seconds) {
            float elapsed = 0f;
            while (elapsed < seconds) {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
