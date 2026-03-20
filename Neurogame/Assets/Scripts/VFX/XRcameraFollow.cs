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

    private CharacterController _cc;
    private Vector3 _startPos;
    private float _originStartY;
    private float[] _cutoffRadii;
    private float[] _fadeRadii;
    private bool _insideFence;

    void Start()
    {
        _cc = xrOrigin.GetComponent<CharacterController>();
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
        if (_cc == null) return;

        float originYaw = xrOrigin.transform.eulerAngles.y;
        Quaternion rot = Quaternion.Euler(0f, originYaw, 0f);
        Vector3 rotatedCenter = rot * _cc.center;

        // Fence check
        Vector3 playerPos = xrOrigin.transform.position + rot * _cc.center;

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

        // Only follow when outside all fences
        if (!_insideFence)
        {
            Vector3 basePos = _startPos + rotatedCenter;
            basePos.y += xrOrigin.transform.position.y - _originStartY;

            float camYaw = mainCamera.localEulerAngles.y;
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