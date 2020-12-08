using System;
using System.Collections;
using UnityEngine;

namespace EasyDriving
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [HideInInspector] public InputStates Input = new InputStates();
        [SerializeField] public Steering Steering = new Steering();
        [SerializeField] public Sound Sound = new Sound();
        [SerializeField] public Engine Engine = new Engine();
        [SerializeField] public Transmission Transmission = new Transmission();
        [SerializeField] public Chassis Chassis = new Chassis();
        [SerializeField] public Brakes Brakes = new Brakes();
        [SerializeField] public Effects Effects = new Effects();
        [SerializeField] public Lights Lights = new Lights();

        public event EventHandler OnCollideObstacle;

        private bool _active;
        private Vector3 _velocity;
        private Vector3 _prevVelocity;

        public bool Active
        {
            get => _active;
            set
            {
                if (_active == false && value)
                    Activate();
                else if (_active && value == false)
                    Suspend();

                _active = value;
            }
        }
        public Rigidbody Rigidbody { get; private set; }
        public float ForwardVelocity { get; private set; }
        public float Speed => Mathf.Abs(ForwardVelocity);
        public float SpeedKPH => Speed * 3.6f;
        public Vector3 Acceleration { get; private set; }
        public float ForwardAcceleration { get; private set; }

        public float TopSpeedForGear
        {
            get
            {
                if (Transmission.Gear == 0) return short.MaxValue;
                var gearRatio = Mathf.Abs(Transmission.GearRatio);
                var maxWheelRpm = Engine.MaxRpm / gearRatio / 60;  // 单位r/s
                var wheelPerimeter = Mathf.PI * (Chassis.FrontAxle.LeftWheelCollider.radius + Chassis.FrontAxle.RightWheelCollider.radius);
                return maxWheelRpm * wheelPerimeter * 3.6f;
            }
        }
        IEnumerator Start()
        {
            Initialize();
            yield return new WaitForSeconds(2f);
            Active = true;
        }

        void Update()
        {
            Chassis.Update();
            Effects.Update();
            Lights.Update();
            Sound.Update();
            //Debug.Log(SpeedKPH);
        }

        private void FixedUpdate()
        {
            //计算加速度
            _prevVelocity = _velocity;
            _velocity = transform.InverseTransformDirection(Rigidbody.velocity);
            Acceleration = (_velocity - _prevVelocity) / Time.fixedDeltaTime;

            ForwardVelocity = _velocity.z;
            ForwardAcceleration = Acceleration.z;
            //更新组件
            if (Engine.Starting || Engine.Stopping)
                Engine.FixedUpdate();

            Chassis.FixedUpdate();

            if (Active)
            {
                Transmission.FixedUpdate();
                Engine.FixedUpdate();
                Transmission.TorqueSplit(Engine.Torque, Engine.Rpm);
            }
            Brakes.FixedUpdate();
            Steering.FixedUpdate();
            //限制速度
            var idlingMaxSpeed = Engine.IdlingMaxSpeed * (1 - Transmission.Clutch);
            var topSpeedForGear = TopSpeedForGear * Engine.Throttle;
            LimitMaxSpeed(Engine.IsIdling ? idlingMaxSpeed : topSpeedForGear);
        }

        private void Initialize()
        {
            Rigidbody = GetComponent<Rigidbody>();
            Rigidbody.maxAngularVelocity = 10f;

            Engine.Initialize(this);
            Transmission.Initialize(this);
            Chassis.Initialize(this);
            Brakes.Initialize(this);
            Steering.Initialize(this);
            Effects.Initialize(this);
            Lights.Initialize(this);
            Sound.Initialize(this);

        }

        private void Activate() => Engine.Start();
        private void Suspend() => Engine.Stop();

        private void LimitMaxSpeed(float maxSpeed)
        {
            //限制最大速度
            var regulatorThreshold = 0.8f;
            if (SpeedKPH > maxSpeed * regulatorThreshold)
            {
                var powerReduction = Mathf.Clamp01((SpeedKPH - (maxSpeed * regulatorThreshold)) / (maxSpeed * (1f - regulatorThreshold)));
                Engine.PowerReduction = powerReduction * powerReduction;
            }
            else
            {
                Engine.PowerReduction = 0;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Sound.CrashComponent.Play(collision);
            if (collision.collider.CompareTag(TagConst.Obstacle))
                OnCollideObstacle?.Invoke(this, null);
        }
    }
}
