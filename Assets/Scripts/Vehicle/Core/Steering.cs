using System;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class Steering
    {
        [Range(0f, 60f)]
        public float LowSpeedAngle = 45f;
        [Range(0f, 60f)]
        public float HighSpeedAngle = 20f;
        public float CrossoverSpeed = 35f;
        [Range(0f, 360f)]
        public float DegreesPerSecondLimit = 250f;
        [Range(-1f, 1f)]
        public float AckermannPercent = 0.15f;
        [Tooltip("轮子的转向角乘以这个数即为方向盘的转动角")]
        public float SteeringWheelTurnRatio = 10f;
        public GameObject SteeringWheel;

        private float _angle;
        private float _targetAngle;
        private Chassis.Axle _frontAxle;
        private Vector3 _initialSteeringWheelRotation;
        private VehicleController _vc;
        public float Angle => _angle;

        public void Initialize(VehicleController vc)
        {
            this._vc = vc;
            if (SteeringWheel != null)
            {
                _initialSteeringWheelRotation = SteeringWheel.transform.localRotation.eulerAngles;
            }
        }

        public void FixedUpdate()
        {
            var maxAngle = Mathf.Abs(Mathf.Lerp(LowSpeedAngle, HighSpeedAngle, _vc.SpeedKPH / CrossoverSpeed));
            _targetAngle = maxAngle * _vc.Input.Steering;
            _angle = Mathf.MoveTowards(_angle, _targetAngle, DegreesPerSecondLimit * Time.fixedDeltaTime);

            _vc.Chassis.Steer(_angle, AckermannPercent);

            if (SteeringWheel != null)
            {
                float wheelAngle = _angle * SteeringWheelTurnRatio;
                SteeringWheel.transform.localRotation = Quaternion.Euler(_initialSteeringWheelRotation);
                SteeringWheel.transform.Rotate(Vector3.back, wheelAngle);
            }
        }
    }
}
