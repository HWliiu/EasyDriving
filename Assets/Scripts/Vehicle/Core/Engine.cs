using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EasyDriving
{
    [Serializable]
    public class Engine
    {
        [Range(80, 500)]
        public float MaxPower = 250;
        [Range(400, 1200)]
        public float MinRpm = 800;
        [Range(2500, 10000)]
        public float MaxRpm = 5500;
        [Range(0f, 0.1f)]
        public float IdlingThrottle = 0.0125f;
        [Range(0, 10)]
        public float IdlingMaxSpeed = 5f;

        public AnimationCurve PowerCurve = new AnimationCurve(new Keyframe[3] {
            new Keyframe(0f, 0f,1f,1f),
            new Keyframe(0.75f, 1f),
            new Keyframe(1f, 0.92f)
        });


        private VehicleController _vc;
        private float _rpm = 0f;
        private float _smoothRpm = 0f;
        private readonly float _throttleDelay = 0.2f;    //踏板与引擎油门的延迟时间
        private readonly float _startDuration = 1f;
        private readonly float _stopDuration = 1f;
        private bool _wasRunning = false;
        private float _prevRpm = 0f;
        private float _startedTime = -1f;
        private float _stoppedTime = -1f;
        private float _maxRpmChange = 10000f;    //引擎每秒最大可改变的转速
        private float _powerReduction = 0f;
        //SmoothDamp的传入参数
        private float _throttleVelocity = 0f;

        public float Rpm
        {
            get
            {
                if (IsRunning && !Starting && !Stopping)
                    return Mathf.Clamp(_smoothRpm, MinRpm, MaxRpm);
                if (Starting)
                    return StartingPercent * MinRpm;
                if (Stopping)
                    return (1f - StoppingPercent) * MinRpm;
                return 0;
            }
        }
        public float RpmPercent => Mathf.Clamp01((Rpm - MinRpm) / (MaxRpm - MinRpm));
        public float Power { get; private set; } = 0f;
        public float Throttle { get; private set; } = 0f;
        public float Torque
        {
            get
            {
                if (Rpm > 0)
                    return (9548f * Power) / Rpm;   //功率(kW)=扭矩(N-m)×转速(rpm)/9549
                return 0;
            }
        }
        public float ApproxMaxTorque
        {
            get
            {
                if (Rpm > 0)
                    return (9548f * MaxPower) / (MaxRpm * 0.6f);
                return 0;
            }
        }
        public float PowerReduction
        {
            get => _powerReduction;
            set => _powerReduction = Mathf.Clamp01(value);
        }

        public bool IsRunning { get; private set; } = false;
        public bool Starting { get; private set; } = false;
        public bool Stopping { get; private set; } = false;
        public bool IsIdling { get; private set; } = true;
        public float StartingPercent => _startedTime >= 0 ? Mathf.Clamp01((Time.realtimeSinceStartup - _startedTime) / _startDuration) : 1;
        public float StoppingPercent => _stoppedTime >= 0 ? Mathf.Clamp01((Time.realtimeSinceStartup - _stoppedTime) / _stopDuration) : 1;
        public float Load => Mathf.Clamp01(RpmPercent * 0.6f + Power / MaxPower * 0.4f);   //引擎负载

        public void Initialize(VehicleController vc) => this._vc = vc;

        public void Start()
        {
            _wasRunning = IsRunning;
            IsRunning = true;

            if (!_wasRunning) _startedTime = Time.realtimeSinceStartup;
        }
        public void Stop()
        {
            _wasRunning = IsRunning;
            IsRunning = false;
            Stopping = true;
            _stoppedTime = Time.realtimeSinceStartup;
        }
        public void Toggle()
        {
            if (IsRunning)
                Stop();
            else
                Start();
        }

        public void FixedUpdate()
        {
            if (_vc.Input == null) return;

            StartEngine();

            CalculateRPM();

            CalculatePower();
        }

        private void CalculatePower()
        {
            //计算马力
            Power = 0;
            if (!Starting && !Stopping && IsRunning && !_vc.Transmission.Shifting)
            {
                Throttle = Mathf.SmoothDamp(Throttle, _vc.Input.Throttle, ref _throttleVelocity, _throttleDelay);
                if (Throttle < IdlingThrottle)
                {
                    Throttle = IdlingThrottle;
                    IsIdling = true;
                }
                else
                {
                    IsIdling = false;
                }

                Power = Mathf.Abs(PowerCurve.Evaluate(Rpm / MaxRpm) * MaxPower * Throttle) * (1f - PowerReduction);
            }
            else
            {
                Throttle = 0;
                Power = 0;
                _throttleVelocity = 0;
            }
        }

        private void CalculateRPM()
        {
            if (IsRunning)
            {
                var allowedRpmChange = _maxRpmChange * Time.fixedDeltaTime * PowerCurve.Evaluate(_rpm / MaxRpm);

                _prevRpm = _rpm;

                //计算引擎转速
                if (_vc.Transmission.Gear != 0 && !_vc.Transmission.Shifting)
                {
                    //引擎与传动装置连接
                    _rpm = _vc.Transmission.ReverseRpm;
                    if (!_vc.Input.Throttle.IsDeadzoneZero())
                        _rpm += _vc.Transmission.AddedClutchRPM;

                    if (_rpm > (_prevRpm + allowedRpmChange))
                        _rpm = _prevRpm + allowedRpmChange;
                    else if (_rpm < (_prevRpm - allowedRpmChange))
                        _rpm = _prevRpm - allowedRpmChange;
                }
                else
                {
                    //引擎与传动装置断开
                    var userInput = _vc.Input.Throttle;
                    if (_vc.Transmission.Shifting || userInput.IsDeadzoneZero())
                        userInput = -1f;

                    _rpm += allowedRpmChange * userInput;
                }

                if (IsIdling)
                    _rpm = _prevRpm - allowedRpmChange;
                //限制引擎转速
                if (_rpm > MaxRpm)
                    _rpm = MaxRpm + Random.Range(-100, 100);
                else if (_rpm < MinRpm)
                    _rpm = MinRpm + Random.Range(-100, 100);
            }
            else
            {
                _rpm = 0;
                _prevRpm = 0;
            }

            _smoothRpm = Mathf.MoveTowards(_smoothRpm, _rpm, Time.fixedTime);
        }

        private void StartEngine()
        {
            if (IsRunning && !_wasRunning)
            {
                //开始启动
                _startedTime = Time.realtimeSinceStartup;
                _wasRunning = true;
                Starting = true;
                Stopping = false;
            }

            if (Starting && Time.realtimeSinceStartup > _startedTime + _startDuration)
            {
                //启动完成
                Starting = false;
            }

            if (Stopping && Time.realtimeSinceStartup > _stoppedTime + _stopDuration)
            {
                //关闭完成
                Stopping = false;
            }
        }
    }
}
