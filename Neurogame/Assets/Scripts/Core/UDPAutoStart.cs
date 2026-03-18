using UnityEngine;

public class UDPAutoStart : MonoBehaviour
{
    [Header("Connection Settings")]
    public string remoteIp = "127.0.0.1";
    public int sendPort = 3010;
    public int receivePort = 3002;

    [Header("Settings")]
    public bool autoStartOnAwake = true;
    public bool showSettingsCanvasOnStart = false;

    [Header("References")]
    public GameObject udpSettingsCanvasObject;

    void Awake()
    {
        if (autoStartOnAwake)
            StartUDP();
    }

    void Start()
    {
        if (showSettingsCanvasOnStart && udpSettingsCanvasObject != null)
            udpSettingsCanvasObject.SetActive(true);
    }

    public void StartUDP()
    {
        if (UDPManager.Instance == null)
        {
            Debug.LogWarning("UDPAutoStart: UDPManager.Instance is null.");
            return;
        }

        try
        {
            UDPManager.Instance.Configure(remoteIp, sendPort, receivePort);
            UDPManager.Instance.StartUDP();
            Debug.Log($"UDP started: {remoteIp} send:{sendPort} receive:{receivePort}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UDP failed to start: {e.Message}");
        }
    }

    public void StopUDP()
    {
        if (UDPManager.Instance != null)
            UDPManager.Instance.StopUDP();
    }
}