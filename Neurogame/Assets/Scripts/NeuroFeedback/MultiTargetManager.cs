using UnityEngine;

public class MultiTargetManager : MonoBehaviour
{
    [SerializeField] private TargetHealth[] targets;
    [SerializeField] private int startIndex = 0;

    public TargetHealth CurrentTarget { get; private set; }
    private int currentIndex;

    private void Awake()
    {
        currentIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, targets.Length - 1));

        foreach (var t in targets)
            if (t != null) t.OnKilled += HandleKilled;

        PickNextAliveFrom(currentIndex);
    }

    private void OnDestroy()
    {
        foreach (var t in targets)
            if (t != null) t.OnKilled -= HandleKilled;
    }

    private void HandleKilled(TargetHealth killed)
    {
        // DO NOT disable the ship here.
        // Ship handles death/respawn itself.
        if (killed == CurrentTarget)
        {
            int next = (currentIndex + 1) % targets.Length;
            PickNextAliveFrom(next);
        }
    }

    private void PickNextAliveFrom(int start)
    {
        CurrentTarget = null;
        if (targets == null || targets.Length == 0) return;

        for (int i = 0; i < targets.Length; i++)
        {
            int idx = (start + i) % targets.Length;
            if (targets[idx] != null && targets[idx].IsAlive)
            {
                currentIndex = idx;
                CurrentTarget = targets[idx];
                return;
            }
        }
    }

    public TargetHealth GetNearestAliveTarget(Vector3 fromPos, TargetHealth exclude)
    {
        TargetHealth best = null;
        float bestDist = float.PositiveInfinity;

        foreach (var t in targets)
        {
            if (t == null || t == exclude) continue;
            if (!t.IsAlive) continue;

            float d = Vector3.Distance(fromPos, t.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = t;
            }
        }

        return best;
    }
}
