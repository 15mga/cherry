using UnityEngine;
using UnityEngine.EventSystems;

namespace Cherry.View.Event
{
    public class VEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityPointEvent OnEnter { get; } = new();

        public UnityPointEvent OnExit { get; } = new();

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter?.Invoke(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit?.Invoke(eventData);
        }

        private void OnDestroy()
        {
            OnEnter.RemoveAllListeners();
            OnExit.RemoveAllListeners();
        }
    }
}