using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
  private static T instance;
  public static T Instance
  {
    get
    {
      if (instance == null)
      {
        instance = FindFirstObjectByType<T>();
        if (instance == null)
        {
          GameObject singletonObject = new(typeof(T).Name);
          instance = singletonObject.AddComponent<T>();
        }
      }
      return instance;
    }
  }

  protected virtual void Awake()
  {
    if (instance != null)
    {
      Destroy(gameObject);
      return;
    }

    instance = this as T;
  }

  protected virtual void OnDestroy()
  {
    if (instance == this)
    {
      instance = null;
    }
  }
}
