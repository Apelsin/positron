using System;

namespace Positron
{
    public delegate void RenderSetChangeEventHandler(object sender, RenderSetChangeEventArgs e);
    public interface IRenderable
    {
        void Render();
    }
    public interface IRenderSetElementBase
    {
        RenderSet mRenderSet { get; }
    }
    public interface IRenderSetElement
    {
        bool Preserve { get; set; }
        event RenderSetChangeEventHandler RenderSetChange;
    }
}