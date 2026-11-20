# UI

```csharp
using BBUnity.UI.Components;
using BBUnity.UI.Utilities;
```

## UIBackgroundGradient

A UI mesh effect that applies a two-color linear gradient to a `Graphic` by modifying its vertex
colors. The gradient color is multiplied into the Graphic's existing vertex color (rather than
replacing it), so it composes with the Graphic's own tint (`Graphic.color`) instead of overwriting it.

### What it can be added to

`UIBackgroundGradient` requires a `UnityEngine.UI.Graphic` component on the same GameObject —
`Image`, `RawImage`, `Text`, `TextMeshProUGUI`, or any other `Graphic` subclass. It is **not** tied
to any particular kind of UI element ("panel" or otherwise) — a mesh effect only runs when the
Graphic it's attached to rebuilds its mesh and asks its `IMeshModifier`s to run, so without a
Graphic present `ModifyMesh` is simply never called.

This is enforced with `[RequireComponent(typeof(Graphic))]` — adding `UIBackgroundGradient` via the
Add Component menu will also add an `Image` if no Graphic is already present, and Unity will refuse
to remove the Graphic while the gradient is still attached.

```csharp
Image image = gameObject.AddComponent<Image>();
UIBackgroundGradient gradient = gameObject.AddComponent<UIBackgroundGradient>();
gradient.Color1 = Color.white; // top
gradient.Color2 = Color.black; // bottom
gradient.Angle = 0f;           // 0 = bottom-to-top
```

## ColorGradient

The math helpers `UIBackgroundGradient` is built on — axis-agnostic linear gradient sampling over a
`Graphic`'s local rect space, independent of Unity's UI types. See `ColorGradient.LocalPositionMatrix`,
`RotationDir`, and `CompensateAspectRatio` for the individual pieces if you need a custom mesh effect
with similar gradient math.
