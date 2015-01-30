using System;

namespace Positron
{
    public interface IGLContextLateUpdate
    {
        void AddUpdateEventHandler(object sender, UpdateEventHandler handler);
    }
}

