using UnityEngine;
using BBUnity.Entities.Building;


namespace BBUnity.Entities {

    /// <summary>
    /// The base class for all Entities — never added directly (see Entity2D/Entity3D for the
    /// concrete, addable variants). Abstract rather than merely conventionally-base-class, so
    /// Unity itself keeps it out of the Add Component menu and off the "new" surface.
    /// </summary>
    public abstract class Entity : MonoBehaviour {

        [SerializeField, Tooltip("The EntityDefinition this entity was last built from, if any — lets the entity-building inspector show what's here and rebuild after the preset changes, rather than forgetting the moment the Inspector closes.")]
        private EntityDefinition _sourcePreset;

        public EntityDefinition SourceDefinition {
            get { return _sourcePreset; }
            set { _sourcePreset = value; }
        }

        /*
         * The lifecycle functions which are called for each entity
         * Note - These are not your standard Unity calls, but are called
         * as part of those methods which are used by the base Characters
         */
        // protected virtual void OnAwake() { }
        // protected virtual void OnStart() { }
        // protected virtual void OnUpdate() { }
    }
}