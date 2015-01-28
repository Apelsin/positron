using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public abstract class Drawable : IRenderable, ISceneElement
	{
		public event RenderSetChangeEventHandler RenderSetEntry;
		public event RenderSetChangeEventHandler RenderSetTransfer;
		#region State
		#region Member Variables
		protected RenderSet _RenderSet;
		protected List<IRenderable> _Blueprints;
        protected Vector3 _Position = new Vector3();
		protected Vector3 _Scale = new Vector3();
		protected Vector3 _Velocity = new Vector3();
		protected float _Theta = 0.0f;
        protected float _Parallax = 0.0f;
		protected bool _Preserve = false;
		#endregion
		#region Accessors
		public virtual Vector3 Position {
			get { return _Position; }
			set { _Position = value; }
		}
		public virtual float PositionX {
			get { return _Position.X; }
			set { _Position.X = value; }
		}
		public virtual float PositionY {
			get { return _Position.Y; }
			set { _Position.Y = value; }
		}
		public virtual float PositionZ {
			get { return _Position.Z; }
			set { _Position.Z = value; }
		}
        public virtual float Parallax {
            get { return _Parallax; }
            set { _Parallax = value; }
        }
        public virtual Vector3 Corner
        {
			get { return Position - new Vector3(0.5f * SizeX, 0.5f * SizeY, PositionZ); }
            set { Position = value + new Vector3(0.5f * SizeX, 0.5f * SizeY, PositionZ); }
        }
        public virtual float CornerX
        {
            get { return PositionX - 0.5f * SizeX; }
            set { PositionX = value + 0.5f * SizeX; }
        }
        public virtual float CornerY
        {
            get { return PositionY - 0.5f * SizeY; }
            set { PositionY = value + 0.5f * SizeY; }
        }
        public virtual float SizeX
        {
            get { return ScaleX; }
        }
        public virtual float SizeY
        {
            get { return ScaleY; }
        }
		public virtual Vector3 Velocity {
			get { return _Velocity; }
			set { _Velocity = value; }
		}
		public virtual float VelocityX {
			get { return _Velocity.X; }
			set { _Velocity.X = value; }
		}
		public virtual float VelocityY {
			get { return _Velocity.Y; }
			set { _Velocity.Y = value; }
		}
		public virtual Vector3 Scale {
			get { return _Scale; }
			set { _Scale = value; }
		}
		public virtual float ScaleX {
			get { return _Scale.X; }
			set { _Scale.X = value; }
		}
		public virtual float ScaleY {
			get { return _Scale.Y; }
			set { _Scale.Y = value; }
		}
		public virtual float Theta {
			get { return _Theta; }
			set { _Theta = value; }
		}
		public virtual List<IRenderable> Blueprints {
			get { return _Blueprints; }
		}
		/// <summary>
		/// Gets the render set associated with this drawable.
		/// Specifying a null render set in the constructor
		/// implies loose render set association and manual
		/// tracking of the object.
		/// </summary>
		public RenderSet Set {
			get { return _RenderSet; }
			set { _RenderSet = value; }
		}
		public bool Preserve {
			get { return _Preserve; }
			set { _Preserve = value; }
		}
		#endregion
		#endregion
		public Drawable (RenderSet render_set)
		{
			_RenderSet = render_set;
            if(_RenderSet != null)
			    _RenderSet.Add(this);
            RenderSetTransfer += HandlerenderSetTransfer;
		}
        protected void HandlerenderSetTransfer (object sender, RenderSetChangeEventArgs e)
        {
            this._RenderSet = e.To;
        }
        /// <summary>
        /// Creates geometry information necessary for VBO
        /// This is either called in the constructor or
        /// called manually.
        /// </summary>
        public virtual void Build()
        {
            // Some inexpensive drawables are built each frame
            // Therefore it is not required to implement Build()
        }
		public virtual void OnRenderSetEntry(object sender, RenderSetChangeEventArgs e)
		{
			if(RenderSetEntry != null)
				RenderSetEntry(sender, e);
		}
		public virtual void OnRenderSetTransfer(object sender, RenderSetChangeEventArgs e)
		{
			if(RenderSetTransfer != null)
				RenderSetTransfer(sender, e);
		}
		public abstract void Render(float time);
		protected virtual Vector3 CalculateMovementParallax ()
		{
			if(_Parallax == 0.0 || this._RenderSet == this._RenderSet.Scene.HUD)
				return Vector3.Zero;
			float depth = 10.0f / MathUtil.Clamp(_Parallax + 10.0f, 1000.0f, 0.1f) - 1.0f;
			return this._RenderSet.Scene.ViewPosition * (depth);
		}
		public virtual void Dispose ()
        {
            _RenderSet = null;
            if (_Blueprints != null) {
                for(int i = 0; i < _Blueprints.Count; i++)
                {
                    if(_Blueprints[i].Set != null)
                        _Blueprints[i].Set.Remove(_Blueprints[i]);
                    _Blueprints[i].Dispose();
                    _Blueprints[i] = null;
                }
                _Blueprints.Clear ();
                _Blueprints = null;
            }
			RenderSetEntry = null;
			RenderSetTransfer = null;
		}
	}
}