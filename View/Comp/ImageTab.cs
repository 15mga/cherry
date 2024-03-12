using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cherry.View.Comp
{
    [AddComponentMenu("UI/ImageTab")]
    [RequireComponent(typeof(Image))]
    public class ImageTab : MonoBehaviour,
        IPointerDownHandler,
        IPointerEnterHandler, IPointerExitHandler, ITabItem
    {
        [SerializeField] private Sprite _default;
        [SerializeField] private Sprite _selected;
        private Image _image;
        private bool _isSelected;

        private void Awake()
        {
            _image = GetComponent<Image>();
            if (_default == null) _default = _image.sprite;

            _image.sprite = _isSelected ? _selected : _default;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            onSelect?.Invoke(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isSelected) return;
            _image.sprite = _selected;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isSelected) return;
            _image.sprite = _default;
        }

        public MImageTab MTab { get; set; }

        public event Action<ITabItem> onSelect;

        public void Select()
        {
            if (_isSelected) return;
            _isSelected = true;
            if (_image != null) _image.sprite = _selected;
        }

        public void Deselect()
        {
            if (!_isSelected) return;
            _isSelected = false;
            if (_image != null) _image.sprite = _default;
        }
    }
}