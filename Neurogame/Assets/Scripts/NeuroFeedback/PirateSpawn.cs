using System.Collections;
using UnityEngine;

public class PirateShipController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TargetHealth health;
    [SerializeField] private Transform reachPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 1f;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnPointIsDynamic = false;

    [Header("Death VFX")]
    [SerializeField] private GameObject deathExplosionPrefab;
    [SerializeField] private float deathExplosionLife = 4f;
    [Tooltip("Optional: spawn explosion a bit above the ship center")]
    [SerializeField] private Vector3 deathVfxOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Hide on death (root stays active)")]
    [SerializeField] private Renderer[] renderersToHide;
    [SerializeField] private Collider[] collidersToDisable;
    [SerializeField] private GameObject[] objectsToHide;

    private Vector3 startPos;
    private Quaternion startRot;

    private Rigidbody rb;
    private bool dead;

    private void Awake()
    {
        if (health == null) health = GetComponent<TargetHealth>();
        rb = GetComponent<Rigidbody>();

        if (renderersToHide == null || renderersToHide.Length == 0)
            renderersToHide = GetComponentsInChildren<Renderer>(true);

        if (collidersToDisable == null || collidersToDisable.Length == 0)
            collidersToDisable = GetComponentsInChildren<Collider>(true);

        CaptureSpawnOnce();

        if (health != null)
            health.OnKilled += HandleKilled;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnKilled -= HandleKilled;
    }

    private void CaptureSpawnOnce()
    {
        if (spawnPoint != null)
        {
            startPos = spawnPoint.position;
            startRot = spawnPoint.rotation;
        }
        else
        {
            startPos = transform.position;
            startRot = transform.rotation;
        }
    }

    private void Update()
    {
        if (dead) return;
        if (health != null && !health.IsAlive) return;
        if (reachPoint == null) return;

        Vector3 to = reachPoint.position - transform.position;
        to.y = 0f;

        float dist = to.magnitude;
        if (dist <= stopDistance) return;

        Vector3 dir = to.normalized;
        Vector3 step = dir * moveSpeed * Time.deltaTime;

        // Move only (no rotation changes)
        if (rb != null && !rb.isKinematic)
            rb.MovePosition(transform.position + step);
        else
            transform.position += step;
    }

    private void HandleKilled(TargetHealth h)
    {
        if (dead) return;

        float delay = (h != null) ? h.RespawnDelay : 3f;

        // Run on RespawnRunner so coroutine still runs even if some other script disables things
        if (RespawnRunner.Instance != null)
            RespawnRunner.Instance.Run(DeathAndRespawnRoutine(delay));
        else
            StartCoroutine(DeathAndRespawnRoutine(delay));
    }

    private IEnumerator DeathAndRespawnRoutine(float delay)
    {
        dead = true;

        // ✅ Spawn explosion BEFORE hiding ship
        SpawnDeathVFX();

        // Hide ship visuals/colliders/UI
        SetShipVisible(false);

        yield return new WaitForSeconds(delay);

        RespawnAtStart();

        if (health != null)
            health.ResetHealth();

        SetShipVisible(true);

        dead = false;
    }

    private void SpawnDeathVFX()
    {
        if (deathExplosionPrefab == null) return;

        // Spawn at the visual center of the ship (more reliable than transform.position)
        Vector3 spawnPos = GetVisualCenterWorld() + deathVfxOffset;

        GameObject fx = Instantiate(deathExplosionPrefab, spawnPos, Quaternion.identity);

        // ✅ Force particle systems to play (in case Play On Awake is off)
        var psList = fx.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < psList.Length; i++)
            psList[i].Play(true);

        // If it's a VFX Graph, you can also auto-play by enabling the object; Play() isn't needed.

        Destroy(fx, deathExplosionLife);
    }

    private Vector3 GetVisualCenterWorld()
    {
        // If we have renderers, use their bounds center
        if (renderersToHide != null && renderersToHide.Length > 0)
        {
            Bounds b = new Bounds(transform.position, Vector3.zero);
            bool hasAny = false;

            foreach (var r in renderersToHide)
            {
                if (r == null) continue;
                if (!hasAny) { b = r.bounds; hasAny = true; }
                else b.Encapsulate(r.bounds);
            }

            if (hasAny) return b.center;
        }

        return transform.position;
    }

    private void RespawnAtStart()
    {
        if (spawnPoint != null && spawnPointIsDynamic)
        {
            startPos = spawnPoint.position;
            startRot = spawnPoint.rotation;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = startPos;
            rb.rotation = startRot;
        }
        else
        {
            transform.position = startPos;
            transform.rotation = startRot;
        }
    }

    private void SetShipVisible(bool visible)
    {
        if (renderersToHide != null)
            foreach (var r in renderersToHide)
                if (r != null) r.enabled = visible;

        if (collidersToDisable != null)
            foreach (var c in collidersToDisable)
                if (c != null) c.enabled = visible;

        if (objectsToHide != null)
            foreach (var go in objectsToHide)
                if (go != null) go.SetActive(visible);
    }
}
