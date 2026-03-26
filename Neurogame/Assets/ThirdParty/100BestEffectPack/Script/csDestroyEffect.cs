using UnityEngine;
using UnityEngine.InputSystem;

public class csDestroyEffect : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.xKey.wasPressedThisFrame ||
            Keyboard.current.zKey.wasPressedThisFrame ||
            Keyboard.current.cKey.wasPressedThisFrame)
        {
            Destroy(gameObject);
        }
    }
}