using UnityEngine;

namespace EasyDriving
{
    public class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T GetInstance()
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject
                {
                    name = typeof(T).Name
                };
                _instance = obj.AddComponent<T>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }
}

