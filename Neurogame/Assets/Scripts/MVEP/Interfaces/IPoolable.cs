using UnityEngine;
public interface IPoolable<T> where T : Component, IPoolable<T>
{
  void SetPool(ObjectPool<T> pool);
  void ReturnToPool();
}
