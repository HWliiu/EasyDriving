using System;
using UnityEngine;
using UnityEngine.UI;

namespace EasyDriving
{
    public class VehicleGUIController : MonoBehaviour
    {
        public VehicleController VehicleController;

        public AnalogGauge AnalogRpmGauge;
        public AnalogGauge AnalogSpeedGauge;

        [Serializable]
        public class DashLight
        {
            public Image Icon;
            private bool _active;

            private Color _originalColor;
            private Color _activeColor = Color.white;

            public void Initialize()
            {
                if (Icon != null)
                    _originalColor = Icon.color;
                else
                    throw new Exception("Icon is null");
            }

            public bool Active
            {
                get => _active;
                set
                {
                    _active = value;
                    Icon.color = _active ? _activeColor : _originalColor;
                }
            }
        }

        public DashLight leftBlinker = new DashLight();
        public DashLight rightBlinker = new DashLight();
        public DashLight lowBeam = new DashLight();
        public DashLight highBeam = new DashLight();
        public DashLight TCS = new DashLight();
        public DashLight ABS = new DashLight();
        public DashLight checkEngine = new DashLight();

        void Start()
        {
            leftBlinker.Initialize();
            rightBlinker.Initialize();
            lowBeam.Initialize();
            highBeam.Initialize();
            TCS.Initialize();
            ABS.Initialize();
            checkEngine.Initialize();
        }

        void Update()
        {
            if (VehicleController != null)
            {
                if (AnalogRpmGauge != null) AnalogRpmGauge.Value = VehicleController.Engine.Rpm;
                if (AnalogSpeedGauge != null) AnalogSpeedGauge.Value = VehicleController.SpeedKPH;

                if (leftBlinker != null) leftBlinker.Active = VehicleController.Lights.LeftBlinkers.On;
                if (rightBlinker != null) rightBlinker.Active = VehicleController.Lights.RightBlinkers.On;
                if (lowBeam != null) lowBeam.Active = VehicleController.Lights.HeadLights.On;
                if (highBeam != null) highBeam.Active = VehicleController.Lights.FullBeams.On;

                if (VehicleController.Engine.Starting)
                {
                    if (leftBlinker != null) leftBlinker.Active = true;
                    if (rightBlinker != null) rightBlinker.Active = true;
                    if (lowBeam != null) lowBeam.Active = true;
                    if (highBeam != null) highBeam.Active = true;
                    if (TCS != null) TCS.Active = true;
                    if (ABS != null) ABS.Active = true;
                    if (checkEngine != null) checkEngine.Active = true;
                }
                else if (VehicleController.Engine.Stopping)
                {
                    if (leftBlinker != null) leftBlinker.Active = false;
                    if (rightBlinker != null) rightBlinker.Active = false;
                    if (lowBeam != null) lowBeam.Active = false;
                    if (highBeam != null) highBeam.Active = false;
                    if (TCS != null) TCS.Active = false;
                    if (ABS != null) ABS.Active = false;
                    if (checkEngine != null) checkEngine.Active = false;
                }
                else if (VehicleController.Engine.IsRunning)
                {
                    if (checkEngine != null) checkEngine.Active = false;
                    
                }
                else
                {
                    if (TCS != null) TCS.Active = false;
                    if (ABS != null) ABS.Active = false;
                }
            }
        }
    }
}