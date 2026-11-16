using UnityEngine;

public class SingletonObject<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindFirstObjectByType<T>();

                if (_instance)
                    DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    protected bool IsDuplicate { get; private set; }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            IsDuplicate = true;
            Destroy(gameObject);
        }
    }
}