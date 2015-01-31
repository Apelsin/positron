using System;

namespace Positron
{
    public interface IRenderable : IDisposable
    {
        RenderSet Set { get; }
        void Render(float time);
    }
}