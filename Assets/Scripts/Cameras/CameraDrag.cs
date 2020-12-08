using UnityEngine;

namespace EasyDriving
{
    [RequireComponent(typeof(Camera))]
    public class CameraDrag : MonoBehaviour
    {
        public Transform Target;

        [Range(0, 3f)]
        public float TargetUpOffset = 0.8f;
        [Range(0, 100f)]
        public float Distance = 5f;

        [Range(0, 100f)]
        public readonly float MinDistance = 3f;

        [Range(0, 100f)]
        public float MaxDistance = 8.0f;

        [Range(0, 3)]
        public float HorizontalMouseSensitivity = 0.5f;

        [Range(0, 3)]
        public float VerticalMouseSensitivity = 0.5f;

        [Range(-90, 90)]
        public float VerticalMinAngle = 0f;

        [Range(-90, 90)]
        public float VerticalMaxAngle = 80.0f;

        [Range(0, 1)]
        public float Smoothing = 0.5f;


        public bool FollowTargetsRotation = false;

        private Vector3 _target;
        private Vector3 _position;
        private float _mouseX = 0.0f;
        private float _mouseY = 0.0f;
        private Vector3 _desiredPosition = Vector3.zero;
        private Vector3 _velocity;
        private Quaternion _rotation;
        private bool _firstFrame = true;

        public Vector2 DragDelta { private get; set; }

        void Start()
        {
            Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);

            _mouseY = 10f;
            _mouseX = 0f;
        }

        private void OnEnable()
        {
            _firstFrame = true;
        }

        void LateUpdate()
        {
            if (Target == null)
                return;
            _target = Target.position + new Vector3(0f, TargetUpOffset, 0f);

            _mouseX += DragDelta.x * HorizontalMouseSensitivity;
            _mouseY -= DragDelta.y * VerticalMouseSensitivity;

            _mouseY = ClampAngle(_mouseY, VerticalMinAngle, VerticalMaxAngle);
            Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);

            var direction = new Vector3(0, 0, -Distance);
            _rotation = Quaternion.Euler(_mouseY, _mouseX, 0);

            if (FollowTargetsRotation)
            {
                _desiredPosition = _target + (Target.transform.rotation * _rotation * direction);
            }
            else
            {
                _desiredPosition = _target + (_rotation * direction);
            }


            if (!_firstFrame)
            {
                _position = Vector3.SmoothDamp(_position, _desiredPosition, ref _velocity, Smoothing);
            }
            else
            {
                _position = _desiredPosition;
                _firstFrame = false;
            }

            transform.position = _position;
            transform.LookAt(_target);
        }

        public float ClampAngle(float angle, float min, float max)
        {
            while (angle < -360 || angle > 360)
            {
                if (angle < -360)
                    angle += 360;
                if (angle > 360)
                    angle -= 360;
            }

            return Mathf.Clamp(angle, min, max);
        }
    }
}
