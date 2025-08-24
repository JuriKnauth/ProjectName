using UnityEngine;

namespace CompanyName.ProductName.Scripts.Runtime.Utilities
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        private static bool isQuitting;

        public static bool HasInstance => !isQuitting && _instance is not null;

        public static T Instance
        {
            get
            {
                if (!HasInstance)
                {
                    lock (_lock)
                    {
#if UNITY_EDITOR
                        GameObject currentGameObject = new GameObject(typeof(T).Name);
#else
                        GameObject currentGameObject = new GameObject();
#endif
                        _instance = currentGameObject.AddComponent<T>();
                        DontDestroyOnLoad(currentGameObject);
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (!HasInstance)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
                OnFirstInstance();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit() => isQuitting = true;

        protected virtual void OnFirstInstance()
        {
        }
    }
}