using System;
using UnityEngine;

namespace EasyDriving
{
    public class CFTCManager : GameManager
    {
        public WheelStateDetector ParkDetector;
        public StopOffDetector StopOffDetector;

        private bool[] _stepsFinished;

        private bool _detectTurnLightFlag;
        private bool _turnLightFlag;

        // Start is called before the first frame update
        void Start() => Initialize();

        private void Update()
        {
            base.Update();
            if (_detectTurnLightFlag)
            {
                DetectTurnLight();
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            RegisterInitialEvent();
            _stepsFinished = new bool[4];
        }

        protected override void RegisterInitialEvent()
        {
            base.RegisterInitialEvent();

            foreach (var sideline in SidelineDetectors)
            {
                sideline.OnVehicleBodyOutlet += OnBodyOutlet;
            }

            EntranceDetector.OnRearWheelEnter += OnStepOneFinished;
        }

        private void OnStepOneFinished(object o, EventArgs e)
        {
            if (!_stepsFinished[0])
            {
                _stepsFinished[0] = true;
                GUI.SetPromptText("考试开始");
                StartTimer(210f);
                ExitDetector.OnFrontWheelEnterAndStop += OnStepTwoFinished;
                ParkDetector.OnAllWheelEnterAndStop += OnStepThreeFinished;
                ExitDetector.OnRearWheelEnter += OnOutOfRange;
                EntranceDetector.OnFrontWheelEnter += OnOutOfRange;
                StopOffDetector.OnStopOffTimeOut += OnStopOffTimeOut;
                EntranceDetector.OnRearWheelEnter -= OnStepOneFinished;
            }
            else
            {
                OnErrorPath();
            }

        }

        private void OnStepTwoFinished(object o, EventArgs e)
        {
            if (!_stepsFinished[1])
            {
                _stepsFinished[1] = true;
                GUI.SetPromptText("将车倒入车库");
            }
            else
            {
                OnErrorPath();
            }
        }

        private void OnStepThreeFinished(object o, EventArgs e)
        {
            if (!_stepsFinished[2])
            {
                if (_stepsFinished[1])
                {
                    _stepsFinished[2] = true;
                    GUI.SetPromptText("停车已到位");
                    ExitDetector.OnFrontWheelEnterAndStop -= OnStepTwoFinished;
                    ExitDetector.OnRearWheelEnter -= OnOutOfRange;
                    ExitDetector.OnRearWheelEnter += OnAllStepFinished;
                    _detectTurnLightFlag = true;
                }
                else
                {
                    OnErrorPath();
                }

            }
            else
            {
                OnErrorPath();
            }
        }

        private void OnAllStepFinished(object o, EventArgs e) => GUI.OnSuccess();

        private void OnStopOffTimeOut(object o, EventArgs e)
        {
            GUI.SetPromptText("停车时间超过2秒 扣10分");
            Score -= 10;
        }

        private void DetectTurnLight()
        {
            if (VC.SpeedKPH > 1f && !_turnLightFlag)
            {
                if (!VC.Input.LeftBlinker && !VC.Input.RightBlinker)
                {
                    GUI.SetPromptText("未使用转向灯 扣10分");
                    Score -= 10;
                    _turnLightFlag = true;
                }
                else if (VC.Input.RightBlinker)
                {
                    GUI.SetPromptText("错误使用转向灯 扣10分");
                    Score -= 10;
                    _turnLightFlag = true;
                }
            }
        }
    }
}
