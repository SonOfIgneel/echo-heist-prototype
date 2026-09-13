using UnityEngine;
using UnityEngine.EventSystems;

namespace EchoHeist
{
    public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform knob;
        [SerializeField, Min(1f)] private float radius = 72f;

        public static Vector2 Value { get; private set; }

        public void Configure(RectTransform knobTransform, float movementRadius)
        {
            knob = knobTransform;
            radius = Mathf.Max(1f, movementRadius);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransform area = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    area, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            Value = Vector2.ClampMagnitude(localPoint / radius, 1f);
            if (knob != null) knob.anchoredPosition = Value * radius;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }

        private void OnDisable()
        {
            Value = Vector2.zero;
            if (knob != null) knob.anchoredPosition = Vector2.zero;
        }
    }
}