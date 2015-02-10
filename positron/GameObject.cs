using System;
using System.Collections.Generic;

using OpenTK;

namespace Positron
{
    public abstract class GameObjectBase
    {
        protected readonly Xform _Transform;
        public Xform mTransform { get { return _Transform; } }
        protected bool _Preserve = false;
        public bool Preserve { get { return _Preserve; } set { _Preserve = value; } }
        protected List<Extension> Extensions;
        public void AddExtension(params Extension[] extensions)
        {
            Extensions.AddRange(extensions);
        }
        public GameObjectBase()
        {
            _Transform = new Xform(this);
        }
        public void Update()
        {
            mTransform.mState.Modified = false;
            if(mBody != null)
            {
                FarseerPhysics.Common.Transform transform;
                mBody.GetTransform(out transform);
                // TODO: update mTransform from Farseer transform
            }
        }
        protected void LateUpdate()
        {
            if(mBody != null)
            {
                Matrix4 global_matrix = mTransform.GlobalMatrix;
                float x = global_matrix[0,0]; // X+ vector
                float y = global_matrix[1,0]; // Y+ vector
                float theta = (float)Math.Atan2((double)y, (double)x); // Rotation about Z-axis
                mBody.SetTransform(
                    new Microsoft.Xna.Framework.Vector2(mTransform.PositionX, mTransform.PositionY),
                    theta);
            }
        }
        public virtual ThreadedRendering mWindow { get { return mGame.Window; } }
        public virtual PositronGame mGame { get { return mScene.Game; } }
        public virtual Scene mScene { get { return null; } }
        public virtual FarseerPhysics.Dynamics.World mWorld { get { return mScene.World; } }
        public virtual RenderSet mRenderSet { get { return null; } }
        public virtual FarseerPhysics.Dynamics.Body mBody { get { return null; } }
        public virtual Drawable mDrawable { get { return null; } }
        public virtual SpriteBase mSprite { get { return null; } }
        
    }
    public class GameObject : GameObjectBase
    {
        protected Extension[] Extensions;
        public GameObject() : base()
        {
            Extensions = new Extension[]{};
        }
    }
}