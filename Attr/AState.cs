using System;

namespace Cherry.Attr
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AState : Attribute
    {
        public AState(string fsm = null)
        {
            Fsm = fsm;
        }

        public string Fsm { get; }
    }
}