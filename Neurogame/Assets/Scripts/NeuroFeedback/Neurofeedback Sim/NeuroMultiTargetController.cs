using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuroMultiTargetManager : MonoBehaviour
{
    [Header("Base Ships (In Scene)")]
    [Tooltip("Base ships already placed in the scene.")]
    public List<NeuroTargetHealth> baseTargets = new List<NeuroTargetHealth>();

    [Header("Boss Ship (In Scene)")]
    [Tooltip("Boss ship GameObject placed in the scene and disabled at start.")]
    public GameObject bossShipObject;

    private NeuroTargetHealth bossTarget;
    private NeuroPirateShipController bossShipController;

    [Header("Optional Boss Spawn Override")]
    [Tooltip("If assigned, the boss will always respawn at this transform.")]
    public Transform bossSpawnPoint;

    [Header("Timing")]
    public float bossSpawnDelay = 2f;
    public float baseRespawnDelay = 3f;

    [Header("Audio")]
    public AudioSource normalPirateAudioSource;
    public AudioSource bossPirateAudioSource;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    private int currentIndex = 0;
    private bool bossPhaseActive = false;
    private bool bossSpawnScheduled = false;
    private Coroutine bossSpawnRoutine;
    private Coroutine respawnRoutine;

    private Vector3 bossInitialPosition;
    private Quaternion bossInitialRotation;
    private Vector3 bossInitialScale;

    private void Awake()
    {
        foreach (var t in baseTargets)
        {
            if (t == null) continue;
            t.OnKilled += HandleBaseTargetKilled;
        }

        if (bossShipObject != null)
        {
            bossTarget = bossShipObject.GetComponentInChildren<NeuroTargetHealth>(true);
            bossShipController = bossShipObject.GetComponent<NeuroPirateShipController>();

            if (bossTarget != null)
            {
                bossTarget.OnKilled += HandleBossKilled;

                if (enableDebugLogs)
                    Debug.Log($"[TARGET MANAGER] Boss target found: {bossTarget.name}");
            }
            else
            {
                Debug.LogWarning("[TARGET MANAGER] No NeuroTargetHealth found on boss ship object or its children.");
            }

            if (bossSpawnPoint != null)
            {
                bossInitialPosition = bossSpawnPoint.position;
                bossInitialRotation = bossSpawnPoint.rotation;
                bossInitialScale = bossShipObject.transform.localScale;
            }
            else
            {
                bossInitialPosition = bossShipObject.transform.position;
                bossInitialRotation = bossShipObject.transform.rotation;
                bossInitialScale = bossShipObject.transform.localScale;
            }

            bossShipObject.SetActive(false);
        }

        bossPhaseActive = false;
        bossSpawnScheduled = false;
        currentIndex = 0;

        PlayBaseAudio();
    }

    private void OnDestroy()
    {
        foreach (var t in baseTargets)
        {
            if (t == null) continue;
            t.OnKilled -= HandleBaseTargetKilled;
        }

        if (bossTarget != null)
            bossTarget.OnKilled -= HandleBossKilled;
    }

    private void HandleBaseTargetKilled(NeuroTargetHealth killed)
    {
        if (enableDebugLogs)
            Debug.Log($"[TARGET MANAGER] Base ship destroyed: {killed.name}");

        if (bossPhaseActive)
            return;

        MoveToNextAliveBaseTarget();

        if (!bossSpawnScheduled && AllBaseTargetsDestroyed())
        {
            bossSpawnScheduled = true;

            if (bossSpawnRoutine != null)
                StopCoroutine(bossSpawnRoutine);

            bossSpawnRoutine = StartCoroutine(SpawnBossRoutine());
        }
    }

    private IEnumerator SpawnBossRoutine()
    {
        if (enableDebugLogs)
            Debug.Log("[TARGET MANAGER] All base ships destroyed. Boss spawn scheduled.");

        yield return new WaitForSeconds(bossSpawnDelay);

        ActivateBoss();
        bossSpawnRoutine = null;
    }

    private void ActivateBoss()
    {
        if (bossShipObject == null)
        {
            Debug.LogWarning("[TARGET MANAGER] Boss ship object missing.");
            bossSpawnScheduled = false;
            return;
        }

        if (bossTarget == null)
        {
            bossTarget = bossShipObject.GetComponentInChildren<NeuroTargetHealth>(true);

            if (bossTarget != null)
            {
                bossTarget.OnKilled -= HandleBossKilled;
                bossTarget.OnKilled += HandleBossKilled;
            }
        }

        bossPhaseActive = true;
        bossSpawnScheduled = false;

        ResetBossImmediate();

        PlayBossAudio();

        if (enableDebugLogs)
            Debug.Log("[TARGET MANAGER] Boss phase active.");
    }

    private void ResetBossImmediate()
    {
        if (bossShipObject == null)
            return;

        bossShipObject.SetActive(true);

        Transform bossTransform = bossShipObject.transform;

        if (bossSpawnPoint != null)
        {
            bossTransform.position = bossSpawnPoint.position;
            bossTransform.rotation = bossSpawnPoint.rotation;
        }
        else
        {
            bossTransform.position = bossInitialPosition;
            bossTransform.rotation = bossInitialRotation;
        }

        bossTransform.localScale = bossInitialScale;

        if (bossShipController != null)
        {
            bossShipController.ResetShipImmediate();
        }
        else if (bossTarget != null)
        {
            bossTarget.ResetHealth();
        }

        Renderer[] renderers = bossShipObject.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
            r.enabled = true;

        Collider[] colliders = bossShipObject.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
            c.enabled = true;

        Rigidbody[] rigidbodies = bossShipObject.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[TARGET MANAGER] Boss reset at position {bossTransform.position} rotation {bossTransform.rotation.eulerAngles} active={bossShipObject.activeSelf}"
            );

            if (bossTarget != null)
                Debug.Log($"[TARGET MANAGER] Boss IsAlive={bossTarget.IsAlive}");
        }
    }

    private void HandleBossKilled(NeuroTargetHealth killed)
    {
        if (enableDebugLogs)
            Debug.Log("[TARGET MANAGER] Boss defeated.");

        if (respawnRoutine != null)
            StopCoroutine(respawnRoutine);

        respawnRoutine = StartCoroutine(RespawnBaseShipsRoutine());
    }

    private IEnumerator RespawnBaseShipsRoutine()
    {
        yield return new WaitForSeconds(baseRespawnDelay);

        if (bossShipObject != null)
            bossShipObject.SetActive(false);

        bossPhaseActive = false;
        bossSpawnScheduled = false;
        currentIndex = 0;

        foreach (var t in baseTargets)
        {
            if (t == null) continue;

            var ship = t.GetComponent<NeuroPirateShipController>();

            if (ship != null)
                ship.ResetShipImmediate();
            else
                t.ResetHealth();
        }

        PlayBaseAudio();

        if (enableDebugLogs)
            Debug.Log("[TARGET MANAGER] Base ships respawned. Loop reset.");

        respawnRoutine = null;
    }

    private bool AllBaseTargetsDestroyed()
    {
        foreach (var t in baseTargets)
        {
            if (t != null && t.IsAlive)
                return false;
        }

        return true;
    }

    private void MoveToNextAliveBaseTarget()
    {
        if (baseTargets.Count == 0)
            return;

        for (int tries = 0; tries < baseTargets.Count; tries++)
        {
            int idx = (currentIndex + 1 + tries) % baseTargets.Count;
            var t = baseTargets[idx];

            if (t != null && t.IsAlive)
            {
                currentIndex = idx;
                return;
            }
        }
    }

    public NeuroTargetHealth GetCurrentAliveTarget()
    {
        if (bossPhaseActive)
        {
            if (bossTarget != null && bossTarget.IsAlive)
            {
                if (enableDebugLogs)
                    Debug.Log($"[TARGET MANAGER] Returning boss target: {bossTarget.name}");

                return bossTarget;
            }

            if (enableDebugLogs)
                Debug.LogWarning("[TARGET MANAGER] Boss phase active, but boss target is null or not alive.");

            return null;
        }

        for (int tries = 0; tries < baseTargets.Count; tries++)
        {
            int idx = (currentIndex + tries) % baseTargets.Count;
            var t = baseTargets[idx];

            if (t != null && t.IsAlive)
            {
                currentIndex = idx;

                if (enableDebugLogs)
                    Debug.Log($"[TARGET MANAGER] Returning base target: {t.name}");

                return t;
            }
        }

        if (enableDebugLogs)
            Debug.LogWarning("[TARGET MANAGER] No alive base target found.");

        return null;
    }

    private void PlayBaseAudio()
    {
        if (bossPirateAudioSource != null)
            bossPirateAudioSource.Stop();

        if (normalPirateAudioSource != null && !normalPirateAudioSource.isPlaying)
            normalPirateAudioSource.Play();
    }

    private void PlayBossAudio()
    {
        if (normalPirateAudioSource != null)
            normalPirateAudioSource.Stop();

        if (bossPirateAudioSource != null && !bossPirateAudioSource.isPlaying)
            bossPirateAudioSource.Play();
    }
}