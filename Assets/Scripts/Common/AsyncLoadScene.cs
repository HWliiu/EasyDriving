using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EasyDriving
{
    public class AsyncLoadScene : MonoBehaviour
    {
        public static int SceneBuildIndex;
        public Slider LoadingSlider;
        public Text ProgressText;

        private float _targetValue;
        private AsyncOperation _operation;

        // Use this for initialization
        void Start()
        {
            LoadingSlider.value = 0.0f;
            StartCoroutine(AsyncLoading());
            DOTween.Clear(true);
        }

        // Update is called once per frame
        void Update()
        {
            _targetValue = _operation.progress;

            if (_operation.progress > 0.89f)
            {
                //operation.progress的值最大为0.9
                _targetValue = 1.0f;
            }

            LoadingSlider.value = Mathf.MoveTowards(LoadingSlider.value, _targetValue, Time.deltaTime / 2f);

            ProgressText.text = ((int)(LoadingSlider.value * 100)).ToString() + "%";

            if ((int)(LoadingSlider.value * 100) == 100)
            {
                //允许异步加载完毕后自动切换场景
                _operation.allowSceneActivation = true;
            }
        }

        IEnumerator AsyncLoading()
        {
            _operation = SceneManager.LoadSceneAsync(SceneBuildIndex);
            //阻止当加载完成自动切换
            _operation.allowSceneActivation = false;

            yield return _operation;
        }
    }
}
