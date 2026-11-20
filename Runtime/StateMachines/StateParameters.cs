using System.Collections.Generic;
using UnityEngine;

namespace BBUnity.StateMachines {

    /// <summary>
    /// A collection of <see cref="StateParameter"/> entries used to register multiple states
    /// with a <see cref="StateMachine"/> in one call, either via the constructor or
    /// <see cref="StateMachine.AddStates"/>.
    /// </summary>
    public class StateParameters : List<StateParameter> {
        public void Add(string key, State state) {
            if(Find(p => p.Key == key) != null) {
                Debug.LogWarning($"BBUnity.StateMachines.StateParameters - A state with the key '{key}' has already been added. The duplicate will be ignored.");
                return;
            }

            Add(new StateParameter(key, state));
        }
    }
}