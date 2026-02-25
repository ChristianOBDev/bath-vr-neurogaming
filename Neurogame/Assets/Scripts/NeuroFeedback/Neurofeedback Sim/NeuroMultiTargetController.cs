using System.Collections.Generic;
using UnityEngine;

public class NeuroMultiTargetManager : MonoBehaviour
{
    [Tooltip("Drag your ship root objects that have NeuroTargetHealth here (Ship1, Ship2, Ship3).")]
    public List<NeuroTargetHealth> targets = new List<NeuroTargetHealth>();

    private int currentIndex = 0;

    void Awake()
    {
        // Subscribe once
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null) continue;
            targets[i].OnKilled += HandleKilled;
        }
    }

    private void HandleKilled(NeuroTargetHealth killed)
    {
        // Move to next in order when a ship dies
        currentIndex = (currentIndex + 1) % Mathf.Max(1, targets.Count);
        Debug.Log($"[TARGET MANAGER] {killed.name} killed -> next index {currentIndex}");
    }

    public NeuroTargetHealth GetCurrentAliveTarget()
    {
        if (targets == null || targets.Count == 0) return null;

        // Find the next alive starting from currentIndex
        for (int tries = 0; tries < targets.Count; tries++)
        {
            int idx = (currentIndex + tries) % targets.Count;
            var t = targets[idx];
            if (t != null && t.IsAlive)
            {
                currentIndex = idx;
                return t;
            }
        }

        return null;
    }
}