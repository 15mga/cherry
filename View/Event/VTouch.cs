using UnityEngine;
using UnityEngine.EventSystems;

namespace Cherry.View.Event
{
    public class VTouch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        public UnityPointEvent OnDown { get; } = new();
        public UnityPointEvent OnUp { get; } = new();
        public UnityPointEvent OnTouch { get; } = new();

        public void OnPointerClick(PointerEventData eventData)
        {
            OnTouch.Invoke(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDown.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUp.Invoke(eventData);
        }

        public void Clear()
        {
            OnDown.RemoveAllListeners();
            OnUp.RemoveAllListeners();
        }
    }
}