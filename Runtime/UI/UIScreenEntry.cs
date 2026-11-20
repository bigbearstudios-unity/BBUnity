using UnityEngine;
using System;
using System.Collections.Generic;
using BBUnity.UI.Transitions;

namespace BBUnity.UI {

    /// <summary>
    /// One entry in a UIScreenController's screen list — a UIScreen looked up by a
    /// designer-chosen name rather than a hand-written field per screen.
    /// </summary>
    [Serializable]
    public class UIScreenEntry {

        [SerializeField]
        private string _name;

        [SerializeField]
        private UIScreen _screen;

        public string Name {
            get { return _name; }
        }

        public UIScreen Screen {
            get { return _screen; }
        }
    }
}
