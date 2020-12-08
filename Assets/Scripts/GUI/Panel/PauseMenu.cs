using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyDriving
{
    public class PauseMenu : MonoBehaviour
    {
        public Button RestartButton;
        public Button ExitButton;
        // Start is called before the first frame update
        void Start()
        {
            if (RestartButton == null)
                RestartButton = Utility.FindChild<Button>(transform, nameof(RestartButton)) ?? throw new ArgumentNullException(nameof(RestartButton));
            RestartButton.onClick.AddListener(OnReStart);

            if (ExitButton == null)
                ExitButton = Utility.FindChild<Button>(transform, nameof(ExitButton)) ?? throw new ArgumentNullException(nameof(ExitButton));
            ExitButton.onClick.AddListener(OnExit);
        }

        private void OnReStart()
        {
            Time.timeScale = 1f;
            // TODO
            Utility.LoadScene(2);
        }

        private void OnExit()
        {
            Time.timeScale = 1f;
            UnitySingleton<Main>.GetInstance().CurrentCourse = Course.None;
            Utility.LoadScene(0);
        }
    }
}
