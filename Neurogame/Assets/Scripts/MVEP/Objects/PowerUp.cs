using UnityEngine;

public class PowerUp : MonoBehaviour, IPoolable<PowerUp>
{
  private ObjectPool<PowerUp> pool;

  public void SetPool(ObjectPool<PowerUp> pool)
  {
    this.pool = pool;
  }

  public void ReturnToPool()
  {
    pool.Return(this);
  }
}
