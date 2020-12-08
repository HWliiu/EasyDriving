using System;
using UnityEngine;

namespace EasyDriving
{
    public class PTQBManager : GameManager
    {
        public WheelStateDetector StopAreaDetector;
        public DistanceDetector FrontDistanceDetector;
        public DistanceDetector SideDistanceDetector;

        private bool[] _stepsFinished;

        private bool _detectTurnLightFlag;
        private bool _turnLightFlag;
        private bool _reverseFlag;

        private float _stopPointDistance;

        void Start() => Initialize();

        private void Update()
        {
            base.Update();
            FrontDistanceDetector.DetectDistance();
            SideDistanceDetector.DetectDistance();
            //Debug.Log($"FrontDistanceDetector:{FrontDistanceDetector.Point1Distance} {FrontDistanceDetector.Point2Distance}");
            //Debug.Log($"SideDistanceDetector:{SideDistanceDetector.Point1Distance} {SideDistanceDetector.Point2Distance}");
            if (_reverseFlag)
            {
                DetectReverse();
            }

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
                sideline.OnWheelOutlet += OnWheelOutlet;
            }

            EntranceDetector.OnFrontWheelEnter += OnStepOneFinished;
            StopAreaDetector.OnAllWheelEnterAndStop += OnStepTwoFinished;
            ExitDetector.OnRearWheelEnter += OnAllStepFinished;
        }

        private void OnStepOneFinished(object o, EventArgs e)
        {
            if (!_stepsFinished[0])
            {
                _stepsFinished[0] = true;
                GUI.SetPromptText("考试开始");
                StartTimer(210f);
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
                FrontDistanceDetector.DetectDistance();
                SideDistanceDetector.DetectDistance();
                _stopPointDistance = FrontDistanceDetector.Point1Distance;
                if (FrontDistanceDetector.Point1Distance < 0.6f || FrontDistanceDetector.Point2Distance < 0.6f)
                {
                    if (SideDistanceDetector.Point1Distance < 0.4f && SideDistanceDetector.Point2Distance < 0.4f)
                    {
                        GUI.SetPromptText("停车已到位");
                        _stepsFinished[1] = true;
                        _reverseFlag = true;
                        _turnLightFlag = true;
                    }
                    else
                    {
                        GUI.OnFailure("车身距离边线超过30cm");
                    }
                }
                else
                {
                    GUI.OnFailure("车头未停于桩杆线50cm内");
                }
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

        private void OnAllStepFinished(object o, EventArgs e)
        {
            if (_stepsFinished[2])
            {
                GUI.OnSuccess();
            }
            else
            {
                GUI.OnFailure("超出考试范围");
            }
        }

        private void DetectReverse()
        {
            FrontDistanceDetector.DetectDistance();
            if (VC.ForwardVelocity < 0f && FrontDistanceDetector.Point1Distance - _stopPointDistance > 0.5f)
            {
                GUI.OnFailure("溜车距离大于50cm");
            }
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
