﻿using System;
using UnityEngine;

namespace EasyDriving
{
    public class QXXSManager : GameManager
    {
        public StopOffDetector StopOffDetector;

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
            StopOffDetector.MaxStopOffTime = 0.2f;
        }

        protected override void RegisterInitialEvent()
        {
            base.RegisterInitialEvent();

            foreach (var sideline in SidelineDetectors)
            {
                sideline.OnWheelOutlet += OnWheelOutlet;
            }

            EntranceDetector.OnFrontWheelEnter += OnStepOneFinished;
            ExitDetector.OnRearWheelEnter += OnAllStepFinished;

            StopOffDetector.OnStopOffTimeOut += OnStopOffTimeOut;
        }

        private void OnStepOneFinished(object o, EventArgs e)
        {
            GUI.SetPromptText("考试开始");
            EntranceDetector.OnFrontWheelEnter -= OnStepOneFinished;
        }

        private void OnAllStepFinished(object o, EventArgs e) => GUI.OnSuccess();

        private void OnStopOffTimeOut(object o, EventArgs e)
        {
            GUI.OnFailure("中途停车 开始失败");
        }
    }
}
