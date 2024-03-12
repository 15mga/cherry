using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cherry.Extend
{
    public static class EGameObject
    {
        public static void SetLayer(this GameObject obj, int layer, bool recursive = true)
        {
            obj.layer = layer;

            if (recursive)
                obj.transform.EveryChild(tnf => { SetLayer(tnf.gameObject, layer, recursive); });
        }

        public static void SetLayer(this GameObject obj, string layer, bool recursive = true)
        {
            obj.SetLayer(LayerMask.NameToLayer(layer), recursive);
        }

        public static T GetComp<T>(this GameObject obj) where T : Component
        {
            var c = obj.GetComponent<T>();
            if (c == null) c = obj.AddComponent<T>();

            return c;
        }

        public static Component GetComp(this GameObject obj, Type type)
        {
            var c = obj.GetComponent(type);
            if (c == null) c = obj.gameObject.AddComponent(type);

            return c;
        }

        public static void RemoveComp<T>(this GameObject obj) where T : Component
        {
            var c = obj.GetComponent<T>();
            if (c != null) Object.Destroy(c);
        }

        public static void RemoveComp(this GameObject obj, Type type)
        {
            var c = obj.GetComponent(type);
            if (c != null) Object.Destroy(c);
        }

        public static void ResetShader(this GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            foreach (var mat in renderer.materials)
                mat.shader = Shader.Find(mat.shader.name);
        }
    }
}