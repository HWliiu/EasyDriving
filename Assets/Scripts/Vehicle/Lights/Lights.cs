using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyDriving
{
    [Serializable]
    public class Lights
    {
        [Serializable]
        public class VehicleLight
        {
            public List<Light> lightSources = new List<Light>();
            public bool On { get; private set; }
            public void TurnOn()
            {
                On = true;
                foreach (var light in lightSources)
                    light.enabled = true;
            }
            public void TurnOff()
            {
                On = false;
                foreach (var light in lightSources)
                    light.enabled = false;
            }
        }

        public VehicleLight BreakeLights = new VehicleLight();
        public VehicleLight RearLights = new VehicleLight();
        public VehicleLight ReverseLights = new VehicleLight();
        public VehicleLight HeadLights = new VehicleLight();
        public VehicleLight FullBeams = new VehicleLight();
        public VehicleLight LeftBlinkers = new VehicleLight();
        public VehicleLight RightBlinkers = new VehicleLight();

        private VehicleController _vc;

        public bool BlinkerState => (int)(Time.realtimeSinceStartup * 2) % 2 == 0;

        public void Initialize(VehicleController vc) => this._vc = vc;

        public void Update()
        {
            if (_vc)
            {
                //刹车灯
                if (BreakeLights != null)
                {
                    if (_vc.Brakes.Braking)
                    {
                        BreakeLights.TurnOn();
                    }
                    else
                    {
                        BreakeLights.TurnOff();
                    }
                }

                //倒车灯
                if (ReverseLights != null)
                {
                    if (_vc.Transmission.Gear < 0)
                    {
                        ReverseLights.TurnOn();
                    }
                    else
                    {
                        ReverseLights.TurnOff();
                    }
                }

                //近光灯
                if (RearLights != null && HeadLights != null)
                {
                    if (_vc.Input.LowBeamLights)
                    {
                        RearLights.TurnOn();
                        HeadLights.TurnOn();
                    }
                    else
                    {
                        RearLights.TurnOff();
                        HeadLights.TurnOff();

                        if (FullBeams != null && FullBeams.On)
                        {
                            FullBeams.TurnOff();
                            _vc.Input.FullBeamLights = false;
                        }
                    }
                }

                //远光灯
                if (FullBeams != null)
                {
                    if (_vc.Input.FullBeamLights)
                    {
                        FullBeams.TurnOn();

                        if (HeadLights != null && RearLights != null && !_vc.Input.LowBeamLights)
                        {
                            HeadLights.TurnOn();
                            RearLights.TurnOn();
                            _vc.Input.LowBeamLights = true;
                        }
                    }
                    else
                    {
                        FullBeams.TurnOff();
                    }
                }

                //左转向灯
                if (LeftBlinkers != null)
                {
                    if (_vc.Input.LeftBlinker)
                    {
                        if (BlinkerState)
                        {
                            LeftBlinkers.TurnOn();
                        }
                        else
                        {
                            LeftBlinkers.TurnOff();
                        }
                    }
                    else
                    {
                        LeftBlinkers.TurnOff();
                    }
                }
                //右转向灯
                if (RightBlinkers != null)
                {
                    if (_vc.Input.RightBlinker)
                    {
                        if (BlinkerState)
                        {
                            RightBlinkers.TurnOn();
                        }
                        else
                        {
                            RightBlinkers.TurnOff();
                        }
                    }
                    else
                    {
                        RightBlinkers.TurnOff();
                    }
                }

                //应急灯
                //if (LeftBlinkers != null && RightBlinkers != null)
                //{
                //    if (_vc.Input.HazardLights)
                //    {
                //        if (BlinkerState)
                //        {
                //            LeftBlinkers.TurnOn();
                //            RightBlinkers.TurnOn();
                //        }
                //        else
                //        {
                //            LeftBlinkers.TurnOff();
                //            RightBlinkers.TurnOff();
                //        }
                //    }
                //    else
                //    {
                //        LeftBlinkers.TurnOff();
                //        RightBlinkers.TurnOff();
                //    }
                //}
            }
        }
    }
}