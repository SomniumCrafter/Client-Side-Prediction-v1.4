using UnityEngine;

namespace Project
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        #region Variables

        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    UnityEngine.Debug.LogError(typeof(T).ToString() + " is missing.");
                }

                return _instance;
            }
        }

        #endregion

        #region Mono Behaviour Callbacks

        private void Awake()
        {
            if (_instance != null)
            {
                UnityEngine.Debug.LogWarning("Dupplicate found! Destroying dupplicate");
                Destroy(_instance.gameObject);
            }
            _instance = this as T;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }


        #endregion
    }
}