using UnityEngine;

public class Obstacle : MonoBehaviour, IPoolable<Obstacle>
{
  private ObjectPool<Obstacle> pool;

  public void SetPool(ObjectPool<Obstacle> pool)
  {
    this.pool = pool;
  }

  public void ReturnToPool()
  {
    pool.Return(this);
  }
}
