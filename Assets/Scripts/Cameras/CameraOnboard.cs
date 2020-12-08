using UnityEngine;

namespace EasyDriving
{
    [RequireComponent(typeof(Camera))]
    public class CameraOnboard : MonoBehaviour
    {
        public VehicleController VehicleController;

        [Range(0f, 1f)]
        public float PositionSmoothing = 0.3f;

        [Range(0f, 1f)]
        public float PositionIntensity = 0.125f;

        [Range(0f, 1f)]
        public float MaxPositionOffsetMagnitude = 0.2f;

        private Vector3 _positionOffset;
        private Vector3 _accelerationChangeVelocity;
        private Vector3 _offsetChangeVelocity;
        private Vector3 _prevAcceleration;
        private Vector3 _localAcceleration;
        private Vector3 _newPositionOffset;

        private Vector3 _initialPosition;

        private void Start()
        {
            _initialPosition = VehicleController.transform.InverseTransformPoint(transform.position);
        }

        void FixedUpdate()
        {
            transform.position = VehicleController.transform.TransformPoint(_initialPosition);

            _localAcceleration = Vector3.zero;
            if (VehicleController != null)
            {
                _localAcceleration = VehicleController.transform.TransformDirection(VehicleController.Acceleration);
            }

            _newPositionOffset = (Vector3.SmoothDamp(_prevAcceleration, _localAcceleration, ref _accelerationChangeVelocity, PositionSmoothing) / 100f) * PositionIntensity;
            _positionOffset = Vector3.SmoothDamp(_positionOffset, _newPositionOffset, ref _offsetChangeVelocity, PositionSmoothing);
            _positionOffset.y *= 0.3f;
            _positionOffset.x *= 0.7f;
            _positionOffset = Vector3.ClampMagnitude(_positionOffset, MaxPositionOffsetMagnitude);
            transform.position -= VehicleController.transform.TransformDirection(_positionOffset);

            if (VehicleController != null)
            {
                _prevAcceleration = VehicleController.Acceleration;
            }
        }
    }
}
