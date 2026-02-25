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

    [Header("Score Popup")]
    public GameObject scorePopupPrefab;
    public TMP_FontAsset arcadeFont;

    private bool activated;
    private BumperFlash flash;

    void Awake()
    {
        flash = GetComponent<BumperFlash>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (activated) return;
        Rigidbody rb = collision.rigidbody;
        if (rb == null) return;

        activated = true;

        Vector3 radial = (rb.position - transform.position).normalized;
        Vector3 force = radial * repulsionForce + Vector3.up * upwardForce;
        rb.AddForce(force, ForceMode.Impulse);

        int pointsEarned = 0;
        int combo = 1;

        if (GameManager.Instance != null)
        {
            (pointsEarned, combo) = GameManager.Instance.RegisterBumperHit();
        }

        SpawnPopup(pointsEarned, combo);

        if (burstOnContact)
        {
            StartCoroutine(FlashThenBurst());
        }
        else
        {
            if (flash != null) StartCoroutine(flash.DoFlash());
            activated = false;
        }
    }

    void SpawnPopup(int points, int combo)
    {
        if (scorePopupPrefab == null) return;
        GameObject popup = Instantiate(scorePopupPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        ScorePopup sp = popup.GetComponent<ScorePopup>();
        if (sp != null) sp.Init(points, combo);
    }

    IEnumerator FlashThenBurst()
    {
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