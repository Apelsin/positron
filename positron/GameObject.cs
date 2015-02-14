using System;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public abstract class GameObjectBase
    {
        public abstract Xform mTransform { get; }
        public ThreadedRendering mWindow { get { return mGame.Window; } }
        public PositronGame mGame { get { return mScene.Game; } }
        public FarseerPhysics.Dynamics.World mWorld { get { return mScene.World; } }
        public virtual Scene mScene {
            get {
                if (mTransform.Parent != null)
                    return mTransform.Parent.mScene;
                return null;
            }
        }
        public GameObjectBase() : base()
        {
        }
        public virtual FarseerPhysics.Dynamics.Body mBody { get { return null; } }
        public virtual SpriteBase mSprite { get { return null; } }
    }
    public class GameObject : GameObjectBase, IDrawable
    {
        public class State
        {
            public bool BodyEnabled = true;
            /// <summary>
            /// Flag for whether or not this object should be preserved across Scene changes.
            /// If enabled, the object will be moved to the entering scene.
            /// Only works for objects that have the root node as their parent.
            /// </summary>
            public bool Persist = false;
        }
        public readonly State mState;

        public void SaveState()
        {
            mState.BodyEnabled = mBody.Enabled;
        }

        public void LoadState()
        {
            mBody.Enabled = mState.BodyEnabled;
        }

        protected readonly Xform _Transform;
        public override Xform mTransform { get { return _Transform; } }

        protected HashSet<Extension> Extensions;
        public void AddExtension(params Extension[] extensions)
        {
            foreach (Extension extension in extensions)
                Extensions.Add(extension);
        }
        public void RemoveExtension(params Extension[] extensions)
        {
            foreach (Extension extension in extensions)
                Extensions.Remove(extension);
        }
        public GameObject(Xform parent) : base()
        {
            _Transform = new Xform(this, parent);
            Extensions = new HashSet<Extension>();
            mState = new State();
        }
        public virtual void Update()
        {
            mTransform.mState.Modified = false;
            if (mBody != null)
            {
                FarseerPhysics.Common.Transform transform;
                mBody.GetTransform(out transform);
                // TODO: update mTransform from Farseer transform
            }
            foreach (Xform child in mTransform.Children)
                child.mGameObject.Update();
        }
        public virtual void Render()
        {
            GL.PushMatrix();
            GL.MultMatrix(ref mTransform._Local);
            Draw();
            foreach (Xform child in mTransform.Children)
                child.mGameObject.Render();
            GL.PopMatrix();
        }
        public virtual void Draw()
        {
            // Do a little dance
        }
        public void LateUpdate()
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
            foreach (Xform child in mTransform.Children)
                child.mGameObject.LateUpdate();
        }
    }
}