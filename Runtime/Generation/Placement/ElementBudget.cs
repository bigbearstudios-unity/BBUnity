using System;

using UnityEngine;

using BBUnity.Generation.Parameters;

namespace BBUnity.Generation.Placement {

    /// <summary>"Place this many of this" — a count that can be fixed or a range.</summary>
    [Serializable]
    public class ElementBudget {

        [SerializeField]
        private PlaceableDefinition _placeable;

        [SerializeField]
        private IntParameter _count = IntParameter.Fixed(1);

        public PlaceableDefinition Placeable {
            get { return _placeable; }
        }

        public IntParameter Count {
            get { return _count; }
        }

        public ElementBudget() { }

        public ElementBudget(PlaceableDefinition placeable, IntParameter count) {
            _placeable = placeable;
            _count = count;
        }
    }
}
