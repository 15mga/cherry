using UnityEngine;

namespace Cherry.Extend
{
    public static class EObject
    {
        public static bool RefEquals(this Object obj, Object other)
        {
            return ReferenceEquals(obj, other);
        }
    }
}