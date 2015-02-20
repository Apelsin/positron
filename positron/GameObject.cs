using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    [DataContract]
    public abstract class GameObjectBase
    {
        [OnDeserialized]
        internal void _OnDeserialized(StreamingContext context)
        {
            OnDeserialized(context);
        }
        internal virtual void OnDeserialized(StreamingContext context)
        {

        }
        internal string _ElementId;
        [DataMember]
        internal string ElementId {
            get { return _ElementId; }
            set { _ElementId = value; }
        }
        public abstract Xform mTransform { get; internal set; }
        public ThreadedRendering mWindow { get { return mGame.Window; } }
        public PositronGame mGame { get { return mScene.Game; } }
        public FarseerPhysics.Dynamics.World mWorld { get { return mScene.World; } }
        public virtual Scene mScene {
            get { return mTransform.Parent.mScene; }
            internal set { mTransform.Parent.mScene = value; }
        }
        public GameObjectBase() : base()
        {
            _ElementId = this.GetType().FullName + "#" + GetHashCode().ToString();
        }
        public virtual FarseerPhysics.Dynamics.Body mBody { get { return null; } }
        public virtual SpriteBase mSprite { get { return null; } }
        public virtual Camera mCamera {
            get { return mTransform.Parent.mCamera; }
            internal set { mTransform.Parent.mCamera = value; }
        }
    }
    
    [DataContract]
    public class GameObject : GameObjectBase, IDrawable
    {
        [DataContract]
        public class State
        {
            [DataMember]
            public bool BodyEnabled = true;
            /// <summary>
            /// Flag for whether or not this object should be preserved across Scene changes.
            /// If enabled, the object will be moved to the entering scene.
            /// Only works for objects that have the root node as their parent.
            /// </summary>
            [DataMember]
            public bool Persist = false;
        }

        protected State _State;
        [DataMember]
        public State mState
        {
            get { return _State; }
            internal set { _State = value; }
        }

        /// <summary>
        /// Runtime method to load state for object that has become active
        /// </summary>
        public void SaveState()
        {
            if (mBody != null)
                mState.BodyEnabled = mBody.Enabled;
        }
        /// <summary>
        /// Runtime method to load state for object that has become inactive
        /// </summary>
        public void LoadState()
        {
            if(mBody != null)
                mBody.Enabled = mState.BodyEnabled;
        }

        protected Xform _Transform;
        public override Xform mTransform {
            get { return _Transform; }
            internal set { _Transform = value; }
        }

        [DataMember]
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
        public GameObject(GameObject parent)
            : this(parent.mTransform)
        {
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
            GL.LoadMatrix(ref mTransform._Global);
            Draw();
            foreach (Xform child in mTransform.Children)
                child.mGameObject.Render();
        }
        public virtual void Draw()
        {
            // Do a little dance
        }
        public void LateUpdate()
        {
            mTransform.UpdateGlobalMatrix();
            if (mBody != null)
            {
                float x = mTransform._Global[0, 0]; // X+ vector
                float y = mTransform._Global[1, 0]; // Y+ vector
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