using System;

namespace Positron
{
    public class WR<TargetType> : WeakReference
    {
        public TargetType T { get { return (TargetType)Target; } }
        public WR (TargetType target)
            :base(target)
        {
        }
    }
}

