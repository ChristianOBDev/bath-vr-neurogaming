using System.Collections;
using UnityEngine;

[RequireComponent(typeof(NeuroTargetHealth))]
public class NeuroPirateShipController : MonoBehaviour
{
    [Header("Movement")]
    public Transform reachPoint;
    public float moveSpeed = 1.2f;

    [Header("Death VFX (optional)")]
    public GameObject deathVfxPrefab;

    private Vector3 startPos;
    private Quaternion startRot;

    private NeuroTargetHealth health;
    private Collider[] colliders;
    private Renderer[] renderers;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        health = GetComponent<NeuroTargetHealth>();
        health.OnKilled += HandleKilled;

        colliders = GetComponentsInChildren<Collider>(true);
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    void Update()
    {
        if (!health.IsAlive) return;
        if (reachPoint == null) return;

        Vector3 dir = reachPoint.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        transform.position += dir.normalized * moveSpeed * Time.deltaTime;
    }

    private void HandleKilled(NeuroTargetHealth th)
    {
        if (deathVfxPrefab != null)
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);

        SetVisible(false);
    }

    public void ResetShipImmediate()
    {
        transform.position = startPos;
        transform.rotation = startRot;

        health.ResetHealth();
        SetVisible(true);
        gameObject.SetActive(true);
    }

    private void SetVisible(bool on)
    {
        foreach (var c in colliders)
            if (c != null) c.enabled = on;

        foreach (var r in renderers)
            if (r != null) r.enabled = on;
    }
}