using UnityEngine;

/// <summary>
/// Represents an obstacle object in the game world.
/// Obstacles are pooled for efficient reuse and can be collected/destroyed.
/// When hit by the canoe, triggers a penalty event.
/// </summary>
public class Obstacle : MonoBehaviour, IPoolable<Obstacle>
{
  // References
  private ObjectPool<Obstacle> pool;

  [SerializeField] private AudioSource collisionSound;

  /// <summary>
  /// Sets the object pool that manages this obstacle's lifecycle.
  /// Called automatically by the ObjectPool when this obstacle is created.
  /// </summary>
  /// <param name="pool">The object pool managing this obstacle.</param>
  public void SetPool(ObjectPool<Obstacle> pool)
  {
    this.pool = pool;
  }

  /// <summary>
  /// Returns this obstacle to the object pool for reuse.
  /// Should be called when the obstacle is no longer needed or has been collected.
  /// </summary>
  public void ReturnToPool()
  {
    if (pool != null)
    {
      pool.Return(this);
    }
    else
    {
      Debug.LogWarning("Obstacle: Attempted to return to pool, but pool reference is null. Destroying instead.");
      Destroy(gameObject);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Canoe"))
    {
      if (collisionSound != null)
      {
        collisionSound.Play();
      }
    }
  }
}
