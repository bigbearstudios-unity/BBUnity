using System;

using UnityEngine;

namespace BBUnity.Editor.Support.Validation.Tests {

    [Serializable]
    public class ScannerFixtureBase { }

    [Serializable]
    public class ScannerFixtureImpl : ScannerFixtureBase {
        public int Value = 1;
    }

    /// <summary>An asset with a [SerializeReference] entry, used to prove the scanner notices a removed type.</summary>
    public class ScannerFixtureAsset : ScriptableObject {

        [SerializeReference]
        public ScannerFixtureBase Reference = new ScannerFixtureImpl();
    }
}
