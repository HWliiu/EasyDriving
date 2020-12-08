using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EasyDriving
{
    public class PedalScrollbar : Scrollbar, IBeginDragHandler, IEndDragHandler
    {
        public bool IsTouched { get; private set; }
        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (!IsTouched && value > 0f) value -= Time.deltaTime * 5f;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            IsTouched = true;
        }

        public void OnEndDrag(PointerEventData eventData) => IsTouched = false;
    }
}
