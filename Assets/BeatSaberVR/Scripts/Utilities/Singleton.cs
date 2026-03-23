using UnityEngine;
namespace BeatSaberVR
{
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        [Header("Singleton Settings")]
        [SerializeField] protected bool dontDestroyOnLoad = false;

        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        if (instance.GetComponent<Singleton<T>>().dontDestroyOnLoad)
                            DontDestroyOnLoad(singletonObject);
                        Debug.Log($"[Singleton] An instance of {typeof(T)} is created with DontDestroyOnLoad.");
                    }
                }
                return instance;
            }
        }

        public virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            else if (instance != null)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already exists. Destroying duplicate.");
                Destroy(gameObject);
            }
        }
    }
}
