using UnityEngine;
using UnityEngine.SceneManagement;

namespace EasyDriving
{
    public static class Utility
    {
        public static bool IsDeadzoneZero(this float value) => (value > -0.025f && value < 0.025f);

        public static void LoadScene(int sceneBuildIndex)
        {
            AsyncLoadScene.SceneBuildIndex = sceneBuildIndex;
            //SceneManager.LoadScene(1);
            SceneManager.LoadSceneAsync(1);
        }

        public static Transform FindChild(Transform parent, string name)
        {
            Transform child = null;
            child = parent.Find(name);
            if (child != null)
                return child;
            Transform grandchild = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                grandchild = FindChild(parent.GetChild(i), name);
                if (grandchild != null)
                    return grandchild;
            }
            return null;
        }

        public static T FindChild<T>(Transform parent, string name) where T : Component
        {
            Transform child = null;
            child = FindChild(parent, name);
            if (child != null)
                return child.GetComponent<T>();
            return null;
        }
    }
}