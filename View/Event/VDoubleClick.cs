using UnityEngine;
using UnityEngine.EventSystems;

namespace Cherry.View.Event
{
    public class VDoubleClick : MonoBehaviour, IPointerClickHandler
    {
        public UnityPointEvent OnDoubleClick { get; } = new();

        private float _lastClick;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            var now = Time.time;
            if (now - _lastClick < 0.2f)
            {
                OnDoubleClick.Invoke(eventData);
            }
            _lastClick = now;
        }
    }
}