using System;
using UnityEngine;

/// <summary>
/// Manages XR rig profile transitions when the player enters a trigger zone.
/// Applies profile settings, positions, rotations, and constraints based on configured transforms.
/// </summary>
public class XRRigProfileTrigger : MonoBehaviour
{
  [SerializeField]
  [Tooltip("The XR rig profile to apply when triggered")]
  private XRRigProfile profile;

  [SerializeField]
  [Tooltip("Target position and rotation for the rig when triggered")]
  private Transform spawnPoint;

  [SerializeField]
  [Tooltip("Source transform for constraint calculations")]
  private Transform constraintSource;

  [SerializeField]
  [Tooltip("Offset applied to the constraint source")]
  private Vector3 constraintOffset;

  [SerializeField]
  [Tooltip("Position to reset the rig to")]
  private Transform resetPoint;

  /// <summary>
  /// Event triggered when a rig profile is applied. Provides the name of the applied profile.
  /// </summary>
  public Action onRigProfileApplied;
  public Action onRigReset;

  private XRRigController rigController;

  /// <summary>
  /// Initializes the rig controller reference and disables input actions if configured.
  /// </summary>
  private void Start()
  {
    rigController = XRRigController.Instance;

    if (profile != null && profile.inputActions != null)
    {
      profile.inputActions.Disable();
    }
  }

  /// <summary>
  /// Applies the profile settings when the player enters the trigger zone.
  /// </summary>
  /// <param name="other">The collider that entered the trigger</param>
  private void OnTriggerEnter(Collider other)
  {
    // Only process player collisions
    if (!other.CompareTag("Player"))
    {
      return;
    }

    // Ensure rig controller is available
    if (rigController == null)
    {
      rigController = XRRigController.Instance;
    }

    // Validate required components
    if (rigController == null || profile == null)
    {
      return;
    }

    // Apply profile configuration
    rigController.ApplyProfileSettings(profile);

    // Apply spatial transformations if specified
    if (spawnPoint != null)
    {
      rigController.ApplyPositionAndRotation(spawnPoint);
    }

    // Apply constraints if specified
    if (constraintSource != null)
    {
      rigController.ApplyConstraints(constraintSource, constraintOffset);
    }
  }

  /// <summary>
  /// Resets the rig to its initial state and disables input actions.
  /// </summary>
  public void Reset()
  {
    if (rigController == null)
    {
      return;
    }

    rigController.ResetRig(resetPoint);

    if (profile != null && profile.inputActions != null)
    {
      profile.inputActions.Disable();
    }
  }
}