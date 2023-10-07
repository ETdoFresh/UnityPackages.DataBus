using UnityEngine;

namespace ETdoFresh.UnityPackages.DataBusSystem
{
    public class MonoBehaviourLazyLoadedSingleton<T> : MonoBehaviour where T : Component
    {
        private static T _instance;
        private static bool _isApplicationQuitting;
        
        public static T Instance
        {
            get
            {
                if (_isApplicationQuitting) return _instance;
                if (_instance) return _instance;
                _instance = FindObjectOfType<T>();
                if (_instance) return _instance;
                _instance = new GameObject($"{typeof(T).Name} (Singleton)").AddComponent<T>();
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }
    }
}