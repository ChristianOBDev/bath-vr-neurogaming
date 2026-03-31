using UnityEngine;
using UnityEngine.Audio;

public class FenceMusicFader : MonoBehaviour
{
    [Header("References")]
    public AudioMixer mixer;
    public string musicVolParam = "MusicVol";
    public XRcameraFollow xrCameraFollow;
    public Transform xrOrigin;

    private float _baselineDb;
    private bool _gotBaseline;
    private CharacterController _cc;

    void Start()
    {
        _cc = xrOrigin.GetComponent<CharacterController>();
        // Grab whatever the mixer is currently set to
        if (mixer.GetFloat(musicVolParam, out float current))
        {
            _baselineDb = current;
            _gotBaseline = true;
        }
        else
        {
            Debug.LogError($"FenceMusicFader: '{musicVolParam}' not exposed on mixer.");
        }
    }

    void Update()
    {
        if (!_gotBaseline || xrCameraFollow == null) return;

        Vector3 playerPos = xrOrigin.position;
        if (_cc != null)
        {
            float yaw = xrOrigin.eulerAngles.y;
            playerPos += Quaternion.Euler(0f, yaw, 0f) * _cc.center;
        }

        float closestT = 0f; // 0 = outside all fades, 1 = at cutoff

        var fenceZones = xrCameraFollow.fenceZones;
        for (int i = 0; i < fenceZones.Length; i++)
        {
            if (fenceZones[i] == null) continue;

            float fadeR = xrCameraFollow.GetFadeRadius(i);
            float cutoffR = GetCutoffRadius(fenceZones[i]);
            float dist = Vector3.Distance(playerPos, fenceZones[i].position);

            if (dist <= cutoffR)
            {
                closestT = 1f;
                break;
            }
            else if (dist < fadeR)
            {
                float t = 1f - (dist - cutoffR) / (fadeR - cutoffR);
                if (t > closestT) closestT = t;
            }
        }

        float db = Mathf.Lerp(_baselineDb, -80f, closestT);
        mixer.SetFloat(musicVolParam, db);
    }

    private float GetCutoffRadius(Transform fenceZone)
    {
        foreach (Transform child in fenceZone)
        {
            if (child.name.EndsWith("_Cutoff"))
                return child.localScale.x / 2f;
        }
        return 5f;
    }
}