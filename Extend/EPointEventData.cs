using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Cherry.Extend
{
    public static class EPointEventData
    {
        public static void ProceedDown(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.pointerDownHandler);
        }

        public static void ProceedUp(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.pointerUpHandler);
        }

        public static void ProceedClick(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.pointerClickHandler);
        }

        public static void ProceedBeginDrag(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.beginDragHandler);
        }

        public static void ProceedDrag(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.dragHandler);
        }

        public static void ProceedEndDrag(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.endDragHandler);
        }

        public static void ProceedEnter(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.pointerEnterHandler);
        }

        public static void ProceedExit(this PointerEventData data)
        {
            ProceedEvent(data, ExecuteEvents.pointerExitHandler);
        }

        public static void ProceedEvent<T>(this PointerEventData data, ExecuteEvents.EventFunction<T> func)
            where T : IEventSystemHandler
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            var current = data.pointerCurrentRaycast.gameObject;
            foreach (var t in results)
            {
                if (current == t.gameObject) continue;

                ExecuteEvents.Execute(t.gameObject, data, func);
            }
        }
    }
}