using System.Collections;
using UnityEngine;

public class RespawnRunner : MonoBehaviour
{
    public static RespawnRunner Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Run(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
}
