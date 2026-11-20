# BBUnity
A Unity library to aid with games development.

See [`/Documentation`](Documentation) for per-namespace documentation — e.g.
[`Documentation/Conversion.md`](Documentation/Conversion.md) documents the `BBUnity.Support.Conversion` namespace.

## Conventions

- **Namespaces follow folders.** `Runtime/Audio` is `BBUnity.Audio`. A namespace is `BBUnity.<Area>` when every game type can use it, and `BBUnity.<Pack>.<Area>` when it belongs to one game-type pack (`BBUnity.Platformer.Generation`). There is no `Gameplay` prefix, and namespaces stay light: a sub-namespace is added only when a type needs one. A type
  only used by its neighbors and not part of the public surface goes in an `Internal` namespace,
  and exceptions in an `Exceptions` namespace. An assembly other than Core has its own folder under `Runtime` (`Runtime/2D`, `Runtime/Platformer`, `Runtime/Generation`), and inside it the folders follow the namespaces (`Runtime/2D/Tiles` is `BBUnity.Tiles`). A dimension's types keep the shared feature namespace. One type per file, except a generic/non-generic pair.
- **Editor code lives under `BBUnity.Editor.*`**, mirroring the runtime namespace it edits
  (`BBUnity.Audio` → `BBUnity.Editor.Audio`). Custom inspectors end in `Editor`,
  property drawers end in `Drawer`. Standalone tools (`EditorWindow`s) live in `BBUnity.Editor.Windows`, since they are not tied to one runtime type.
  Editor UI is written in **UI Toolkit** (`CreateInspectorGUI` / `CreatePropertyGUI` / `CreateGUI`), building on `BBUnity.Editor.Toolkit`; don't add IMGUI (`OnInspectorGUI`, `OnGUI`) to new editor code.
- **Menu paths follow the namespace.** A component is `BBUnity/<namespace without BBUnity>/<Class Name>`
  in the Add Component menu, and ScriptableObjects use the same path in the Create menu
  (`BBUnity.Audio.SoundDefinition` → `BBUnity/Audio/Sound Definition`).
- **ScriptableObject concepts.** A `…Settings` asset configures a system (`BBUnitySettings`,
  `BootSettings`). A `…Definition` asset is data that defines a thing, or a reusable block a thing is
  defined from (`SoundDefinition`, `CombatantDefinition`, `EntityCapabilityDefinition`).
- **Events** are named for what happened, in the past tense, with no `On` prefix (`Hit`, `StateChanged`). `On…` is for the method that raises or handles one.
- **Spelling** is American in code, comments and docs (`Initialize`, `PoolBehavior`, `Specialized`).
- **Enums** are singular (`ReductionKind`), plural only when `[Flags]`.
- **Exceptions** end in `Exception`; helper statics are nouns (`Validator`, `GameObjectFactory`).
- **Support** (`BBUnity.Support.*`) is game-agnostic infrastructure — conversion, validation, scenes.
  **Vendor** (`BBUnity.Vendor.*`) is integration with third-party packages (Cinemachine), each in
  its own assembly that only compiles when that package is present.
