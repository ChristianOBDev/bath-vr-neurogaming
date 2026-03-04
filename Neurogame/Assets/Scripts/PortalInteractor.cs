using UnityEngine;
using UnityEngine.Animations;

public class PortalInteractor : MonoBehaviour
{
  [SerializeField] private PositionConstraint positionConstraint;
  private void OnTriggerEnter(Collider other)
  {
    if (other.CompareTag("Portal"))
    {
      Debug.Log("Player entered a portal!");
      if (other.TryGetComponent<Portal>(out var portal))
      {
        Transform destination = portal.destination;
        if (destination != null)
        {
          transform.SetPositionAndRotation(destination.position, destination.rotation);
        }

        // if (portal.constraintTarget != null)
        // {
        //   positionConstraint.AddSource(new ConstraintSource { sourceTransform = portal.constraintTarget, weight = 1 });
        //   positionConstraint.translationOffset = portal.destination.localPosition;
        //   positionConstraint.locked = true;
        //   positionConstraint.constraintActive = true;
        // }
      }
    }
  }
}
