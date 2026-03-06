using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class BCIDataSender : MonoBehaviour
{
    [Tooltip("Drag your Audio Mixer asset here (dearVRDemoMixer)")]
    public AudioMixer mixer;

    [Header("Mixer Group & Effect Names")]
    [Tooltip("Name of the subgroup containing RNBO_MusicEngine (e.g. RNBOrelaxed)")]
    public string groupName = "RNBOrelaxed";

    [Tooltip("Name of the RNBO effect in the group")]
    public string effectName = "RNBO_MusicEngine";

    [Header("Exposed Parameter Names (must match RNBO exposures)")]
    public string frontalAParam = "FrontalA";
    public string frontalBParam = "FrontalB";
    public string parietalAParam = "ParietalA";
    public string frontalMidTParam = "FrontalMidT";

    [Header("CSV Data")]
    [Tooltip("Drag Forest.csv here")]
    [SerializeField] private TextAsset csvFile;

    [Tooltip("Time per CSV row in seconds")]
    public float timePerRow = 5f;

    private List<float[]> bciData = new List<float[]>();
    private float timer = 0f;
    private int currentRow = 0;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (mixer == null)
        {
            Debug.LogError("AudioMixer missing!");
            return;
        }

        if (csvFile == null || string.IsNullOrEmpty(csvFile.text))
        {
            Debug.LogError("CSV file not assigned or empty!");
            return;
        }

        LoadCSV();
        Debug.Log($"Loaded {bciData.Count} BCI rows from CSV");

        // Initial send
        SendBCIData(bciData[0]);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timePerRow)
        {
            timer -= timePerRow;
            currentRow = (currentRow + 1) % bciData.Count;  // Loop
            SendBCIData(bciData[currentRow]);
        }
        else
        {
            int nextRow = (currentRow + 1) % bciData.Count;
            float t = timer / timePerRow;
            float[] current = bciData[currentRow];
            float[] next = bciData[nextRow];
            float[] interpolated = new float[4];
            for (int i = 0; i < 4; i++)
            {
                interpolated[i] = Mathf.Lerp(current[i], next[i], t);
            }
            SendBCIData(interpolated);
        }
    }

    void LoadCSV()
    {
        string[] lines = csvFile.text.Split('\n');
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split(',');
            if (parts.Length < 4) continue;
            float[] row = new float[4];
            bool valid = true;
            for (int i = 0; i < 4; i++)
            {
                if (!float.TryParse(parts[i].Trim(), out row[i]))
                {
                    valid = false;
                    break;
                }
            }
            if (valid) bciData.Add(row);
        }
    }

    void SendBCIData(float[] values)
    {
        if (mixer == null) return;

        mixer.SetFloat("MyExposedParam", values[0]);       // Column 1
        mixer.SetFloat("MyExposedParam 1", values[1]);       // Column 2
        mixer.SetFloat("MyExposedParam 2", values[2]);      // Column 3
        mixer.SetFloat("MyExposedParam 3", values[3]);    // Column 4

        // Debug to check success
        //bool success = mixer.SetFloat("MyExposedParam", values[0]);
        //Debug.Log($"Set MyExposedParam to {values[0]} → success: {success}");
    }
}