using System;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;

namespace EasyDriving
{
    public class DCRKManager : GameManager
    {
        public WheelStateDetector ParkDetector;
        public StopOffDetector StopOffDetector;

        private bool[] _stepsFinished;

        // Start is called before the first frame update
        void Start() => Initialize();

        private void Update()
        {
            base.Update();
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

            ExitDetector.OnFrontWheelEnterAndStop += OnStepOneFinished;
            ParkDetector.OnAllWheelEnterAndStop += OnStepTwoFinished;

            ExitDetector.OnRearWheelEnter += OnOutOfRange;
            EntranceDetector.OnFrontWheelEnter += OnOutOfRange;

            StopOffDetector.OnStopOffTimeOut += OnStopOffTimeOut;
        }

        private void OnStepOneFinished(object o, EventArgs e)
        {
            if (!_stepsFinished[0])
            {
                _stepsFinished[0] = true;   //右边线停车成功
                EntranceDetector.OnFrontWheelEnter -= OnOutOfRange;
                EntranceDetector.OnRearWheelEnter += OnOutOfRange;
                GUI.SetPromptText("考试开始");
                StartTimer(210f);
            }
            else
            {
                //判断是否多次右边线停车
                OnErrorPath();
            }
        }

        private void OnStepTwoFinished(object o, EventArgs e)
        {
            if (_stepsFinished[2] && !_stepsFinished[3])  //判断左边线停车操作是否完成
            {
                _stepsFinished[3] = true;
                ExitDetector.OnRearWheelEnter -= OnOutOfRange;
                ExitDetector.OnFrontWheelEnterAndStop -= OnStepOneFinished;
                ExitDetector.OnRearWheelEnter += OnAllStepFinished;
                GUI.SetPromptText("左入库操作完成");
                return;
            }

            if (_stepsFinished[0] && !_stepsFinished[1])  //判断右边线停车操作是否完成
            {
                _stepsFinished[1] = true;
                EntranceDetector.OnFrontWheelEnterAndStop += OnStepThreeFinished;
                GUI.SetPromptText("右入库操作完成");
                return;
            }
            //判断是否出库后直接入库
            OnErrorPath();
        }

        private void OnStepThreeFinished(object o, EventArgs e)
        {
            if (!_stepsFinished[2])
            {
                _stepsFinished[2] = true;   //右边线停车成功
                EntranceDetector.OnFrontWheelEnterAndStop -= OnStepThreeFinished;
                GUI.SetPromptText("将车再次倒入车库");
            }
            else
            {
                //判断是否多次左边线停车
                OnErrorPath();
            }
        }

        private void OnAllStepFinished(object o, EventArgs e) => GUI.OnSuccess();

        private void OnStopOffTimeOut(object o, EventArgs e)
        {
            GUI.SetPromptText("停车时间超过2秒 扣10分", 1.5f);
            Score -= 10;
        }
    }
}
