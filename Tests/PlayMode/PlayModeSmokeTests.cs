using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BBUnity.Entities.Building;
using BBUnity.Entities;


namespace BBUnity.Smoke.Tests {

    /// <summary>
    /// Builds a tiny scene in code (a camera, ground and one entity built from an EntityDefinition), lets it
    /// run for five seconds and fails if anything logs an error. Independent of every game asset, so it
    /// guards the library on its own. Later items extend it as new systems arrive.
    /// </summary>
    public class PlayModeSmokeTests {

        private const float RunSeconds = 5f;

        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();
        private readonly List<string> _errors = new List<string>();

        [TearDown]
        public void TearDown() {
            Application.logMessageReceived -= OnLog;
            foreach (UnityEngine.Object created in _created) {
                if (created != null) { UnityEngine.Object.Destroy(created); }
            }
            _created.Clear();
            _errors.Clear();
        }

        [UnityTest]
        public IEnumerator EntityOnGround_RunsFiveSeconds_LogsNoErrorsAndComesToRest() {
            Assume.That(Application.isPlaying, "The smoke test needs physics, which only simulates in PlayMode.");

            Application.logMessageReceived += OnLog;

            Track(new GameObject("Camera", typeof(Camera))).tag = "MainCamera";

            GameObject ground = Track(new GameObject("Ground"));
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.AddComponent<BoxCollider2D>().size = new Vector2(20f, 1f);

            GameObject entity = Track(new GameObject("Entity"));
            entity.transform.position = new Vector3(0f, 3f, 0f);
            EntityBuilder.Build(entity, CreateDefinition(typeof(Entity2D), typeof(Rigidbody2D), typeof(BoxCollider2D)));

            Assert.IsNotNull(entity.GetComponent<Entity2D>(), "The definition did not build an Entity2D.");

            float end = Time.realtimeSinceStartup + RunSeconds;
            while (Time.realtimeSinceStartup < end) { yield return null; }

            Assert.IsEmpty(_errors, "Errors were logged during the run:\n" + string.Join("\n", _errors));
            Assert.AreEqual(0.5f, entity.transform.position.y, 0.1f, "The entity did not fall onto the ground and rest there.");
        }

        private void OnLog(string message, string stackTrace, LogType type) {
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) { _errors.Add(message); }
        }

        private T Track<T>(T created) where T : UnityEngine.Object {
            _created.Add(created);
            return created;
        }

        /// <summary>An EntityDefinition whose own component list adds each type. The fields are private, so they are set by reflection.</summary>
        private EntityDefinition CreateDefinition(params Type[] componentTypes) {
            var definition = Track(ScriptableObject.CreateInstance<EntityDefinition>());

            var specs = new List<ComponentSpec>();
            foreach (Type type in componentTypes) {
                var reference = new ComponentTypeReference();
                SetField(reference, "_typeName", type.AssemblyQualifiedName);

                var spec = new ComponentSpec();
                SetField(spec, "_type", reference);
                specs.Add(spec);
            }

            SetField(definition, "_components", specs);
            return definition;
        }

        private static void SetField(object target, string name, object value) {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(field, $"{target.GetType().Name} has no field '{name}'.");
            field.SetValue(target, value);
        }
    }
}
