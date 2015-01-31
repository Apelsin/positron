using System;
using System.Collections.Generic;

namespace Positron
{
    public class RenderSet : List<IRenderable>, IRenderable
    {
        public RenderSet Set { get { return null; } }
        protected Scene _Scene;
        public Scene Scene { get { return _Scene; } }
        public RenderSet (Scene scene, params IRenderable[] renderables):
            base()
        {
            _Scene = scene;
            foreach(IRenderable renderable in renderables)
                Add (renderable);
        }
        public void Render (float time)
        {
            for(int i = 0; i < Count; i++)
                this[i].Render(time);
        }
        public virtual void Dispose()
        {
            Clear();
            _Scene = null;
        }
    }
}

