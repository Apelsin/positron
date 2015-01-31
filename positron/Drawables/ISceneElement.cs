using System;
using System.Collections.Generic;

namespace Positron
{
    public delegate void RenderSetChangeEventHandler(object sender, RenderSetChangeEventArgs e);
    public interface ISceneElement : IDisposable
    {
        event RenderSetChangeEventHandler RenderSetEntry;
        event RenderSetChangeEventHandler RenderSetTransfer;
        void OnRenderSetEntry (object sender, RenderSetChangeEventArgs e);
        void OnRenderSetTransfer (object sender, RenderSetChangeEventArgs e);
        bool Preserve { get; set; }
        RenderSet Set { get; }
        List<IRenderable> Blueprints { get; }
    }
}