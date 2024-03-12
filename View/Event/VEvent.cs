using Cherry.Extend;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cherry.View.Event
{
    public class VEvent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
        IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        /// <summary>
        ///     事件是否穿透
        /// </summary>
        public bool ProceedEvent { get; set; }

        public UnityPointEvent OnDragBegin { get; } = new();
        public UnityPointEvent OnDragging { get; } = new();
        public UnityPointEvent OnDragEnd { get; } = new();
        public UnityPointEvent OnEnter { get; } = new();
        public UnityPointEvent OnExit { get; } = new();
        public UnityPointEvent OnTouchBegin { get; } = new();
        public UnityPointEvent OnTouchEnd { get; } = new();
        public UnityPointEvent OnTouch { get; } = new();

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnDragBegin?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedBeginDrag();
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragging?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedDrag();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnDragEnd?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedEndDrag();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnTouch?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedClick();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnTouchBegin?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedDown();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedExit();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnTouchEnd?.Invoke(eventData);

            if (ProceedEvent) eventData.ProceedUp();
        }

        private void OnDestroy()
        {
            OnDragBegin.RemoveAllListeners();
            OnDragging.RemoveAllListeners();
            OnDragEnd.RemoveAllListeners();
            OnEnter.RemoveAllListeners();
            OnExit.RemoveAllListeners();
            OnTouchBegin.RemoveAllListeners();
            OnTouchEnd.RemoveAllListeners();
            OnTouch.RemoveAllListeners();
        }
    }
}