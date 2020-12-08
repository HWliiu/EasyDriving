using System;
using UnityEngine;

namespace EasyDriving
{
    public class WheelStateDetector : MonoBehaviour
    {
        public bool IsLFWheelInside { get; private set; }
        public bool IsRFWheelInside { get; private set; }
        public bool IsLRWheelInside { get; private set; }
        public bool IsRRWheelInside { get; private set; }

        public event EventHandler OnFrontWheelEnterAndStop;
        private bool _onFWRaiseFlag;

        public event EventHandler OnAllWheelEnterAndStop;
        private bool _onAWRaiseFlag;

        public event EventHandler OnRearWheelEnter;
        public event EventHandler OnFrontWheelEnter;

        private void Start()
        {
            //OnRearWheelEnter += (t, n) => Debug.Log("OnRearWheelEnter");
        }

        private void Update()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            DetectWheelEnter(other);
            DetectRearWheelEnter(other);
            DetectFrontWheelEnter(other);
        }

        private void OnTriggerExit(Collider other)
        {
            DetectWheelExit(other);
            ResetFrontWheelEnterAndStopFlag(other);
            ResetAllWheelEnterAndStopFlag(other);
        }

        private void OnTriggerStay(Collider other)
        {
            DetectFrontWheelEnterAndStop(other);
            DetectAllWheelEnterAndStop(other);
        }

        private void DetectWheelEnter(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel))
            {
                IsLFWheelInside = true;
                return;
            }

            if (other.CompareTag(TagConst.RightFrontWheel))
            {
                IsRFWheelInside = true;
                return;
            }

            if (other.CompareTag(TagConst.LeftRearWheel))
            {
                IsLRWheelInside = true;
                return;
            }

            if (other.CompareTag(TagConst.RightRearWheel))
            {
                IsRRWheelInside = true;
            }
        }
        private void DetectWheelExit(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel))
            {
                IsLFWheelInside = false;
                return;
            }

            if (other.CompareTag(TagConst.RightFrontWheel))
            {
                IsRFWheelInside = false;
                return;
            }

            if (other.CompareTag(TagConst.LeftRearWheel))
            {
                IsLRWheelInside = false;
                return;
            }

            if (other.CompareTag(TagConst.RightRearWheel))
            {
                IsRRWheelInside = false;
            }
        }

        private void DetectFrontWheelEnterAndStop(Collider other)
        {
            if (IsLFWheelInside && IsRFWheelInside && other.attachedRigidbody.velocity.sqrMagnitude < 0.01f && !_onFWRaiseFlag)
            {
                OnFrontWheelEnterAndStop?.Invoke(this, null);
                _onFWRaiseFlag = true;
            }
        }
        private void ResetFrontWheelEnterAndStopFlag(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel) && IsRFWheelInside == false)
            {
                _onFWRaiseFlag = false;
                return;
            }
            if (other.CompareTag(TagConst.RightFrontWheel) && IsLFWheelInside == false)
            {
                _onFWRaiseFlag = false;
            }
        }

        private void DetectAllWheelEnterAndStop(Collider other)
        {
            if (IsLFWheelInside && IsRFWheelInside && IsLRWheelInside && IsRRWheelInside &&
                other.attachedRigidbody.velocity.sqrMagnitude < 0.01f && !_onAWRaiseFlag)
            {
                OnAllWheelEnterAndStop?.Invoke(this, null);
                _onAWRaiseFlag = true;
            }
        }
        private void ResetAllWheelEnterAndStopFlag(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel) && IsRFWheelInside == false && IsLRWheelInside == false && IsRRWheelInside == false)
            {
                _onAWRaiseFlag = false;
                return;
            }
            if (IsLFWheelInside == false && other.CompareTag(TagConst.RightFrontWheel) && IsLRWheelInside == false && IsRRWheelInside == false)
            {
                _onAWRaiseFlag = false;
                return;
            }
            if (IsLFWheelInside == false && IsRFWheelInside == false && other.CompareTag(TagConst.LeftRearWheel) && IsRRWheelInside == false)
            {
                _onAWRaiseFlag = false;
                return;
            }
            if (IsLFWheelInside == false && IsRFWheelInside == false && IsLRWheelInside == false && other.CompareTag(TagConst.RightRearWheel))
            {
                _onAWRaiseFlag = false;
            }
        }

        private void DetectRearWheelEnter(Collider other)
        {
            if (other.CompareTag(TagConst.LeftRearWheel) && IsRRWheelInside)
            {
                OnRearWheelEnter?.Invoke(this, null);
                return;
            }

            if (other.CompareTag(TagConst.RightRearWheel) && IsLRWheelInside)
            {
                OnRearWheelEnter?.Invoke(this, null);
            }
        }

        private void DetectFrontWheelEnter(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel) && IsRFWheelInside)
            {
                OnFrontWheelEnter?.Invoke(this, null);
                return;
            }

            if (other.CompareTag(TagConst.RightFrontWheel) && IsLFWheelInside)
            {
                OnFrontWheelEnter?.Invoke(this, null);
            }
        }
    }
}
