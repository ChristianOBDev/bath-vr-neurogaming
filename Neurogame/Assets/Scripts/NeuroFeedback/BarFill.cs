using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class EnergyChargerBands_TapHold : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image fillImage;

    [Header("Band visuals (optional)")]
    [SerializeField] private RectTransform bandLow;
    [SerializeField] private RectTransform bandMid;
    [SerializeField] private RectTransform bandHigh;

    [Header("Input")]
    [SerializeField] private KeyCode chargeKey = KeyCode.Space;
#if ENABLE_INPUT_SYSTEM
    [SerializeField] private InputActionReference vrChargeAction;
#endif
    private bool uiPressing;
#if ENABLE_INPUT_SYSTEM
    private bool vrPressing;
#endif

    [Header("Energy Behavior (pump + leak)")]
    [Tooltip("How fast energy increases while pressing.")]
    [SerializeField] private float pumpRate = 0.9f;

    [Tooltip("How fast energy decreases when not pressing (small = easier to maintain).")]
    [SerializeField] private float leakRate = 0.35f;

    [Tooltip("Extra damping near current value (optional). 0 = none.")]
    [SerializeField] private float smoothing = 0f;

    [System.Serializable]
    public class Band
    {
        [Range(0f, 1f)] public float min;
        [Range(0f, 1f)] public float max;
        public float holdSeconds;
    }

    [Header("Bands (fat thresholds)")]
    [SerializeField] private Band low = new Band { min = 0.15f, max = 0.40f, holdSeconds = 0.6f };
    [SerializeField] private Band mid = new Band { min = 0.40f, max = 0.75f, holdSeconds = 0.8f };
    [SerializeField] private Band high = new Band { min = 0.75f, max = 1.00f, holdSeconds = 1.0f };

    [Header("Fire")]
    [SerializeField] private CannonShooter3Power cannon;

    private float holdTimer;
    private CannonShooter3Power.ShotType? bandLock;
    private bool firedThisCycle;

#if ENABLE_INPUT_SYSTEM
    private void OnEnable()
    {
        if (vrChargeAction != null && vrChargeAction.action != null)
        {
            vrChargeAction.action.Enable();
            vrChargeAction.action.started += _ => vrPressing = true;
            vrChargeAction.action.canceled += _ => vrPressing = false;
        }
    }

    private void OnDisable()
    {
        if (vrChargeAction != null && vrChargeAction.action != null)
            vrChargeAction.action.Disable();
    }
#endif

    private void Awake()
    {
        fillImage.fillAmount = 0f;
        LayoutBandRects();
    }

    private void Update()
    {
        bool pressing =
            uiPressing ||
            Input.GetKey(chargeKey)
#if ENABLE_INPUT_SYSTEM
            || vrPressing
#endif
            ;

        float targetDelta = (pressing ? pumpRate : -leakRate) * Time.deltaTime;

        // Optional smoothing (helps with “hovering” feel)
        if (smoothing > 0f)
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, fillImage.fillAmount + targetDelta, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
        else
            fillImage.fillAmount += targetDelta;

        fillImage.fillAmount = Mathf.Clamp01(fillImage.fillAmount);

        // Determine current band (if any)
        var currentBand = GetBand(fillImage.fillAmount, out float requiredHold);

        // If not inside a band, reset hold tracking
        if (currentBand == null)
        {
            holdTimer = 0f;
            bandLock = null;

            // Allow new firing after you drop out of all bands OR energy goes low enough
            if (fillImage.fillAmount <= 0.05f)
                firedThisCycle = false;

            return;
        }

        // If we already fired, require you to drop low first to fire again
        if (firedThisCycle) return;

        // If you moved to a different band, reset timer
        if (bandLock != currentBand)
        {
            bandLock = currentBand;
            holdTimer = 0f;
        }

        // You must keep the fill within the band (tap/hold/release) for required time
        holdTimer += Time.deltaTime;

        if (holdTimer >= requiredHold)
        {
            if (cannon != null)
                cannon.Fire(bandLock.Value);

            firedThisCycle = true;

            // Spend some energy on fire (tweakable)
            fillImage.fillAmount = 0f;

            holdTimer = 0f;
            bandLock = null;
        }
    }

    private CannonShooter3Power.ShotType? GetBand(float fill, out float holdRequired)
    {
        // priority high > mid > low
        if (InBand(fill, high)) { holdRequired = high.holdSeconds; return CannonShooter3Power.ShotType.HighDoubleFast; }
        if (InBand(fill, mid)) { holdRequired = mid.holdSeconds; return CannonShooter3Power.ShotType.MidChain; }
        if (InBand(fill, low)) { holdRequired = low.holdSeconds; return CannonShooter3Power.ShotType.LowSlow; }

        holdRequired = 0f;
        return null;
    }

    private bool InBand(float f, Band b)
    {
        float mn = Mathf.Min(b.min, b.max);
        float mx = Mathf.Max(b.min, b.max);
        return f >= mn && f <= mx;
    }

    // UI button hooks
    public void StartCharge() => uiPressing = true;
    public void StopCharge() => uiPressing = false;

    // Optional: auto-place your fat red rectangles to match min/max values
    private void LayoutBandRects()
    {
        if (bandLow) SetBandRect(bandLow, low.min, low.max);
        if (bandMid) SetBandRect(bandMid, mid.min, mid.max);
        if (bandHigh) SetBandRect(bandHigh, high.min, high.max);
    }

    private void SetBandRect(RectTransform rt, float min, float max)
    {
        RectTransform parent = rt.parent as RectTransform;
        if (parent == null) return;

        float h = parent.rect.height;
        float mn = Mathf.Min(min, max);
        float mx = Mathf.Max(min, max);

        float yMin = (mn * h) - (h * parent.pivot.y);
        float yMax = (mx * h) - (h * parent.pivot.y);

        Vector2 size = rt.sizeDelta;
        size.y = Mathf.Abs(yMax - yMin);
        rt.sizeDelta = size;

        Vector2 pos = rt.anchoredPosition;
        pos.y = (yMin + yMax) * 0.5f;
        rt.anchoredPosition = pos;
    }
}
