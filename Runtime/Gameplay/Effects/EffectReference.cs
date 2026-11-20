using UnityEngine;

namespace BBUnity.Gameplay.Effects {

    [System.Serializable]
    public class EffectReference {

        [SerializeField]
        private string _name;
        public string Name { get { return _name; } }

        [SerializeField]
        private GameObject _prefab;
        public GameObject Prefab { get { return _prefab; } }

        [SerializeField]
        private Transform _position;
        public Transform Position { get { return _position; } }
    }
}
