using System;
using System.Collections.Generic;
using UnityEngine;
using BBUnity.Entities.Building;


namespace BBUnity.Core {

    /// <summary>
    /// Project-wide BBUnity configuration. Must live at Assets/Resources/BBUnitySettings.asset —
    /// same reasoning as BootSettings, loaded via Resources so it's reachable with nothing else
    /// set up yet. Currently holds:
    ///
    /// - ProjectType: whether this project is 2D or 3D. Nothing reads this yet, but it's the
    ///   natural place to eventually gate which of Entity2D/Entity3D's building tools even apply.
    /// - Entities: everything to do with the entity-building system, grouped in one section
    ///   (EntitySettings). Currently AdditionalComponentTypes: Unity's own built-in components (Rigidbody2D, BoxCollider2D,
    ///   ...) that should appear in the entity-building system's Component Type dropdown. BBUnity
    ///   never appears in this specific list — it's exclusively for the Unity components a
    ///   project opts into, since BBUnity's own types are already covered automatically (see
    ///   ComponentTypeReferenceDrawer, Editor: every EntityController subclass and everything
    ///   explicitly marked [CapabilityComponent]).
    /// - ComponentManagement: the on/off switch (and later, options) for the Inspector's
    ///   component sub categories / hidden components tooling.
    /// </summary>
    [CreateAssetMenu(fileName = "BBUnitySettings", menuName = "BBUnity/Core/BBUnity Settings")]
    public class BBUnitySettings : ScriptableObject {

        public enum ProjectDimension {
            [InspectorName("2D")]
            TwoDimensional,

            [InspectorName("3D")]
            ThreeDimensional
        }

        /// <summary>
        /// The Entities section of the settings — everything that configures the entity-building
        /// system lives here so it shows as one foldout in the Inspector and new entity options
        /// have an obvious home.
        /// </summary>
        [Serializable]
        public class EntitySettings {

            [SerializeField, Tooltip("Unity's own built-in component types to offer in the entity-building system's Component Type dropdown.")]
            private List<UnityComponentTypeReference> _additionalComponentTypes = new List<UnityComponentTypeReference>();

            public IReadOnlyList<UnityComponentTypeReference> AdditionalComponentTypes {
                get { return _additionalComponentTypes; }
            }
        }

        /// <summary>
        /// Inspector tooling that groups a GameObject's components into sub categories by rules
        /// (a preset of component type -> category rules, edited in Window > BBUnity > Component
        /// Presets). More options for it will be added here as the tooling grows.
        /// </summary>
        [Serializable]
        public class ComponentManagementSettings {

            [SerializeField, Tooltip("Turns the component management tools in the Inspector on or off. When off, the Inspector looks exactly as Unity draws it.")]
            private bool _enabled = false;

            [SerializeField, Tooltip("Which preset of category rules to use. Empty is the built-in BBUnity preset; otherwise the GUID of a preset asset. Chosen from the dropdown, not typed.")]
            private string _preset = string.Empty;

            public bool Enabled {
                get { return _enabled; }
            }

            public string Preset {
                get { return _preset; }
            }
        }

        private const string ResourcePath = "BBUnitySettings";

        [SerializeField]
        private ProjectDimension _projectType = ProjectDimension.TwoDimensional;

        [SerializeField]
        private EntitySettings _entities = new EntitySettings();

        [SerializeField]
        private ComponentManagementSettings _componentManagement = new ComponentManagementSettings();

        public ProjectDimension ProjectType {
            get { return _projectType; }
        }

        public EntitySettings Entities {
            get { return _entities; }
        }

        public ComponentManagementSettings ComponentManagement {
            get { return _componentManagement; }
        }

        public static BBUnitySettings Load() {
            return Resources.Load<BBUnitySettings>(ResourcePath);
        }
    }
}
