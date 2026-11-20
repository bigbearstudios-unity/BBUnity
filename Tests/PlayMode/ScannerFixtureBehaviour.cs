using UnityEngine;

namespace BBUnity.Editor.Support.Validation.Tests {

    /// <summary>Lives in the PlayMode test assembly (not editor-only) because an editor-only script cannot be added to a prefab. A component whose script GUID the tests break, to prove the scanner notices a missing script.</summary>
    public class ScannerFixtureBehaviour : MonoBehaviour { }
}
