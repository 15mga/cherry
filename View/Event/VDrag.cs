using UnityEngine;
using UnityEngine.EventSystems;

namespace Cherry.View.Event
{
    public class VDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
    {
        public UnityPointEvent OnDragBegin { get; } = new();
        public UnityPointEvent OnDragging { get; } = new();
        public UnityPointEvent OnDragEnd { get; } = new();

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDragBegin.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragging.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnd.Invoke(eventData);
        }

        private void OnDestroy()
        {
            OnDragBegin.RemoveAllListeners();
            OnDragging.RemoveAllListeners();
            OnDragEnd.RemoveAllListeners();
        }
    }
}