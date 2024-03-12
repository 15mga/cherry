using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ASceneObjTags : Attribute
    {
        public ASceneObjTags(params string[] tags)
        {
            Tags = tags;
        }

        public string[] Tags { get; }
    }
}