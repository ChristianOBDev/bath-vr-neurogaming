using System.Collections.Generic;
using UnityEngine;

public class NeuroCalibrator : MonoBehaviour
{
    [SerializeField] private MonoBehaviour signalBehaviour; // INeuroSignal
    private INeuroSignal signal;

    [Header("Calibration")]
    public float recordSeconds = 25f;
    [Range(0f, 1f)] public float minQuality = 0.6f;

    [Header("Results")]
    public float alphaMean = 1f;
    public float alphaStd = 0.25f;

    [Header("Debug")]
    public bool debugLogs = true;

    private readonly List<float> samples = new();
    private float dbgT;

    void Awake()
    {
        signal = signalBehaviour as INeuroSignal;
        if (signal == null) Debug.LogError("signalBehaviour must implement INeuroSignal.");
        Load();
    }

    public void StartCalibration()
    {
        samples.Clear();
        StopAllCoroutines();
        StartCoroutine(CalibrateRoutine());
        if (debugLogs) Debug.Log("[CAL] Calibration started...");
    }

    private System.Collections.IEnumerator CalibrateRoutine()
    {
        float t = 0f;
        while (t < recordSeconds)
        {
            t += Time.deltaTime;

            if (signal != null && signal.Quality >= minQuality)
                samples.Add(signal.Alpha);

            if (debugLogs)
            {
                dbgT += Time.deltaTime;
                if (dbgT >= 1f)
                {
                    dbgT = 0f;
                    Debug.Log($"[CAL] collecting... {t:F1}/{recordSeconds:F1}s | samples:{samples.Count}");
                }
            }

            yield return null;
        }

        ComputeStats(samples, out alphaMean, out alphaStd);
        Save();

        if (debugLogs)
            Debug.Log($"[CAL] Done. alphaMean={alphaMean:F3}, alphaStd={alphaStd:F3}, n={samples.Count}");
    }

    private void ComputeStats(List<float> xs, out float mean, out float std)
    {
        if (xs == null || xs.Count < 10)
        {
            mean = 1f; std = 0.25f; return;
        }

        double sum = 0;
        for (int i = 0; i < xs.Count; i++) sum += xs[i];
        mean = (float)(sum / xs.Count);

        double var = 0;
        for (int i = 0; i < xs.Count; i++)
        {
            double d = xs[i] - mean;
            var += d * d;
        }
        var /= Mathf.Max(1, xs.Count - 1);
        std = Mathf.Sqrt((float)var);
        std = Mathf.Max(0.05f, std);
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("NF_alphaMean", alphaMean);
        PlayerPrefs.SetFloat("NF_alphaStd", alphaStd);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (PlayerPrefs.HasKey("NF_alphaMean"))
            alphaMean = PlayerPrefs.GetFloat("NF_alphaMean");

        if (PlayerPrefs.HasKey("NF_alphaStd"))
            alphaStd = PlayerPrefs.GetFloat("NF_alphaStd");
    }
}
