using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for reusing component instances to reduce garbage collection overhead.
/// Works with any component type that implements IPoolable&lt;T&gt;.
/// </summary>
/// <typeparam name="T">The component type to pool. Must implement IPoolable&lt;T&gt;.</typeparam>
public class ObjectPool<T> where T : Component, IPoolable<T>
{
  // Configuration
  private readonly T prefab;
  private readonly Transform parent;

  // State
  private readonly Queue<T> pool = new();

  /// <summary>
  /// Initializes the object pool with pre-allocated instances.
  /// </summary>
  /// <param name="prefab">The component prefab to instantiate for the pool.</param>
  /// <param name="initialSize">Number of instances to pre-allocate.</param>
  /// <param name="parent">Optional transform parent for instantiated objects.</param>
  public ObjectPool(T prefab, int initialSize, Transform parent = null)
  {
    if (prefab == null)
    {
      Debug.LogError($"ObjectPool<{typeof(T).Name}>: Prefab cannot be null");
      return;
    }

    if (initialSize < 0)
    {
      Debug.LogError($"ObjectPool<{typeof(T).Name}>: Initial size cannot be negative");
      initialSize = 0;
    }

    this.prefab = prefab;
    this.parent = parent;

    for (int i = 0; i < initialSize; i++)
    {
      CreateInstance();
    }
  }

  /// <summary>
  /// Gets an active instance from the pool, creating a new one if necessary.
  /// </summary>
  /// <returns>An active pooled instance, or null if prefab is invalid.</returns>
  public T Get()
  {
    if (prefab == null)
    {
      Debug.LogError($"ObjectPool<{typeof(T).Name}>: Cannot get instance - prefab is null");
      return null;
    }

    if (pool.Count == 0)
      CreateInstance();

    T instance = pool.Dequeue();
    instance.gameObject.SetActive(true);
    return instance;
  }

  /// <summary>
  /// Returns an instance to the pool for reuse.
  /// </summary>
  /// <param name="instance">The instance to return to the pool.</param>
  public void Return(T instance)
  {
    if (instance == null)
    {
      Debug.LogError($"ObjectPool<{typeof(T).Name}>: Cannot return null instance to pool");
      return;
    }

    instance.gameObject.SetActive(false);
    pool.Enqueue(instance);
  }

  /// <summary>
  /// Creates a new instance and adds it to the pool.
  /// </summary>
  /// <returns>The newly created instance.</returns>
  private T CreateInstance()
  {
    T instance = Object.Instantiate(prefab, parent);
    instance.gameObject.SetActive(false);
    instance.SetPool(this);
    pool.Enqueue(instance);
    return instance;
  }

  /// <summary>
  /// Gets the current number of available instances in the pool.
  /// Useful for debugging and monitoring pool health.
  /// </summary>
  public int AvailableCount => pool.Count;
}
