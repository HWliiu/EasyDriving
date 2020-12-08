using System;
using UnityEngine;

namespace EasyDriving
{
    public class StopOffDetector : MonoBehaviour
    {
        public float MaxStopOffTime = 2f;

        public event EventHandler OnStopOffTimeOut;

        public float AccumulateStopOffTime { get; private set; }

        private bool _isLFWheelInside;
        private bool _isRFWheelInside;
        private bool _isLRWheelInside;
        private bool _isRRWheelInside;

        private bool IsFourWheelInside => _isLFWheelInside && _isRFWheelInside && _isLRWheelInside && _isRRWheelInside;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnTriggerEnter(Collider other) => DetectWheelEnter(other);

        private void OnTriggerExit(Collider other) => DetectWheelExit(other);


        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(TagConst.VehicleBody) && IsFourWheelInside)
            {
                var velocity = other.attachedRigidbody.velocity.sqrMagnitude;
                if (Math.Abs(velocity) < 0.01f)
                {
                    StartTimer();
                }
                else
                {
                    ResetTimer();
                }
            }
        }

        private void StartTimer()
        {
            AccumulateStopOffTime += Time.deltaTime;
            if (AccumulateStopOffTime > MaxStopOffTime)
            {
                OnStopOffTimeOut?.Invoke(this, null);
                ResetTimer();
            }
        }

        private void ResetTimer() => AccumulateStopOffTime = 0f;

        private void DetectWheelEnter(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel))
            {
                _isLFWheelInside = true;
                return;
            }

            if (other.CompareTag(TagConst.RightFrontWheel))
            {
                _isRFWheelInside = true;
                return;
            }

            if (other.CompareTag(TagConst.LeftRearWheel))
            {
                _isLRWheelInside = true;
                return;
            }

            if (other.CompareTag(TagConst.RightRearWheel))
            {
                _isRRWheelInside = true;
            }
        }
        private void DetectWheelExit(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel))
            {
                _isLFWheelInside = false;
                return;
            }

            if (other.CompareTag(TagConst.RightFrontWheel))
            {
                _isRFWheelInside = false;
                return;
            }

            if (other.CompareTag(TagConst.LeftRearWheel))
            {
                _isLRWheelInside = false;
                return;
            }

            if (other.CompareTag(TagConst.RightRearWheel))
            {
                _isRRWheelInside = false;
            }
        }
    }
}
