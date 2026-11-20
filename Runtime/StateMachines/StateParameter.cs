using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.StateMachines {

    /// <summary>
    /// A key-state pair used to register a single state with a <see cref="StateMachine"/>.
    /// </summary>
    sealed public class StateParameter {
        private string _key;
        private State _state;

        public string Key { get { return _key; } }
        public State State { get { return _state; } }

        public StateParameter(string key, State state) {
            if(key == null) throw new System.ArgumentNullException("key");
            if(state == null) throw new System.ArgumentNullException("state");

            _key = key;
            _state = state;
        }
    }
}
