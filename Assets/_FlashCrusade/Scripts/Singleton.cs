using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T i;
    public static T I
    {
        get
        {
            if (applicationQuitting) return null;
            
            if (i == null)
            {
                i = FindFirstObjectByType<T>();

                if (i == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    i = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return i;
        }
    }

    private static bool applicationQuitting;

    protected virtual void Awake()
    {
        if (i != null && i != this) Destroy(gameObject);
        else
        {
            i = this as T;
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        applicationQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (i == this)
        {
            i = null;
        }
    }
}
