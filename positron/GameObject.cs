using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Positron
{
    public class GameObject
    {
        public abstract ThreadedRendering Window { get; }
        public abstract PositronGame Game { get; }
        public abstract Scene Scene { get; }
        public abstract RenderSet RenderSet { get; }
        public abstract FarseerPhysics.Dynamics.Body Body { get; }
        public abstract Drawable Drawable { get; }
        public abstract SpriteBase Sprite { get; }
    }
}
