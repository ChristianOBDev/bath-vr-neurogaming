using UnityEngine;

public class SceneOrientation : MonoBehaviour
{
    public static SceneOrientation Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Vector3 Resolve(Vector3 localDirection)
    {
        return transform.TransformDirection(localDirection);
    }

    public Vector3 Forward => transform.forward;
    public Vector3 Back => -transform.forward;
    public Vector3 Right => transform.right;
    public Vector3 Left => -transform.right;
    public Vector3 Up => transform.up;
    public Vector3 Down => -transform.up;
}