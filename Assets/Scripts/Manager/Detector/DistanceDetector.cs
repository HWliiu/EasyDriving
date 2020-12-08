using UnityEngine;

namespace EasyDriving
{
    public class DistanceDetector : MonoBehaviour
    {
        public Transform DetectPoint1;
        public Transform DetectPoint2;

        private readonly float _maxDistance = 20f;

        public float Point1Distance { get; private set; }
        public float Point2Distance { get; private set; }

        public void DetectDistance()
        {
            RaycastHit hitInfo;
            if (DetectPoint1)
            {
                if (Physics.Raycast(DetectPoint1.position, DetectPoint1.forward, out hitInfo, _maxDistance, 1 << 8))
                {
                    Debug.DrawLine(DetectPoint1.position, hitInfo.point, Color.red);
                    Point1Distance = hitInfo.distance;
                }
                else
                {
                    Point1Distance = _maxDistance;
                }
            }

            if (DetectPoint2)
            {
                if (Physics.Raycast(DetectPoint2.position, DetectPoint2.forward, out hitInfo, _maxDistance, 1 << 8))
                {
                    Debug.DrawLine(DetectPoint2.position, hitInfo.point, Color.red);
                    Point2Distance = hitInfo.distance;
                }
                else
                {
                    Point2Distance = _maxDistance;
                }
            }
        }
    }
}
