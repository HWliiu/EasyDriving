using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace EasyDriving
{
    public class GUIManager : MonoBehaviour
    {
        public Text PromptText;
        public Text ResultText;
        public Image ResultPanel;
        public Button RestartButton;
        public Button ExitButton;

        private VehicleController _vc;

        private DCRKManager _dcrkManager;
        private CFTCManager _cftcManager;
        private PTQBManager _ptqbManager;
        private QXXSManager _qxxsManager;
        private ZJZWManager _zjzwManager;

        private TweenerCore<Color, Color, ColorOptions> _textTween;
        // Start is called before the first frame update
        void Start()
        {
            Initialize();
            var main = UnitySingleton<Main>.GetInstance();
            switch (main.CurrentCourse)
            {
                case Course.None:
                    break;
                case Course.Dcrk:
                    if (_dcrkManager != null)
                        _dcrkManager.gameObject.SetActive(true);
                    else
                        throw new ArgumentNullException(nameof(_dcrkManager));
                    break;
                case Course.Ptqb:
                    if (_ptqbManager != null)
                        _ptqbManager.gameObject.SetActive(true);
                    else
                        throw new ArgumentNullException(nameof(_ptqbManager));
                    break;
                case Course.Zjzw:
                    if (_zjzwManager != null)
                        _zjzwManager.gameObject.SetActive(true);
                    else
                        throw new ArgumentNullException(nameof(_zjzwManager));
                    break;
                case Course.Qxxs:
                    if (_qxxsManager != null)
                        _qxxsManager.gameObject.SetActive(true);
                    else
                        throw new ArgumentNullException(nameof(_qxxsManager));
                    break;
                case Course.Cftc:
                    if (_cftcManager != null)
                        _cftcManager.gameObject.SetActive(true);
                    else
                        throw new ArgumentNullException(nameof(_cftcManager));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Initialize()
        {
            if (PromptText == null)
                PromptText = Utility.FindChild<Text>(transform, nameof(PromptText)) ??
                             throw new ArgumentNullException(nameof(PromptText));
            if (ResultText == null)
                ResultText = Utility.FindChild<Text>(transform, nameof(ResultText)) ??
                             throw new ArgumentNullException(nameof(ResultText));
            if (ResultPanel == null)
                ResultPanel = Utility.FindChild<Image>(transform, nameof(ResultPanel)) ??
                              throw new ArgumentNullException(nameof(ResultPanel));

            if (RestartButton == null)
                RestartButton = Utility.FindChild<Button>(ResultPanel.transform, nameof(RestartButton)) ??
                                throw new ArgumentNullException(nameof(RestartButton));
            RestartButton.onClick.AddListener(OnReStart);

            if (ExitButton == null)
                ExitButton = Utility.FindChild<Button>(ResultPanel.transform, nameof(ExitButton)) ??
                             throw new ArgumentNullException(nameof(ExitButton));
            ExitButton.onClick.AddListener(OnExit);

            var vc = GameObject.Find("World/Sedan") ?? throw new ArgumentNullException(nameof(VehicleController));
            _vc = vc.GetComponent<VehicleController>();

            var dcrk = GameObject.Find("World/Management/DCRK") ?? throw new ArgumentNullException(nameof(DCRKManager));
            _dcrkManager = dcrk.GetComponent<DCRKManager>();

            var cftc = GameObject.Find("World/Management/CFTC") ?? throw new ArgumentNullException(nameof(CFTCManager));
            _cftcManager = cftc.GetComponent<CFTCManager>();

            var ptqb = GameObject.Find("World/Management/PTQB") ?? throw new ArgumentNullException(nameof(PTQBManager));
            _ptqbManager = ptqb.GetComponent<PTQBManager>();

            var qxxs = GameObject.Find("World/Management/QXXS") ?? throw new ArgumentNullException(nameof(QXXSManager));
            _qxxsManager = qxxs.GetComponent<QXXSManager>();

            var zjzw = GameObject.Find("World/Management/ZJZW") ?? throw new ArgumentNullException(nameof(ZJZWManager));
            _zjzwManager = zjzw.GetComponent<ZJZWManager>();

        }

        // Update is called once per frame
        //void Update()
        //{
        //}

        public void OnFailure(string reason)
        {
            var tween = DOTween.To(() => Time.timeScale, (x) => Time.timeScale = x, 0f, 2f).SetUpdate(true);
            tween.OnComplete(ActiveResultPanel);
            SetResultText(reason);
        }

        public void OnSuccess()
        {
            DOTween.To(() => Time.timeScale, (x) => Time.timeScale = x, 0f, 2f).SetUpdate(true).OnComplete(ActiveResultPanel);
            SetResultText("恭喜 考试通过");
        }
        private void ActiveResultPanel()
        {
            ResultPanel.gameObject.SetActive(true);
            _vc.Sound.DisableAllSound();
        }
        public void SetPromptText(string text, float duration = 5f)
        {
            PromptText.text = text;
            if (_textTween != null && _textTween.IsPlaying())
            {
                _textTween.Restart();
            }
            else
            {
                PromptText.color = new Color(1f, 1f, 1f, 1);
                _textTween = DOTween
                    .To(() => PromptText.color, (newcolor) => PromptText.color = newcolor, new Color(0f, 0f, 0f, 0), duration)
                    .SetEase(Ease.InSine);
            }
        }

        public void SetResultText(string text)
        {
            PromptText.text = "";
            ResultText.text = text;
        }

        private void OnReStart()
        {
            Time.timeScale = 1f;
            DOTween.Clear(true);
            Utility.LoadScene(2);
        }

        private void OnExit()
        {
            Time.timeScale = 1f;
            DOTween.Clear(true);
            UnitySingleton<Main>.GetInstance().CurrentCourse = Course.None;
            Utility.LoadScene(0);
        }
    }
}
