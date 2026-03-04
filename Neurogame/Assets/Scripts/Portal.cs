using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Portal : MonoBehaviour
{
  public Transform destination;

  public Transform constraintTarget;
  void Awake()
  {
    Collider collider = GetComponent<Collider>();
    collider.isTrigger = true;

    this.tag = "Portal";
  }
}
