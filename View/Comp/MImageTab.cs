using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cherry.View.Comp
{
    [AddComponentMenu("UI/MTab")]
    public class MImageTab : MonoBehaviour
    {
        private List<ITabItem> _tabs;
        public Func<string, string> ConvertIn;
        public Func<string, string> ConvertOut;

        public string Current { get; private set; }

        public ITabItem[] Tabs => _tabs?.ToArray();

        public event Action<string> onSelect;

        public void UpdateList()
        {
            if (_tabs != null)
                foreach (var tab in _tabs)
                    tab.onSelect -= OnSelectTab;

            _tabs = new List<ITabItem>(transform.GetComponentsInChildren<ITabItem>());
            foreach (var tab in _tabs)
            {
                tab.onSelect += OnSelectTab;
                tab.MTab = this;
            }
        }

        public void Clear()
        {
            if (_tabs == null) return;
            foreach (var tab in _tabs) tab.onSelect -= OnSelectTab;

            onSelect = null;
            _tabs = null;
        }

        private void OnSelectTab(ITabItem tab)
        {
            onSelect?.Invoke(ConvertOut != null ? ConvertOut(tab.name) : tab.name);
        }

        public void Select(string name)
        {
            Current = ConvertIn != null ? ConvertIn(name) : name;
            Game.Log.Info($"select {Current}");
            if (_tabs != null) ShowSelect();
        }

        public bool Has(string name)
        {
            name = ConvertIn != null ? ConvertIn(name) : name;

            return _tabs.Any(t => t.name == name);
        }

        private void ShowSelect()
        {
            foreach (var t in _tabs)
                if (t.name == Current)
                    t.Select();
                else
                    t.Deselect();
        }
    }

    public interface ITabItem
    {
        public string name { get; set; }
        public GameObject gameObject { get; }
        MImageTab MTab { get; set; }
        event Action<ITabItem> onSelect;
        public void Select();
        public void Deselect();
    }
}