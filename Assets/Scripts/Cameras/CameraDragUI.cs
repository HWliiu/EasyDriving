using UnityEngine;
using UnityEngine.EventSystems;

namespace EasyDriving
{
    public class CameraDragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public CameraDrag CameraDrag;
        private Vector2 _lastPosition;
        private bool _flag;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _lastPosition = Vector3toVector2(Input.mousePosition);
            _flag = true;
        }

        private void Update()
        {
            if (CameraDrag && _flag)
            {
                CameraDrag.DragDelta = Vector3toVector2(Input.mousePosition) - _lastPosition;
                _lastPosition = Vector3toVector2(Input.mousePosition);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (CameraDrag)
            {
                CameraDrag.DragDelta = Vector2.zero;
                _flag = false;
            }
        }

        private Vector2 Vector3toVector2(Vector3 vector3) => new Vector2(vector3.x, vector3.y);
        public void OnDrag(PointerEventData eventData)
        {

        }
    }
}
