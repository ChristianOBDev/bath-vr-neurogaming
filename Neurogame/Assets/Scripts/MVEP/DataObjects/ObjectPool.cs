using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component, IPoolable<T>
{
  private readonly T prefab;
  private readonly Transform parent;
  private readonly Queue<T> pool = new();

  public ObjectPool(T prefab, int initialSize, Transform parent = null)
  {
    this.prefab = prefab;
    this.parent = parent;

    for (int i = 0; i < initialSize; i++)
    {
      CreateInstance();
    }
  }

  private T CreateInstance()
  {
    T instance = Object.Instantiate(prefab, parent);
    instance.gameObject.SetActive(false);

    instance.SetPool(this);

    pool.Enqueue(instance);
    return instance;
  }

  public T Get()
  {
    if (pool.Count == 0)
      CreateInstance();

    T instance = pool.Dequeue();
    instance.gameObject.SetActive(true);
    return instance;
  }

  public void Return(T instance)
  {
    instance.gameObject.SetActive(false);
    pool.Enqueue(instance);
  }
}
