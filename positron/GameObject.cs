using System;
using System.Collections.Generic;

using OpenTK;

namespace Positron
{
    public class GameObject
    {
        protected readonly Xform _Transform;
        public Xform mTransform
        {
            get { return _Transform; }
        }
        public GameObject()
        {
            _Transform = new Xform(this);
        }
        public virtual ThreadedRendering mWindow { get { return mGame.Window; } }
        public virtual PositronGame mGame { get { return mScene.Game; } }
        public virtual Scene mScene { get { return null; } }
        public virtual RenderSet mRenderSet { get { return null; } }
        public virtual FarseerPhysics.Dynamics.Body mBody { get { return null; } }
        public virtual Drawable mDrawable { get { return null; } }
        public virtual SpriteBase mSprite { get { return null; } }
        
    }
}