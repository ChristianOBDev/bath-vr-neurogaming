using UnityEngine;

/// <summary>
/// Represents a collectible power-up object in the game.
/// Displays a bobbing, spinning crystal that animates when collected.
/// Notifies the event system when collected by the player.
/// </summary>
public class PowerUp : MonoBehaviour, IPoolable<PowerUp>
{
  // Constants
  private const float CRYSTAL_INITIAL_SCALE = 1.4f;
  private const float COLLECTION_SCALE_DURATION = 0.25f;

  // Configuration - Animation
  [SerializeField] private float bobbingAmplitude = 0.5f;
  [SerializeField] private float bobbingFrequency = 1f;
  [SerializeField] private float rotationSpeed = 30f;

  // Configuration - References
  [SerializeField] private GameObject crystal;

  // State - Lifecycle
  private bool bobbing = true;

  // State - Position
  private Vector3 initialPosition;

  // References
  private ObjectPool<PowerUp> pool;

  /// <summary>
  /// Initializes the power-up with stored initial position and scale.
  /// </summary>
  private void Start()
  {
    if (crystal == null)
    {
      Debug.LogError("PowerUp: Crystal object not assigned!");
      return;
    }

    initialPosition = crystal.transform.position;
  }

  /// <summary>
  /// Sets the object pool that manages this power-up's lifecycle.
  /// Called automatically by the ObjectPool when this object is created.
  /// </summary>
  /// <param name="pool">The object pool managing this power-up.</param>
  public void SetPool(ObjectPool<PowerUp> pool)
  {
    this.pool = pool;
  }

  /// <summary>
  /// Returns this power-up to the object pool for reuse.
  /// </summary>
  public void ReturnToPool()
  {
    pool?.Return(this);
  }

  /// <summary>
  /// Resets the power-up when re-enabled from the pool.
  /// Restores bobbing and crystal scale.
  /// </summary>
  private void OnEnable()
  {
    bobbing = true;

    if (crystal != null)
    {
      crystal.transform.localScale = Vector3.one * CRYSTAL_INITIAL_SCALE;
    }
  }

  /// <summary>
  /// Detects collision with the canoe and triggers collection.
  /// </summary>
  /// <param name="other">The collider that entered the trigger.</param>
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Canoe"))
    {
      bobbing = false;
      HandleCollection();
    }
  }

  /// <summary>
  /// Handles the collection animation and event broadcast.
  /// </summary>
  private void HandleCollection()
  {
    if (crystal == null)
      return;

    // Play collection animation
    crystal.LeanScale(Vector3.zero, COLLECTION_SCALE_DURATION).setEaseInBack();
  }

  /// <summary>
  /// Updates bob and spin animations each frame.
  /// Only performs animation if the power-up hasn't been collected.
  /// </summary>
  private void Update()
  {
    if (!bobbing || crystal == null)
      return;

    UpdateBobbing();
    UpdateRotation();
  }

  /// <summary>
  /// Updates the vertical bobbing motion.
  /// </summary>
  private void UpdateBobbing()
  {
    float newY = initialPosition.y + Mathf.Sin(Time.time * bobbingFrequency) * bobbingAmplitude;
    transform.position = new Vector3(transform.position.x, newY, transform.position.z);
  }

  /// <summary>
  /// Updates the spinning rotation around the Y axis.
  /// </summary>
  private void UpdateRotation()
  {
    transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
  }
}
