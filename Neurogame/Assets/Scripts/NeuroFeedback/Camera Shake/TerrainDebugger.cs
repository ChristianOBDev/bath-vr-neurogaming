using UnityEngine;
using System.Text;

public class TerrainRuntimeDebugger : MonoBehaviour
{
    [Header("References")]
    public Terrain terrain;
    public Transform player;
    public Transform cameraTransform;

    [Header("Logging")]
    public bool logOnStart = true;
    public bool logEverySecond = false;
    public bool warnOnTransformChanges = true;
    public float positionTolerance = 0.001f;

    private Transform terrainTransform;
    private Transform[] terrainParents;
    private Vector3[] initialParentPositions;
    private Quaternion[] initialParentRotations;
    private Vector3[] initialParentScales;

    private Vector3 initialTerrainPosition;
    private Quaternion initialTerrainRotation;
    private Vector3 initialTerrainScale;

    private Vector3 initialPlayerPosition;
    private Vector3 initialCameraPosition;

    private float timer;

    void Start()
    {
        if (terrain == null)
            terrain = Terrain.activeTerrain;

        if (terrain == null)
        {
            Debug.LogError("[TerrainRuntimeDebugger] No terrain found.");
            enabled = false;
            return;
        }

        terrainTransform = terrain.transform;

        CacheTerrainState();

        if (player != null)
            initialPlayerPosition = player.position;

        if (cameraTransform != null)
            initialCameraPosition = cameraTransform.position;

        if (logOnStart)
            LogFullState("START");
    }

    void Update()
    {
        CheckTerrainTransform();
        CheckParentTransforms();
        CheckPlayerVsTerrainHeight();

        if (logEverySecond)
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;
                LogFullState("TICK");
            }
        }
    }

    void CacheTerrainState()
    {
        initialTerrainPosition = terrainTransform.position;
        initialTerrainRotation = terrainTransform.rotation;
        initialTerrainScale = terrainTransform.localScale;

        int parentCount = GetParentCount(terrainTransform);
        terrainParents = new Transform[parentCount];
        initialParentPositions = new Vector3[parentCount];
        initialParentRotations = new Quaternion[parentCount];
        initialParentScales = new Vector3[parentCount];

        Transform current = terrainTransform.parent;
        int i = 0;
        while (current != null)
        {
            terrainParents[i] = current;
            initialParentPositions[i] = current.position;
            initialParentRotations[i] = current.rotation;
            initialParentScales[i] = current.localScale;
            current = current.parent;
            i++;
        }
    }

    int GetParentCount(Transform t)
    {
        int count = 0;
        Transform current = t.parent;
        while (current != null)
        {
            count++;
            current = current.parent;
        }
        return count;
    }

    void CheckTerrainTransform()
    {
        if (!warnOnTransformChanges) return;

        if (Vector3.Distance(terrainTransform.position, initialTerrainPosition) > positionTolerance)
        {
            Debug.LogWarning(
                $"[TerrainRuntimeDebugger] Terrain position changed.\n" +
                $"Initial: {initialTerrainPosition}\n" +
                $"Current: {terrainTransform.position}",
                terrainTransform
            );
        }

        if (terrainTransform.rotation != initialTerrainRotation)
        {
            Debug.LogWarning(
                $"[TerrainRuntimeDebugger] Terrain rotation changed.\n" +
                $"Initial: {initialTerrainRotation.eulerAngles}\n" +
                $"Current: {terrainTransform.rotation.eulerAngles}",
                terrainTransform
            );
        }

        if (terrainTransform.localScale != initialTerrainScale)
        {
            Debug.LogWarning(
                $"[TerrainRuntimeDebugger] Terrain scale changed.\n" +
                $"Initial: {initialTerrainScale}\n" +
                $"Current: {terrainTransform.localScale}",
                terrainTransform
            );
        }
    }

    void CheckParentTransforms()
    {
        if (!warnOnTransformChanges) return;

        for (int i = 0; i < terrainParents.Length; i++)
        {
            Transform p = terrainParents[i];
            if (p == null) continue;

            if (Vector3.Distance(p.position, initialParentPositions[i]) > positionTolerance)
            {
                Debug.LogWarning(
                    $"[TerrainRuntimeDebugger] Parent moved: {GetPath(p)}\n" +
                    $"Initial Pos: {initialParentPositions[i]}\n" +
                    $"Current Pos: {p.position}",
                    p
                );
            }

            if (p.rotation != initialParentRotations[i])
            {
                Debug.LogWarning(
                    $"[TerrainRuntimeDebugger] Parent rotated: {GetPath(p)}\n" +
                    $"Initial Rot: {initialParentRotations[i].eulerAngles}\n" +
                    $"Current Rot: {p.rotation.eulerAngles}",
                    p
                );
            }

            if (p.localScale != initialParentScales[i])
            {
                Debug.LogWarning(
                    $"[TerrainRuntimeDebugger] Parent scale changed: {GetPath(p)}\n" +
                    $"Initial Scale: {initialParentScales[i]}\n" +
                    $"Current Scale: {p.localScale}",
                    p
                );
            }
        }
    }

    void CheckPlayerVsTerrainHeight()
    {
        if (player == null || terrain == null) return;

        Vector3 playerPos = player.position;
        float terrainWorldY = terrain.SampleHeight(playerPos) + terrain.transform.position.y;
        float delta = playerPos.y - terrainWorldY;

        if (Mathf.Abs(delta) > 2f)
        {
            Debug.LogWarning(
                $"[TerrainRuntimeDebugger] Player height differs a lot from terrain.\n" +
                $"Player Y: {playerPos.y:F3}\n" +
                $"Terrain Y under player: {terrainWorldY:F3}\n" +
                $"Delta: {delta:F3}",
                player
            );
        }
    }

    [ContextMenu("Log Full State")]
    public void LogFullStateContext()
    {
        LogFullState("MANUAL");
    }

    void LogFullState(string label)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"[TerrainRuntimeDebugger] {label}");
        sb.AppendLine($"Terrain: {terrain.name}");
        sb.AppendLine($"Terrain Path: {GetPath(terrain.transform)}");
        sb.AppendLine($"Terrain Pos: {terrain.transform.position}");
        sb.AppendLine($"Terrain Rot: {terrain.transform.rotation.eulerAngles}");
        sb.AppendLine($"Terrain Scale: {terrain.transform.localScale}");

        if (Terrain.activeTerrain != null)
            sb.AppendLine($"Active Terrain: {Terrain.activeTerrain.name}");

        if (player != null)
        {
            float terrainWorldY = terrain.SampleHeight(player.position) + terrain.transform.position.y;
            sb.AppendLine($"Player Pos: {player.position}");
            sb.AppendLine($"Terrain Y under Player: {terrainWorldY:F3}");
            sb.AppendLine($"Player-Terrain Delta: {(player.position.y - terrainWorldY):F3}");
        }

        if (cameraTransform != null)
            sb.AppendLine($"Camera Pos: {cameraTransform.position}");

        Debug.Log(sb.ToString(), terrain);
    }

    string GetPath(Transform t)
    {
        if (t == null) return "(null)";

        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}