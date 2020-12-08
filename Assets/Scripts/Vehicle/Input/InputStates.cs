using System;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class InputStates
    {
        [HideInInspector]
        public bool Settable = true;
        [SerializeField, Range(-1, 1)] private float _steering;
        [SerializeField, Range(0, 1)] private float _throttle;
        [SerializeField, Range(0, 1)] private float _brake;
        [SerializeField, Range(0, 1)] private float _clutch;
        [SerializeField] private bool _shiftUp;
        [SerializeField] private bool _shiftDown;
        [SerializeField] private bool _handbrake;
        [SerializeField] private bool _leftBlinker;
        [SerializeField] private bool _rightBlinker;

        public float Steering
        {
            get => _steering;
            set
            {
                if (Settable)
                {
                    _steering = Mathf.Clamp(value, -1f, 1f);
                }
                else
                {
                    _steering = 0;
                }
            }
        }

        public float Throttle
        {
            get => _throttle;
            set
            {
                if (Settable)
                {
                    _throttle = Mathf.Clamp01(value);
                }
                else
                {
                    _throttle = 0;
                }
            }
        }

        public float Brake
        {
            get => _brake;
            set
            {
                if (Settable)
                {
                    _brake = Mathf.Clamp01(value);
                }
                else
                {
                    _brake = 0;
                }
            }
        }

        public float Clutch
        {
            get => _clutch;
            set
            {
                if (Settable)
                {
                    _clutch = Mathf.Clamp01(value);
                }
                else
                {
                    _clutch = 0;
                }
            }
        }

        public bool Handbrake
        {
            get => _handbrake;
            set => _handbrake = value;
        }


        public bool ShiftUp
        {
            get => _shiftUp;
            set => _shiftUp = value;
        }

        public bool ShiftDown
        {
            get => _shiftDown;
            set => _shiftDown = value;
        }

        public bool LowBeamLights;
        public bool FullBeamLights;
        public bool HazardLights;

        public bool Horn { get; set; }

        public bool LeftBlinker
        {
            get => _leftBlinker;
            set
            {
                _leftBlinker = value;
                if (value && RightBlinker) RightBlinker = false;
            }
        }

        public bool RightBlinker
        {
            get => _rightBlinker;
            set
            {
                _rightBlinker = value;
                if (value && LeftBlinker) LeftBlinker = false;
            }
        }
    }
}
