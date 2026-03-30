using System.Collections;
using UnityEngine;
using TMPro;
using MotorImagery;

[RequireComponent(typeof(Collider))]
public class Bumper : MonoBehaviour
{
    [Header("Force")]
    public float repulsionForce = 10f;
    public float upwardForce = 5f;

    [Header("Burst")]
    public bool burstOnContact = true;
    public GameObject burstEffect;

    [Header("Killzone")]
    public bool playEffectOnKillzone = false;

    [Header("Drift")]
    public float driftSpeedMin = 0.3f;
    public float driftSpeedMax = 0.8f;
    public float driftStartDelay = 1f;

    private float driftSpeed;

    [Header("Score Popup")]
    public GameObject scorePopupPrefab;

    [Header("Audio")]
    public AudioClip hitSound;

    [Header("Points")]
    public bool overridePoints = false;
    public int pointOverride = 200;

    private bool activated;
    private bool dying;
    private BumperFlash flash;
    private BumperTween bumperTween;
    private Rigidbody rb;
    private Collider col;
    private AudioSource audioSource;
    private Coroutine driftCoroutine;
    private Coroutine flashBurstCoroutine;
    private Coroutine killzoneCoroutine;
    private Coroutine resetCoroutine;

    void Awake()
    {
        flash = GetComponent<BumperFlash>();
        bumperTween = GetComponent<BumperTween>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        // Defer reset by one frame to avoid race condition with SetActive(true)
        if (resetCoroutine != null)
            StopCoroutine(resetCoroutine);
        resetCoroutine = StartCoroutine(DeferredReset());
    }

    private IEnumerator DeferredReset()
    {
        // Wait one frame to let OnEnable fully complete
        yield return null;
        ResetBumper();
        resetCoroutine = null;
    }

    /// <summary>
    /// Reset bumper to initial state for reuse from pool.
    /// </summary>
    public void ResetBumper()
    {
        activated = false;
        dying = false;

        // Stop all running coroutines (but not the reset coroutine itself)
        if (driftCoroutine != null)
            StopCoroutine(driftCoroutine);
        if (flashBurstCoroutine != null)
            StopCoroutine(flashBurstCoroutine);
        if (killzoneCoroutine != null)
            StopCoroutine(killzoneCoroutine);

        driftCoroutine = null;
        flashBurstCoroutine = null;
        killzoneCoroutine = null;

        // Reset physics - keep kinematic, no velocity clearing needed
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Reset collider
        if (col != null)
            col.enabled = true;

        // Reset scale and visuals BEFORE tween
        if (bumperTween != null)
        {
            // Force reset any corrupted tween state
            bumperTween.ResetTweenState();
            // Play spawn tween (which handles scale animation from 0 to target)
            bumperTween.PlaySpawnTween();
        }
        else
        {
            // Fallback: manually reset scale to full size
            transform.localScale = Vector3.one;
            
            if (flash != null)
            {
                Renderer rend = GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    rend.material.DisableKeyword("_EMISSION");
                }
            }
        }

        // Initialize new drift for this activation
        driftSpeed = Random.Range(driftSpeedMin, driftSpeedMax);
        driftCoroutine = StartCoroutine(BeginDrift());
    }

    IEnumerator BeginDrift()
    {
        if (driftStartDelay > 0f)
            yield return new WaitForSeconds(driftStartDelay);

        while (true)
        {
            transform.position += Vector3.down * driftSpeed * Time.deltaTime;
            yield return null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (activated || dying) return;
        Rigidbody ballRb = collision.rigidbody;
        if (ballRb == null) return;

        BallController ball = collision.gameObject.GetComponent<BallController>();
        if (ball == null) return;
        if (ball.CurrentState != BallState.OnWaterfall && ball.CurrentState != BallState.Falling) return;

        activated = true;

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        Vector3 radial = (ballRb.position - transform.position).normalized;
        Vector3 force = radial * repulsionForce + Vector3.up * upwardForce;
        ballRb.AddForce(force, ForceMode.Impulse);

        int pointsEarned = 0;
        int combo = 1;

        if (GameManager.Instance != null)
            (pointsEarned, combo) = GameManager.Instance.RegisterBumperHit(overridePoints ? pointOverride : -1);

        SpawnPopup(pointsEarned, combo);

        if (burstOnContact)
            flashBurstCoroutine = StartCoroutine(FlashThenBurst());
        else
        {
            if (flash != null)
                flashBurstCoroutine = StartCoroutine(flash.DoFlash());
            activated = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (dying) return;
        if (other.CompareTag("Killzone"))
            killzoneCoroutine = StartCoroutine(KillzoneBurst());
    }

    IEnumerator KillzoneBurst()
    {
        dying = true;

        if (playEffectOnKillzone)
        {
            if (flash != null)
                yield return StartCoroutine(flash.DoFlash());

            if (burstEffect != null)
                Instantiate(burstEffect, transform.position, Quaternion.identity);

            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            yield return new WaitForSeconds(0.3f);
        }

        ReturnToPool();
    }

    void SpawnPopup(int points, int combo)
    {
        if (scorePopupPrefab == null) return;

        Quaternion spawnRotation = SceneOrientation.Instance != null
            ? SceneOrientation.Instance.transform.rotation
            : Quaternion.identity;

        GameObject popup = Instantiate(
            scorePopupPrefab,
            transform.position + Vector3.up * 0.5f,
            spawnRotation
        );

        ScorePopup sp = popup.GetComponent<ScorePopup>();
        if (sp != null) sp.Init(points, combo);
    }

    IEnumerator FlashThenBurst()
    {
        dying = true;

        if (flash != null)
            yield return StartCoroutine(flash.DoFlash());

        Burst();
    }

    void Burst()
    {
        if (burstEffect != null)
            Instantiate(burstEffect, transform.position, Quaternion.identity);

        if (GameManager.Instance != null && GameManager.Instance.verboseLogging)
            Debug.Log("Bumper destroyed: " + gameObject.name);

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (BumperPool.Instance != null)
            BumperPool.Instance.ReturnBumper(this);
    }
}