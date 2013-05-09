using System;

namespace positron
{
    public interface IGLContextLateUpdate
    {
        void AddUpdateEventHandler(object sender, UpdateEventHandler handler);
    }
}

