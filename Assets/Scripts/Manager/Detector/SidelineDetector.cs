using System;
using UnityEngine;

namespace EasyDriving
{
    public class SidelineDetector : MonoBehaviour
    {
        public event EventHandler<OutletEventArgs> OnWheelOutlet;
        public event EventHandler<OutletEventArgs> OnVehicleBodyOutlet;

        private void Start()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            DetectWheelOutlet(other);
            DetectVehicleBodyOutlet(other);
        }

        private void DetectWheelOutlet(Collider other)
        {
            if (other.CompareTag(TagConst.LeftFrontWheel))
            {
                OnWheelOutlet?.Invoke(this, new OutletEventArgs(TagConst.LeftFrontWheel));
                return;
            }
            if (other.CompareTag(TagConst.RightFrontWheel))
            {
                OnWheelOutlet?.Invoke(this, new OutletEventArgs(TagConst.RightFrontWheel));
                return;
            }
            if (other.CompareTag(TagConst.LeftRearWheel))
            {
                OnWheelOutlet?.Invoke(this, new OutletEventArgs(TagConst.LeftRearWheel));
                return;
            }
            if (other.CompareTag(TagConst.RightRearWheel))
            {
                OnWheelOutlet?.Invoke(this, new OutletEventArgs(TagConst.RightRearWheel));
            }
        }

        private void DetectVehicleBodyOutlet(Collider other)
        {
            if (other.CompareTag(TagConst.VehicleBody))
            {
                OnVehicleBodyOutlet?.Invoke(this, new OutletEventArgs(TagConst.VehicleBody));
            }
        }
    }

    public class OutletEventArgs : EventArgs
    {
        public string WhichOutlet { get; set; }
        public OutletEventArgs(string whichOutlet) => WhichOutlet = whichOutlet;
    }

}
