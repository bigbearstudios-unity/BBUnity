# EditorAttributes

```csharp
using BBUnity.EditorAttributes;
```

Property attributes for customizing how a serialized field draws in the Inspector. Each is a
plain marker (`PropertyAttribute`) in `Runtime/EditorAttributes`; the actual `PropertyDrawer` for
each lives in `Editor/EditorAttributes` under `BBUnity.Editor.EditorAttributes`, since drawers
are editor-only.

## LayerAttribute

Draws an `int` field as Unity's built-in layer picker instead of a plain number field.

```csharp
[Layer]
[SerializeField] private int _layer;
```

## SortingLayerAttribute

Draws an `int` field as Unity's built-in sorting layer picker. Falls back to a plain int field
(with a logged error) if Unity's internal `EditorGUI.SortingLayerField` API is ever renamed or
removed in a future Unity version.

```csharp
[SortingLayer]
[SerializeField] private int _sortingLayer;
```

## ReadOnlyAttribute

Draws a field as disabled (visible, but not editable) in the Inspector.

```csharp
[ReadOnly]
[SerializeField] private string _computedValue;
```
