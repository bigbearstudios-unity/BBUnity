# Utilities

```csharp
using BBUnity.Utilities;
```

## GameObjectFactory

Helpers for constructing `GameObject`s with sensible defaults (positioned at the origin/parent's
local origin, uniform scale), and for pulling a component off a freshly-instantiated prefab.

```csharp
GameObject obj = GameObjectFactory.Create("Enemy");
GameObject child = GameObjectFactory.Create("Weapon", parent: obj.transform);
GameObject withComponents = GameObjectFactory.Create("Trigger", components: new[] { typeof(BoxCollider) });

PoolBehavior instance = GameObjectFactory.InstantiatedGameObjectWithComponent<PoolBehavior>(prefab);
```

### AddOrGetComponent

Returns the existing component of type `T` if present, otherwise adds one. Previously an
extension method (`BBUnity.Extensions.GameObjectExtensions.AddOrGetComponent`) — moved onto
`GameObjectFactory` directly since it was its only real caller (`InstantiatedGameObjectWithComponent`
uses it internally), so there was no need for it to be a separate reusable extension.

```csharp
BoxCollider collider = GameObjectFactory.AddOrGetComponent<BoxCollider>(gameObject);
```
