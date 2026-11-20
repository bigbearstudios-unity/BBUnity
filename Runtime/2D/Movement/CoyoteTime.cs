using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBUnity.Movement {

    [System.Serializable]
    /// <summary>
    /// A serializable CoyoteTime class which has the following functionality:
    /// - Allows the resetting of the timer
    /// - Allows access to the 'IsAvailable' boolean value
    /// </summary>
    public class CoyoteTime {

        [SerializeField, Tooltip("The threshold for Coyote time. This is the amount of time, in milliseconds, in which coyote time is available.")] 
        private int _thresholdTime = 100;

        private bool _isAvailable = false;
        private bool _timerStarted = false;
        private float _startedAt;

        public CoyoteTime() {
            Reset();
        }

        /// <summary>
        /// Resets the CoyoteTime, this sets available to false and clears the timer
        /// </summary>
        public void Reset() {
            _isAvailable = false;
            _timerStarted = false;
            _startedAt = 0.0f;
        }

        /// <summary>
        /// Starts the CoyoteTimer
        /// </summary>
        public void StartTimer() {
            _startedAt = Time.time;
            _timerStarted = true;
        }

        /// <summary>
        /// Sets the CoyoteTimer to available
        /// </summary>
        /// <param name="available"></param>
        public void SetAvailable(bool available) {
            _isAvailable = available;
        }

        /// <summary>
        /// Returns a boolean value if the CoyoteTime is available
        /// </summary>
        /// <value></value>
        public bool IsAvailable {
           get { return _isAvailable && _timerStarted && (Time.time - _startedAt) * 1000f < _thresholdTime; }
        }
    }   
}
