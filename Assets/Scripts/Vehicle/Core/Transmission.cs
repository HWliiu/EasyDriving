using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class Transmission
    {
        [Tooltip("R挡,N挡,1,2,3,4,5挡齿轮比")]
        public List<float> GearsRatio = new List<float>() { -5f, 0f, 8f, 5.5f, 4f, 3f, 2.2f };
        public float GearMultiplier = 3.3f;
        public AnimationCurve ClutchEngagementCurve = new AnimationCurve(new Keyframe[2] {
            new Keyframe(0f, 1f,-1f,-1f),
            new Keyframe(1f, 0f,-1f,-1f)
        });
        public readonly float ShiftDuration = 0.8f;

        private VehicleController _vc;
        private int _gear = 0;
        private readonly float _postShiftBan = 0.2f;
        private float _lastShiftTime = 0f;
        private float _clutchDelay = 0.2f;
        //SmoothDamp的传入参数
        private float _clutchVelocity = 0f;


        public int Gear
        {
            get => _gear;
            set
            {
                if (value < -1)
                    _gear = -1;
                else if (value > GearsRatio.Count - 2)
                    _gear = GearsRatio.Count - 2;
                else
                    _gear = value;
            }
        }
        public float GearRatio => GearsRatio[Gear + 1] * GearMultiplier;
        public float AddedClutchRPM { get; private set; }   //离合器踩下时补偿的引擎转速
        public float Clutch { get; private set; }
        public float ReverseRpm => _vc.Chassis.PowerWheelRpm * GearRatio;   //从驱动轮转速推算出的引擎转速
        public bool CanShift => Time.realtimeSinceStartup > _lastShiftTime + ShiftDuration + _postShiftBan;
        public bool Shifting => Time.realtimeSinceStartup < _lastShiftTime + ShiftDuration;


        public void Initialize(VehicleController vc) => this._vc = vc;

        public void UpdateShift()
        {
            if (!CanShift) return;

            if (_vc.Input.ShiftUp)
            {
                if (!(Gear == 0 && _vc.ForwardVelocity < -2f))
                {
                    Gear++;
                    _lastShiftTime = Time.realtimeSinceStartup;
                }
            }
            if (_vc.Input.ShiftDown)
            {
                if (!(Gear == 0 && _vc.ForwardVelocity > 2f))
                {
                    Gear--;
                    _lastShiftTime = Time.realtimeSinceStartup;
                }
            }

            _vc.Input.ShiftUp = false;
            _vc.Input.ShiftDown = false;
        }

        public void UpdateClutch()
        {
            Clutch = Mathf.SmoothDamp(Clutch, _vc.Input.Clutch, ref _clutchVelocity, _clutchDelay);
            AddedClutchRPM = (1f - ClutchEngagementCurve.Evaluate(Clutch)) * (_vc.Engine.MaxRpm - _vc.Engine.MinRpm) * Mathf.Abs(_vc.Engine.Throttle);
        }

        public void FixedUpdate()
        {
            if (_vc.Input == null) return;
            UpdateShift();
            UpdateClutch();
        }

        public void TorqueSplit(float torque, float topRPM)
        {
            //根据当前的挡位和离合器状态修正扭矩的大小和方向
            torque = torque * (Gear != 0 ? Mathf.Sign(Gear) : 0) * ClutchEngagementCurve.Evaluate(Clutch);
            _vc.Chassis.TorqueSplit(torque, topRPM);
        }

        public bool ShiftInto(int gear, out int prevGear)
        {
            prevGear = Gear;

            if (!CanShift) return false;
            if (gear > 0 && _vc.ForwardVelocity < -2f) return false;
            if (gear < 0 && _vc.ForwardVelocity > 2f) return false;

            Gear = gear;
            if (Gear != 0 && Gear != prevGear)
                _lastShiftTime = Time.realtimeSinceStartup;

            return true;
        }
    }
}
