using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cherry.Extend
{
    public static class EComponent
    {
        public static T GetComp<T>(this Component comp) where T : Component
        {
            var c = comp.GetComponent<T>();
            if (c == null) c = comp.gameObject.AddComponent<T>();

            return c;
        }

        public static Component GetComp(this Component comp, Type type)
        {
            var c = comp.GetComponent(type);
            if (c == null) c = comp.gameObject.AddComponent(type);

            return c;
        }

        public static void RemoveComp<T>(this Component comp) where T : Component
        {
            var c = comp.GetComponent<T>();
            if (c == null) Object.Destroy(c);
        }

        public static void RemoveComp(this Component comp, Type type)
        {
            var c = comp.GetComponent(type);
            if (c == null) Object.Destroy(c);
        }

        public static Tuple<float, float, float, float, float, float> GetLocalBounds(this Component comp,
            float scale = 1)
        {
            return comp.GetBounds(comp.transform.position, scale);
        }

        public static Tuple<float, float, float, float, float, float> GetBounds(this Component comp,
            float scale = 1)
        {
            return comp.GetBounds(Vector3.zero, scale);
        }

        public static Tuple<float, float, float, float, float, float> GetBounds(this Component comp,
            Vector3 referencePos, float scale = 1)
        {
            var mfs = comp.GetComponentsInChildren<MeshFilter>();

            var min_x = float.MaxValue;
            var max_x = float.MinValue;
            var min_y = float.MaxValue;
            var max_y = float.MinValue;
            var min_z = float.MaxValue;
            var max_z = float.MinValue;

            for (var index = 0; index < mfs.Length; index++)
            {
                var item = mfs[index];
                var pos = item.transform.position - referencePos + item.mesh.bounds.center * scale;
                var size = item.mesh.bounds.size * scale;
                var hx = size.x / 2;
                var hy = size.y / 2;
                var hz = size.z / 2;
                min_x = Mathf.Min(min_x, pos.x - hx);
                max_x = Mathf.Max(max_x, pos.x + hx);
                min_y = Mathf.Min(min_y, pos.y - hy);
                max_y = Mathf.Max(max_y, pos.y + hy);
                min_z = Mathf.Min(min_z, pos.z - hz);
                max_z = Mathf.Max(max_z, pos.z + hz);
            }

            return new Tuple<float, float, float, float, float, float>(
                min_x,
                max_x,
                min_y,
                max_y,
                min_z,
                max_z
            );
        }

        public static Bounds GetLocalBounds2(this Component comp, float scale = 1)
        {
            return comp.GetBounds2(comp.transform.position, scale);
        }

        public static Bounds GetBounds2(this Component comp, float scale = 1)
        {
            return comp.GetBounds2(Vector3.zero, scale);
        }

        public static Bounds GetBounds2(this Component comp, Vector3 referencePos, float scale = 1)
        {
            var (min_x, max_x, min_y, max_y, min_z, max_z) = comp.GetBounds(referencePos, scale);

            return new Bounds(new Vector3((max_x + min_x) / 2, (max_y + min_y) / 2, (max_z + min_z) / 2),
                new Vector3((max_x - min_x) / 2, (max_y - min_y) / 2, (max_z - min_z) / 2));
        }
    }
}