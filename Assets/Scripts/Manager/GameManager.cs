using System;
using UnityEngine;

namespace EasyDriving
{
    public class GameManager : MonoBehaviour
    {
        public VehicleController VC;
        public GUIManager GUI;
        public Transform StartPoint;
        public WheelStateDetector EntranceDetector;
        public WheelStateDetector ExitDetector;
        public SidelineDetector[] SidelineDetectors;

        protected event EventHandler OnTimeOut;

        private float _startTime;
        private bool _tic;
        private float _maxTime;
        private bool _timeOutFlag;

        private int _score;

        public int Score
        {
            get => _score;
            set
            {
                _score = value;
                if (_score < 80) OnLowScore();
            }
        }

        // Update is called once per frame
        protected void Update() => DetectTimeOut();

        protected virtual void Initialize()
        {
            Score = 100;
            VC.transform.SetPositionAndRotation(StartPoint.position, StartPoint.rotation);
        }

        protected virtual void RegisterInitialEvent()
        {
        }

        private void DetectTimeOut()
        {
            if (_tic && Time.time - _startTime > _maxTime && !_timeOutFlag)
            {
                OnTimeOut?.Invoke(this, null);
                _timeOutFlag = true;
            }
        }

        protected void StartTimer(float maxTime)
        {
            _startTime = Time.time;
            _maxTime = maxTime;
            _tic = true;
            _timeOutFlag = false;
        }

        protected void CancelTimer() => _tic = false;

        protected void OnBodyOutlet(object o, OutletEventArgs e)
        {
            GUI.OnFailure("车身出线 不合格");
            var source = o as SidelineDetector;
            if (source != null) source.GetComponent<MeshRenderer>().enabled = true;
        }

        protected void OnWheelOutlet(object o, OutletEventArgs e)
        {
            GUI.OnFailure("车身出线 不合格");
            var source = o as SidelineDetector;
            if (source != null) source.GetComponent<MeshRenderer>().enabled = true;
        }

        protected void OnOutOfRange(object o, EventArgs e) => GUI.OnFailure("超出考试范围");

        protected void OnErrorPath() => GUI.OnFailure("未按规定路线行驶");

        private void OnLowScore() => GUI.OnFailure("成绩低于80分");
    }
}
