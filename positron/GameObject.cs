using System;
using System.Collections.Generic;

using OpenTK;

namespace Positron
{
    public abstract class GameObjectBase
    {
        protected bool _Preserve = false;
        protected RenderSet _RenderSet;
        /// <summary>
        /// Flag for whether or not this object should persist across Scene changes. If enabled, the object will be moved to the entering scene.
        /// </summary>
        public bool Preserve { get { return _Preserve; } set { _Preserve = value; } }
        public GameObjectBase(RenderSet render_set)
        {
            _RenderSet = render_set;
        }
        public ThreadedRendering mWindow { get { return mGame.Window; } }
        public PositronGame mGame { get { return mScene.Game; } }
        public Scene mScene { get { return mRenderSet.Scene; } }
        public FarseerPhysics.Dynamics.World mWorld { get { return mScene.World; } }
        public RenderSet mRenderSet { get { return _RenderSet; } }
        public virtual FarseerPhysics.Dynamics.Body mBody { get { return null; } }
        public virtual Drawable mDrawable { get { return null; } }
        public virtual SpriteBase mSprite { get { return null; } }
        
    }
    public class GameObject : GameObjectBase
    {
        protected readonly Xform _Transform;
        public Xform mTransform { get { return _Transform; } }
        protected List<Extension> Extensions;
        public void AddExtension(params Extension[] extensions)
        {
            Extensions.AddRange(extensions);
        }
        public void RemoveExtension(params Extension[] extensions)
        {
            foreach (Extension extension in extensions)
                Extensions.Remove(extension);
        }
        public GameObject() : base()
        {
            _Transform = new Xform(this);
            Extensions = new List<Extension>();
        }
        public void Update()
        {
            mTransform.mState.Modified = false;
            if (mBody != null)
            {
                FarseerPhysics.Common.Transform transform;
                mBody.GetTransform(out transform);
                // TODO: update mTransform from Farseer transform
            }
        }
        protected void LateUpdate()
        {
            if (mBody != null)
            {
                Matrix4 global_matrix = mTransform.GlobalMatrix;
                float x = global_matrix[0, 0]; // X+ vector
                float y = global_matrix[1, 0]; // Y+ vector
                float theta = (float)Math.Atan2((double)y, (double)x); // Rotation about Z-axis
                mBody.SetTransform(
                    new Microsoft.Xna.Framework.Vector2(mTransform.PositionX, mTransform.PositionY),
                    theta);
            }
        }
    }
}