# TestSupport

Shared test utility library for BBUnity packages. Provides fixture creation,
GameObject lifecycle helpers, and assertion extensions for Unity-specific types.

## Setup

```csharp
using BBUnity.Support.Testing;
```

---

## Fixtures

Creates lightweight assets in-memory — no file loading required.

```csharp
Fixtures.Sprite();              // 0x0 sprite
Fixtures.Sprite(500, 500);      // 500x500 sprite

Fixtures.Sprites(5);            // array of 5 x 0x0 sprites
Fixtures.Sprites(5, 500, 500);  // array of 5 x 500x500 sprites
```

---

## TestUtilities

### Scene helpers

```csharp
// Number of root objects in the active scene (no allocation)
int count = TestUtilities.NumberOfRootObjectsInScene;

// Destroy all root objects (useful in [TearDown])
TestUtilities.DestroyRootObjectsInScene();
```

### CreateGameObject

Creates a GameObject; the caller is responsible for destroying it.

```csharp
GameObject obj = TestUtilities.CreateGameObject();
GameObject obj = TestUtilities.CreateGameObject(name: "Player");
Camera     cam = TestUtilities.CreateGameObject<Camera>(name: "Cam");

// With extra components
GameObject obj = TestUtilities.CreateGameObject(
    components: new[] { typeof(Rigidbody2D) });
```

### CreateThenDestroyGameObject

Creates a GameObject, passes it to the callback, then destroys it. Net scene
change is zero.

```csharp
TestUtilities.CreateThenDestroyGameObject(obj => { ... });
TestUtilities.CreateThenDestroyGameObject(obj => { ... }, name: "Player");
TestUtilities.CreateThenDestroyGameObject(obj => { ... },
    components: new[] { typeof(Rigidbody2D) });

// Generic variant — callback receives the requested component
TestUtilities.CreateThenDestroyGameObject<Rigidbody2D>(rb => { ... });
TestUtilities.CreateThenDestroyGameObject<Rigidbody2D>(rb => { ... },
    name: "Physics Object");
```

### Coroutine helpers (PlayMode only)

These must be used inside `[UnityTest]` methods.

```csharp
[UnityTest]
public IEnumerator MyTest() {
    yield return TestUtilities.WaitForFrames(3);
    yield return TestUtilities.WaitForSeconds(0.5f);
}
```

---

## UnityAssert

### Scene root object count

```csharp
// Asserts the scene root object count changed by exactly n
UnityAssert.ChangeInSceneRootObjects(1, () => {
    new GameObject();
});
```

### Floating-point tolerant equality

Default tolerance is `0.0001f` for all overloads.

```csharp
UnityAssert.AreEqual(Vector2.zero, Vector2.zero);
UnityAssert.AreEqual(Vector3.zero, Vector3.zero);
UnityAssert.AreEqual(0.0f, 0.0f);
UnityAssert.AreEqual(Quaternion.identity, Quaternion.identity);

// Custom tolerance
UnityAssert.AreEqual(Vector3.zero, Vector3.zero, tolerance: 0.01f);
```

### Array assertions

Assert a predicate holds for every element. Halts at the first failure —
subsequent failures in the same array are not reported in the same test run
(standard NUnit behavior).

```csharp
UnityAssert.IsTrue(array,    v => v.IsActive);
UnityAssert.IsFalse(array,   v => v.IsDestroyed);
UnityAssert.IsNotNull(array, v => v.Renderer);
UnityAssert.IsNull(array,    v => v.Parent);
```

### Dictionary assertions

```csharp
UnityAssert.AreEqual(new Dictionary<int, string> {
    { 1, "I" }, { 5, "V" }, { 10, "X" }
}, NumberTo.RomanNumeral);
```

### Callback invocation count

```csharp
// Exact count
UnityAssert.IsCalled(3, called => {
    called(); called(); called();
});

// At least n times
UnityAssert.IsCalledAtLeast(2, called => {
    for (int i = 0; i < 5; i++) called();
});

// At most n times
UnityAssert.IsCalledAtMost(5, called => {
    called(); called();
});
```
