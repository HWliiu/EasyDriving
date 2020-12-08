using UnityEngine;

namespace EasyDriving
{
    public class AnalogGauge : MonoBehaviour
    {
        public float MaxValue;

        public float StartAngle = 574;

        public float EndAngle = 330;

        [Range(0, 1)]
        public float NeedleSmoothing;

        public bool LockAtStart = false;

        public bool LockAtEnd = false;

        private GameObject _needle;
        private float _currentValue;
        private float _angle;
        private float _prevAngle;
        private float _percent;

        public float Value
        {
            get => _currentValue;
            set => _currentValue = Mathf.Clamp(value, 0, MaxValue);
        }

        private void Awake()
        {
            _needle = transform.Find("Needle").gameObject;
        }

        private void Start()
        {
            _angle = StartAngle;
        }

        void Update()
        {
            _percent = Mathf.Clamp01(_currentValue / MaxValue);
            _prevAngle = _angle;
            _angle = Mathf.Lerp(StartAngle + (EndAngle - StartAngle) * _percent, _prevAngle, NeedleSmoothing);

            if (LockAtEnd) _angle = EndAngle;
            if (LockAtStart) _angle = StartAngle;

            _needle.transform.eulerAngles = new Vector3(_needle.transform.eulerAngles.x, _needle.transform.eulerAngles.y, _angle);
        }
    }
}