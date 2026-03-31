using UnityEngine;
public class XRcameraFollow : MonoBehaviour
{
    public GameObject xrOrigin;
    public Transform mainCamera;
    public float forwardOffset = 3f;
    [Header("Fence Settings")]
    public Transform[] fenceZones;
    [Header("Debug")]
    public bool showDebug = false;
    private Vector3 _startPos;
    private float _originStartY;
    private float[] _cutoffRadii;
    private float[] _fadeRadii;
    private bool _insideFence;
    void Start()
    {
        _startPos = transform.position;
        _originStartY = xrOrigin.transform.position.y;
        if (mainCamera == null)
            mainCamera = Camera.main?.transform;
        _cutoffRadii = new float[fenceZones.Length];
        _fadeRadii = new float[fenceZones.Length];
        for (int i = 0; i < fenceZones.Length; i++)
        {
            Transform cutoff = null;
            Transform fade = null;
            foreach (Transform child in fenceZones[i])
            {
                if (child.name.EndsWith("_Cutoff")) cutoff = child;
                if (child.name.EndsWith("_Fade")) fade = child;
            }
            _cutoffRadii[i] = cutoff != null ? cutoff.localScale.x / 2f : 5f;
            _fadeRadii[i] = fade != null ? fade.localScale.x / 2f : 8f;
            if (showDebug)
                Debug.Log($"Fence {fenceZones[i].name}: cutoff={_cutoffRadii[i]}, fade={_fadeRadii[i]}");
        }
    }
    void LateUpdate()
    {
        float originYaw = xrOrigin.transform.eulerAngles.y;
        Vector3 playerPos = new Vector3(
            xrOrigin.transform.position.x,
            mainCamera.position.y,
            xrOrigin.transform.position.z
);
        _insideFence = false;
        for (int i = 0; i < fenceZones.Length; i++)
        {
            if (fenceZones[i] == null) continue;
            float dist = Vector3.Distance(playerPos, fenceZones[i].position);
            if (showDebug)
                Debug.Log($"Fence {fenceZones[i].name}: dist={dist}, cutoff={_cutoffRadii[i]}");
            if (dist <= _cutoffRadii[i])
            {
                _insideFence = true;
                break;
            }
        }
        if (!_insideFence)
        {
            Vector3 basePos = _startPos;
            basePos.y += xrOrigin.transform.position.y - _originStartY;
            float camYaw = mainCamera != null ? mainCamera.localEulerAngles.y : 0f;
            Quaternion camRot = Quaternion.Euler(0f, originYaw + camYaw, 0f);
            Vector3 forward = camRot * new Vector3(0f, 0f, forwardOffset);
            transform.position = basePos + forward;
        }
        if (showDebug)
            Debug.Log($"Inside fence: {_insideFence}");
    }
    public bool IsInsideFence() => _insideFence;
    public float GetFadeRadius(int index) => index < _fadeRadii.Length ? _fadeRadii[index] : 0f;
}