using System;
using UnityEngine;

/// <summary>
/// Represents a single chunk of the game world that flows towards the player.
/// Manages chunk lifecycle, lane configuration, visual indicators, and pooling.
/// Chunks are recycled and reused through an object pool for performance.
/// </summary>
public class Chunk : MonoBehaviour, IPoolable<Chunk>
{
  // Constants
  private const float ACTIVATION_THRESHOLD = 0f;
  private const int INVALID_LANE = -1;

  // Configuration - Visual
  [SerializeField] private Material greenSlalomMaterial;
  [SerializeField] private Material redSlalomMaterial;

  // Configuration - Structure
  [SerializeField] private Transform[] laneAnchors;
  [SerializeField] private MeshRenderer[] slalomPoles;

  // State - Lifecycle
  private bool active;
  private bool passed;

  // State - Lane Configuration
  private int obstacleLane = INVALID_LANE;
  private int powerUpLane = INVALID_LANE;

  // State - Movement
  private float moveSpeed;
  private float chunkLength;

  // References
  private ObjectPool<Chunk> pool;

  /// <summary>
  /// Gets the lane index where the obstacle is located.
  /// Returns -1 if no obstacle in this chunk.
  /// </summary>
  public int ObstacleLane => obstacleLane;

  /// <summary>
  /// Gets the lane index where the power-up is located.
  /// Returns -1 if no power-up in this chunk.
  /// </summary>
  public int PowerUpLane => powerUpLane;

  /// <summary>
  /// Gets the lane anchor transforms that position objects in lanes.
  /// </summary>
  public Transform[] LaneAnchors => laneAnchors;

  /// <summary>
  /// Sets the object pool that manages this chunk's lifecycle.
  /// Called automatically by the ObjectPool when this chunk is created.
  /// </summary>
  /// <param name="pool">The object pool managing this chunk.</param>
  public void SetPool(ObjectPool<Chunk> pool)
  {
    this.pool = pool;
  }

  /// <summary>
  /// Initializes the chunk with starting conditions.
  /// </summary>
  /// <param name="speed">Movement speed of the chunk.</param>
  /// <param name="startPosition">Starting world position.</param>
  /// <param name="length">Length of the chunk.</param>
  public void Initialize(float speed, Vector3 startPosition, float length)
  {
    if (speed <= 0)
    {
      Debug.LogWarning("Chunk: Speed should be positive");
    }

    if (length <= 0)
    {
      Debug.LogWarning("Chunk: Length should be positive");
    }

    moveSpeed = speed;
    chunkLength = length;
    transform.localPosition = startPosition;
    gameObject.SetActive(true);
  }

  /// <summary>
  /// Updates the movement speed of this chunk.
  /// </summary>
  /// <param name="speed">New movement speed.</param>
  public void SetSpeed(float speed)
  {
    if (speed <= 0)
    {
      Debug.LogWarning("Chunk: Speed should be positive");
    }

    moveSpeed = speed;
  }

  /// <summary>
  /// Configures the lanes for this chunk and updates visual indicators.
  /// Green poles highlight the power-up lane.
  /// </summary>
  /// <param name="obstacleLane">Lane index containing the obstacle, or -1 for none.</param>
  /// <param name="powerUpLane">Lane index containing the power-up, or -1 for none.</param>
  public void SetLanes(int obstacleLane, int powerUpLane)
  {
    this.obstacleLane = obstacleLane;
    this.powerUpLane = powerUpLane;

    UpdateVisualIndicators(powerUpLane);
  }

  /// <summary>
  /// Updates the visual pole materials based on the power-up lane.
  /// </summary>
  /// <param name="powerUpLane">Lane index of the power-up, or -1 for none.</param>
  private void UpdateVisualIndicators(int powerUpLane)
  {
    // Reset all poles to red
    foreach (var pole in slalomPoles)
    {
      pole.material = redSlalomMaterial;
    }

    // Highlight power-up lane with green poles
    if (IsValidLaneIndex(powerUpLane))
    {
      slalomPoles[powerUpLane].material = greenSlalomMaterial;
      if (powerUpLane + 1 < slalomPoles.Length)
      {
        slalomPoles[powerUpLane + 1].material = greenSlalomMaterial;
      }
    }
  }

  /// <summary>
  /// Updates the chunk position and checks lifecycle events.
  /// Should be called once per frame.
  /// </summary>
  /// <param name="deltaTime">Time elapsed since last frame.</param>
  public void Tick(float deltaTime)
  {
    float moveAmount = moveSpeed * deltaTime;
    transform.Translate(Vector3.back * moveAmount, Space.Self);

    CheckAndFireActivation();
    if (active) CheckAndFirePassed();
  }

  /// <summary>
  /// Resets the chunk's lifecycle state for reuse.
  /// </summary>
  public void Reset()
  {
    active = false;
    passed = false;
  }

  /// <summary>
  /// Returns this chunk to the object pool for reuse.
  /// </summary>
  public void ReturnToPool()
  {
    gameObject.SetActive(false);
    pool?.Return(this);
  }

  /// <summary>
  /// Gets the lane configuration as a tuple.
  /// </summary>
  /// <returns>Tuple containing (obstacleLane, powerUpLane).</returns>
  public (int, int) GetLanes()
  {
    return (obstacleLane, powerUpLane);
  }

  /// <summary>
  /// Checks if the chunk has entered the active zone and fires the activation event.
  /// </summary>
  private void CheckAndFireActivation()
  {
    if (active) return;

    if (transform.localPosition.z <= ACTIVATION_THRESHOLD)
    {
      MVEPGameEvents.OnChunkActivated?.Invoke(this);
      active = true;
    }
  }

  /// <summary>
  /// Checks if the chunk has completely passed and fires the passed event.
  /// </summary>
  private void CheckAndFirePassed()
  {
    if (passed) return;

    if (transform.localPosition.z <= -chunkLength)
    {
      MVEPGameEvents.OnChunkPassed?.Invoke(this);
      passed = true;
    }
  }

  /// <summary>
  /// Validates if a lane index is within the valid lane range.
  /// </summary>
  /// <param name="laneIndex">Lane index to validate.</param>
  /// <returns>True if lane index is valid and >= 0, false otherwise.</returns>
  private bool IsValidLaneIndex(int laneIndex)
  {
    return laneIndex >= 0 && laneIndex < slalomPoles.Length;
  }
}
