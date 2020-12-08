using System;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class Brakes
    {
        public float MaxTorque = 1500f;
        public float EngineBrakeTorque = 800f;
        public float SpeedThreshold = 30f;

        public AnimationCurve BrakeTorqueCurve = new AnimationCurve(new Keyframe[2] {
            new Keyframe(0f, 0.5f,0.5f,0.5f),
            new Keyframe(1f, 1f,0.5f,0.5f)
        });

        private VehicleController _vc;


        public bool Braking => !_vc.Input.Brake.IsDeadzoneZero();


        public void Initialize(VehicleController vc) => this._vc = vc;

        public void FixedUpdate()
        {
            if (_vc.Input == null) return;
            //重置当前帧的BrakeTorque
            _vc.Chassis.ResetBrakes();

            //低速时降低BrakeTorque
            var brakingIntensity = BrakeTorqueCurve.Evaluate(Mathf.Clamp01(_vc.SpeedKPH / SpeedThreshold));
            //引擎制动
            var brakeTorque = EngineBrakeTorque * (1 - _vc.Transmission.Clutch) * brakingIntensity * Mathf.Clamp01(Mathf.Sin(Time.frameCount));
            if (_vc.SpeedKPH > _vc.TopSpeedForGear)
            {
                _vc.Chassis.AddBrakeTorque(brakeTorque, true);
            }
            if (_vc.Input.Throttle.IsDeadzoneZero() && _vc.Transmission.Gear != 0 && !_vc.Transmission.Shifting && _vc.SpeedKPH > _vc.Engine.IdlingMaxSpeed)
            {
                _vc.Chassis.AddBrakeTorque(brakeTorque, true);
            }
            //脚刹
            brakeTorque = MaxTorque * _vc.Input.Brake * brakingIntensity * Mathf.Abs(Mathf.Sin(Time.frameCount));
            _vc.Chassis.AddBrakeTorque(brakeTorque);
            //手刹
            if (_vc.Input.Handbrake)
                _vc.Chassis.AddBrakeTorque(99999f);
        }
    }
}
