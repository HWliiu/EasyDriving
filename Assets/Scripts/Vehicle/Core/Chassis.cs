using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class Chassis
    {
        [Serializable]
        public struct Axle : IEnumerable<WheelCollider>
        {
            // ReSharper disable once UnassignedField.Global
            public WheelCollider LeftWheelCollider;
            // ReSharper disable once UnassignedField.Global
            public WheelCollider RightWheelCollider;
            // ReSharper disable once UnassignedField.Global
            public Transform LeftVisualWheel;
            // ReSharper disable once UnassignedField.Global
            public Transform RightVisualWheel;
            public IEnumerator<WheelCollider> GetEnumerator()
            {
                yield return LeftWheelCollider;
                yield return RightWheelCollider;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        // ReSharper disable once UnassignedField.Global
        public Axle FrontAxle;
        // ReSharper disable once UnassignedField.Global
        public Axle RearAxle;

        private VehicleController _vc;
        private Axle[] _axles;

        public float PowerWheelRpm => (FrontAxle.LeftWheelCollider.rpm + FrontAxle.RightWheelCollider.rpm) / 2.0f;    //驱动轮的平均转速

        public void Initialize(VehicleController vc)
        {
            this._vc = vc;
            _axles = new Axle[] { FrontAxle, RearAxle };
            ResetCOM();
        }

        public void AntiRollBar()
        {
            foreach (var axle in _axles)
            {
                var travelL = 1.0f;
                var travelR = 1.0f;
                //计算两侧轮胎在不同情况下的悬挂系数
                var wheelL = axle.LeftWheelCollider;
                if (wheelL.GetGroundHit(out var hit))
                {
                    travelL = (-wheelL.transform.InverseTransformPoint(hit.point).y - wheelL.radius) / wheelL.suspensionDistance;
                }
                var wheelR = axle.RightWheelCollider;
                if (wheelR.GetGroundHit(out hit))
                {
                    travelR = (-wheelR.transform.InverseTransformPoint(hit.point).y - wheelR.radius) / wheelR.suspensionDistance;
                }
                //计算平衡杆刚度系数
                var antiRollForce = (travelL - travelR) * (wheelL.suspensionSpring.spring + wheelR.suspensionSpring.spring) / 2;
                //向两侧的轮胎分配力
                _vc.Rigidbody.AddForceAtPosition(wheelL.transform.up * -antiRollForce, wheelL.transform.position);
                _vc.Rigidbody.AddForceAtPosition(wheelR.transform.up * antiRollForce, wheelR.transform.position);
            }
        }

        public void SyncVisualWheels()
        {
            foreach (var axle in _axles)
            {
                // ReSharper disable once IdentifierTypo
                axle.LeftWheelCollider.GetWorldPose(out var pos, out var quat);
                axle.LeftVisualWheel.SetPositionAndRotation(pos, quat);

                axle.RightWheelCollider.GetWorldPose(out pos, out quat);
                axle.RightVisualWheel.SetPositionAndRotation(pos, quat);
            }
        }

        private void ResetCOM()
        {
            var centre = Vector3.zero;
            var front = Vector3.zero;
            for (var index = 0; index < _axles.Length; index++)
            {
                foreach (var wheel in _axles[index])
                {
                    wheel.GetWorldPose(out var pos, out _);
                    centre += pos;
                    if (index == 0) front += pos;
                }
            }
            centre = centre / (_axles.Length * 2);
            front = front / 2;
            var com = _vc.transform.InverseTransformPoint((centre + front) / 2);
            _vc.Rigidbody.centerOfMass = com;
        }

        public void Steer(float angle, float ackermannPercent)
        {
            // 应用Ackermann
            if (angle < 0)     //左转弯
            {
                FrontAxle.LeftWheelCollider.steerAngle = angle;
                FrontAxle.RightWheelCollider.steerAngle = angle - (angle * ackermannPercent);
            }
            else if (angle > 0)    //右转弯
            {
                FrontAxle.LeftWheelCollider.steerAngle = angle - (angle * ackermannPercent);
                FrontAxle.RightWheelCollider.steerAngle = angle;
            }
            else //直行
            {
                FrontAxle.LeftWheelCollider.steerAngle = angle;
                FrontAxle.RightWheelCollider.steerAngle = angle;
            }
        }

        public void Update() => SyncVisualWheels();

        public void FixedUpdate()
        {
            //AntiRollBar();
        }

        public void TorqueSplit(float torque, float topRpm)
        {
            //驱动轮转速过高时切断动力
            var torqueFactor = PowerWheelRpm < topRpm * 2.2f ? 1 : 0;
            foreach (var wheel in FrontAxle)
                wheel.motorTorque = torque * torqueFactor;
        }
        public void ResetBrakes()
        {
            foreach (var axle in _axles)
                foreach (var wheel in axle)
                    wheel.brakeTorque = 0f;
        }
        public void AddBrakeTorque(float torque, bool onlyPowerWheel = false)
        {
            foreach (var wheel in FrontAxle)
                wheel.brakeTorque += torque;

            if (!onlyPowerWheel)
                foreach (var wheel in RearAxle)
                    wheel.brakeTorque += torque * 0.2f;
        }

    }
}
