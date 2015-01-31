using System;
using System.Collections.Generic;

using OpenTK;

namespace Positron
{
    public abstract class GameObject
    {
        public virtual ThreadedRendering mWindow { get { return mGame.Window; } }
        public virtual PositronGame mGame { get { return mScene.Game; } }
        public abstract Scene mScene { get; }
        public abstract RenderSet mRenderSet { get; }
        public abstract FarseerPhysics.Dynamics.Body mBody { get; }
        public abstract Drawable mDrawable { get; }
        public abstract SpriteBase mSprite { get; }
        protected readonly Xform _Transform;
        public Xform mTransform {
            get { return _Transform; }
        }
    }
}