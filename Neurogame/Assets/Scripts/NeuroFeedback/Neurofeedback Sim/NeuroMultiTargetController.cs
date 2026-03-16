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


    [Header("Timing")]
    public float bossSpawnDelay = 2f;
    public float baseRespawnDelay = 3f;


    [Header("Audio")]
    public AudioSource normalPirateAudioSource;
    public AudioSource bossPirateAudioSource;


    private int currentIndex = 0;
    private bool bossPhaseActive = false;
    private bool bossSpawnScheduled = false;


    void Awake()
    {
        // Subscribe to base ships death
        foreach (var t in baseTargets)
        {
            if (t == null) continue;
            t.OnKilled += HandleBaseTargetKilled;
        }

        // Setup boss
        if (bossShipObject != null)
        {
            bossTarget = bossShipObject.GetComponent<NeuroTargetHealth>();

            if (bossTarget != null)
                bossTarget.OnKilled += HandleBossKilled;

            bossShipObject.SetActive(false);
        }

        PlayBaseAudio();
    }


    private void HandleBaseTargetKilled(NeuroTargetHealth killed)
    {
        Debug.Log($"[TARGET MANAGER] Base ship destroyed: {killed.name}");

        if (!bossPhaseActive && !bossSpawnScheduled && AllBaseTargetsDestroyed())
        {
            bossSpawnScheduled = true;
            StartCoroutine(SpawnBossRoutine());
            return;
        }

        MoveToNextAliveBaseTarget();
    }


    private IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(bossSpawnDelay);

        ActivateBoss();
    }


    private void ActivateBoss()
    {
        if (bossShipObject == null)
        {
            Debug.LogWarning("Boss ship object missing.");
            return;
        }

        bossPhaseActive = true;
        bossSpawnScheduled = false;

        bossShipObject.SetActive(true);

        if (bossTarget != null)
            bossTarget.ResetHealth();

        PlayBossAudio();

        Debug.Log("[TARGET MANAGER] Boss activated.");
    }


    private void HandleBossKilled(NeuroTargetHealth killed)
    {
        Debug.Log("[TARGET MANAGER] Boss defeated.");

        StartCoroutine(RespawnBaseShips());
    }


    private IEnumerator RespawnBaseShips()
    {
        yield return new WaitForSeconds(baseRespawnDelay);

        if (bossShipObject != null)
            bossShipObject.SetActive(false);

        bossPhaseActive = false;
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

        Debug.Log("[TARGET MANAGER] Base ships respawned.");
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
                return bossTarget;

            return null;
        }

        for (int tries = 0; tries < baseTargets.Count; tries++)
        {
            int idx = (currentIndex + tries) % baseTargets.Count;
            var t = baseTargets[idx];

            if (t != null && t.IsAlive)
            {
                currentIndex = idx;
                return t;
            }
        }

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