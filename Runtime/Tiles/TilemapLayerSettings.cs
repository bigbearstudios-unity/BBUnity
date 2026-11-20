using System;

using UnityEngine;

namespace BBUnity.Tiles {

    public enum TilemapCollisionMode {
        None,
        Solid,
        OneWay
    }

    /// <summary>How one tilemap of a painted level is set up: where it draws and what it collides with.</summary>
    [Serializable]
    public class TilemapLayerSettings {

        [SerializeField]
        private string _name = "Ground";

        [SerializeField, Tooltip("Sorting layer for the tilemap renderer. The default layer is used if blank.")]
        private string _sortingLayerName;

        [SerializeField]
        private int _sortingOrder;

        [SerializeField, Tooltip("The physics layer of the tilemap object.")]
        private int _physicsLayer;

        [SerializeField]
        private TilemapCollisionMode _collision = TilemapCollisionMode.Solid;

        public string Name {
            get { return _name; }
        }

        public string SortingLayerName {
            get { return _sortingLayerName; }
        }

        public int SortingOrder {
            get { return _sortingOrder; }
        }

        public int PhysicsLayer {
            get { return _physicsLayer; }
        }

        public TilemapCollisionMode Collision {
            get { return _collision; }
        }

        public TilemapLayerSettings() { }

        public TilemapLayerSettings(string name, TilemapCollisionMode collision, int sortingOrder = 0, int physicsLayer = 0, string sortingLayerName = null) {
            _name = name;
            _collision = collision;
            _sortingOrder = sortingOrder;
            _physicsLayer = physicsLayer;
            _sortingLayerName = sortingLayerName;
        }
    }
}
