namespace BBUnity.Pools {

    /// <summary>
    /// Implement this on a component living on the pooled prefab itself — it's discovered and
    /// wired up automatically by PoolBehavior, no manual subscription needed. If the calling
    /// code doesn't own the prefab (e.g. it just called ObjectPool.Spawn() from elsewhere),
    /// use PoolBehavior's Created/Spawned/Returned instead.
    /// </summary>
    public interface IPoolBehavior {
        void OnCreate(PoolBehavior behavior);
        void OnSpawn(PoolBehavior behavior);
        void OnReturn(PoolBehavior behavior);
    }
}
