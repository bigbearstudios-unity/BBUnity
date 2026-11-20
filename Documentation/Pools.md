# Pools

A lightweight object pooling system built around three types:

- **`ObjectPool`** — a `MonoBehaviour` you attach to a scene GameObject; holds one or more named `ObjectPoolReference`s and exposes `Spawn(name)` as the main entry point.
- **`ObjectPoolReference`** — configuration and runtime state for a single pooled prefab: name, prefab, starting size, maximum size, and its live instances.
- **`PoolBehavior`** — the component automatically added to every instance a pool creates. Provides `OnCreate`/`OnSpawn`/`OnReturn` (as overridable virtual methods, matching events, or the `IPoolBehavior` interface) and `ReturnToPool()`.

```csharp
using BBUnity.Pools;
```

## Basic Usage

Add an `ObjectPool` component to a GameObject (menu: `BBUnity/Pools/Object Pool`), then add one or more pool references either via the inspector list or in code:

```csharp
ObjectPool pool = GetComponent<ObjectPool>();
pool.AddPoolReference(new ObjectPoolReference("Enemy", enemyPrefab, startingSize: 5, maximumSize: 20));
```

Spawn by name from the pool:

```csharp
PoolBehavior instance = pool.Spawn("Enemy");
```

`Spawn` returns `null` if the name isn't registered, or if the pool is already at `MaximumSize` with no inactive instance to reuse — always null-check the result before using it.

You can also spawn directly from a reference if you already have one (e.g. via `pool.FindPoolReference("Enemy")`):

```csharp
ObjectPoolReference reference = pool.FindPoolReference("Enemy");
PoolBehavior instance = reference.Spawn();
```

### Finding a pool in the scene

```csharp
ObjectPool pool = ObjectPool.FindInScene("Enemy Pool");
```

Returns `null` if no `ObjectPool` GameObject with that name exists — a common pattern is to fall back to creating one:

```csharp
ObjectPool pool = ObjectPool.FindInScene("Enemy Pool") ?? CreateEnemyPool();
```

## Returning instances to the pool

Call `ReturnToPool()` on the spawned `PoolBehavior` when you're done with it, rather than deactivating the GameObject directly — this runs the `OnReturn` callbacks first:

```csharp
instance.ReturnToPool();
```

To return every currently-active instance from a reference at once (e.g. on a scene transition):

```csharp
reference.ClearAll();
```

## Callbacks

There are two ways to hook into a pooled instance's lifecycle — pick based on who owns the code:

- **`IPoolBehavior`** — implement this on a component that lives on the pooled prefab itself. It's discovered automatically via `GetComponents<IPoolBehavior>()` when the instance is created, no manual wiring needed.
- **Events (`Created`, `Spawned`, `Returned`)** — subscribe from external code that calls `Spawn()`/`ReturnToPool()` but doesn't own the prefab.

```csharp
public class EnemyPoolBehavior : PoolBehavior, IPoolBehavior {
    public void OnCreate(PoolBehavior behavior) { /* one-time setup */ }
    public void OnSpawn(PoolBehavior behavior)  { /* reset state each spawn */ }
    public void OnReturn(PoolBehavior behavior) { /* cleanup before returning */ }
}
```

```csharp
PoolBehavior instance = pool.Spawn("Enemy");
instance.Spawned += (spawned) => { /* external caller reacting to this specific spawn */ };
```

`PoolBehavior.AddOnCreateEvent(behavior, handler)` / `AddOnSpawnEvent(behavior, handler)` are convenience statics for subscribing from a `MonoBehaviour` reference that isn't already typed as `PoolBehavior` — both throw a `MissingComponentException` if the target GameObject doesn't have a `PoolBehavior` component.

## Pool statistics

`ObjectPoolReference` exposes read-only stats useful for tuning `StartingSize`/`MaximumSize` during development:

```csharp
reference.NumberOfInstances;        // total instances ever created for this reference
reference.ActiveCount;               // currently spawned
reference.InactiveCount;             // currently available to spawn
reference.HasGrownBeyondStartingSize; // true if StartingSize was set too low
```

## Best Practices

- Always null-check the result of `Spawn()` — a full, non-growing pool returns `null` rather than throwing.
- Set `MaximumSize` to a real positive value. `0` or negative disables growth entirely (logged as a warning in the editor via `OnValidate`).
- Prefer sizing `StartingSize` close to your real peak usage — check `HasGrownBeyondStartingSize` during testing rather than guessing.
- Use `ReturnToPool()` instead of calling `gameObject.SetActive(false)` directly, so `OnReturn` callbacks still run.
