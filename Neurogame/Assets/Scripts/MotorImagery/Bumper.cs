using System.Collections;
using UnityEngine;
using TMPro;

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

    void Awake()
    {
        flash = GetComponent<BumperFlash>();
    }

    void Start()
    {
        driftSpeed = Random.Range(driftSpeedMin, driftSpeedMax);
        StartCoroutine(BeginDrift());
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
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        BallController ball = collision.gameObject.GetComponent<BallController>();
        if (ball == null) return;
        if (ball.CurrentState != BallState.OnWaterfall && ball.CurrentState != BallState.Falling) return;

        activated = true;

        if (hitSound != null)
            GetComponent<AudioSource>().PlayOneShot(hitSound);

        Vector3 radial = (rb.position - transform.position).normalized;
        Vector3 force = radial * repulsionForce + Vector3.up * upwardForce;
        rb.AddForce(force, ForceMode.Impulse);

        int pointsEarned = 0;
        int combo = 1;

        if (GameManager.Instance != null)
            (pointsEarned, combo) = GameManager.Instance.RegisterBumperHit(overridePoints ? pointOverride : -1);

        SpawnPopup(pointsEarned, combo);

        if (burstOnContact)
            StartCoroutine(FlashThenBurst());
        else
        {
            if (flash != null) StartCoroutine(flash.DoFlash());
            activated = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bumper trigger entered by: {other.gameObject.name} tag: {other.tag}");
        if (dying) return;
        if (other.CompareTag("Killzone"))
            StartCoroutine(KillzoneBurst());
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

            if (hitSound != null)
                GetComponent<AudioSource>().PlayOneShot(hitSound);

            // Wait long enough for sound to audibly play before destroying
            yield return new WaitForSeconds(0.3f);
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnBumperDestroyed(this);

        Destroy(gameObject);
    }

    void SpawnPopup(int points, int combo)
    {
        if (scorePopupPrefab == null) return;
        GameObject popup = Instantiate(
            scorePopupPrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity
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

        if (GameManager.Instance != null)
            GameManager.Instance.OnBumperDestroyed(this);

        Destroy(gameObject);
    }
}