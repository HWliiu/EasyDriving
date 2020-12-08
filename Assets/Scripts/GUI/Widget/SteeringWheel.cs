using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyDriving
{
    public class SteeringWheel : MonoBehaviour
    {
        public Graphic SteeringWheelGraphic;

        private RectTransform _rectT;
        private Vector2 _centerPoint;

        public float MaximumSteeringAngle = 720f;
        public float ReturnToCenterSpeed = 1440f;

        private float _wheelAngle = 0f;
        private float _wheelPrevAngle = 0f;

        private bool _wheelBeingHeld = false;

        public float Value => _wheelAngle / MaximumSteeringAngle;

        void Start()
        {
            _rectT = SteeringWheelGraphic.rectTransform;

            InitEventsSystem();
            UpdateRect();
        }

        private void Update()
        {
            //释放时自动回正方向盘
            if (!_wheelBeingHeld && !Mathf.Approximately(0f, _wheelAngle))
            {
                var deltaAngle = ReturnToCenterSpeed * Time.deltaTime;
                if (Mathf.Abs(deltaAngle) > Mathf.Abs(_wheelAngle))
                    _wheelAngle = 0f;
                else if (_wheelAngle > 0f)
                    _wheelAngle -= deltaAngle;
                else
                    _wheelAngle += deltaAngle;
            }
            //更新gui
            _rectT.localEulerAngles = Vector3.back * _wheelAngle;
        }

        private void InitEventsSystem()
        {
            var events = SteeringWheelGraphic.gameObject.GetComponent<EventTrigger>();

            if (events == null)
                events = SteeringWheelGraphic.gameObject.AddComponent<EventTrigger>();

            if (events.triggers == null)
                events.triggers = new List<EventTrigger.Entry>();

            var entry = new EventTrigger.Entry();
            var callback = new EventTrigger.TriggerEvent();
            var functionCall = new UnityAction<BaseEventData>(PressEvent);
            callback.AddListener(functionCall);
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback = callback;

            events.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            callback = new EventTrigger.TriggerEvent();
            functionCall = DragEvent;
            callback.AddListener(functionCall);
            entry.eventID = EventTriggerType.Drag;
            entry.callback = callback;

            events.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            callback = new EventTrigger.TriggerEvent();
            functionCall = ReleaseEvent;
            callback.AddListener(functionCall);
            entry.eventID = EventTriggerType.PointerUp;
            entry.callback = callback;

            events.triggers.Add(entry);
        }

        private void UpdateRect()
        {
            var corners = new Vector3[4];
            _rectT.GetWorldCorners(corners);

            for (var i = 0; i < 4; i++)
            {
                corners[i] = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
            }

            var bottomLeft = corners[0];
            var topRight = corners[2];
            var width = topRight.x - bottomLeft.x;
            var height = topRight.y - bottomLeft.y;

            var rect = new Rect(bottomLeft.x, topRight.y, width, height);
            _centerPoint = new Vector2(rect.x + rect.width * 0.5f, rect.y - rect.height * 0.5f);
        }

        private void PressEvent(BaseEventData eventData)
        {
            var pointerPos = ((PointerEventData)eventData).position;

            _wheelBeingHeld = true;
            _wheelPrevAngle = Vector2.Angle(Vector2.up, pointerPos - _centerPoint);
        }

        private void DragEvent(BaseEventData eventData)
        {
            var pointerPos = ((PointerEventData)eventData).position;

            var wheelNewAngle = Vector2.Angle(Vector2.up, pointerPos - _centerPoint);

            if (Vector2.Distance(pointerPos, _centerPoint) > 20f)
            {
                if (pointerPos.x > _centerPoint.x)
                    _wheelAngle += wheelNewAngle - _wheelPrevAngle;
                else
                    _wheelAngle -= wheelNewAngle - _wheelPrevAngle;
            }

            _wheelAngle = Mathf.Clamp(_wheelAngle, -MaximumSteeringAngle, MaximumSteeringAngle);
            _wheelPrevAngle = wheelNewAngle;
        }

        private void ReleaseEvent(BaseEventData eventData)
        {
            DragEvent(eventData);

            _wheelBeingHeld = false;
        }
    }
}