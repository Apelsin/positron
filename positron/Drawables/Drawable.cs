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
        #region OpenGL
        #endregion
        protected Vector3d _Position = new Vector3d();
		protected Vector3d _Scale = new Vector3d();
		protected Vector3d _Velocity = new Vector3d();
		protected double _Theta = 0.0;
		protected bool _Preserve = false;
		#endregion
		#region Accessors
		public virtual Vector3d Position {
			get { return _Position; }
			set { _Position = value; }
		}
		public virtual double PositionX {
			get { return _Position.X; }
			set { _Position.X = value; }
		}
		public virtual double PositionY {
			get { return _Position.Y; }
			set { _Position.Y = value; }
		}
		public virtual double PositionZ {
			get { return _Position.Z; }
			set { _Position.Z = value; }
		}
        public virtual Vector3d Corner
        {
			get { return Position - new Vector3d(0.5 * SizeX, 0.5 * SizeY, PositionZ); }
            set { Position = value + new Vector3d(0.5 * SizeX, 0.5 * SizeY, PositionZ); }
        }
        public virtual double CornerX
        {
            get { return PositionX - 0.5 * SizeX; }
            set { PositionX = value + 0.5 * SizeX; }
        }
        public virtual double CornerY
        {
            get { return PositionY - 0.5 * SizeY; }
            set { PositionY = value + 0.5 * SizeY; }
        }
        public virtual double SizeX
        {
            get { return ScaleX; }
        }
        public virtual double SizeY
        {
            get { return ScaleY; }
        }
		public virtual Vector3d Velocity {
			get { return _Velocity; }
			set { _Velocity = value; }
		}
		public virtual double VelocityX {
			get { return _Velocity.X; }
			set { _Velocity.X = value; }
		}
		public virtual double VelocityY {
			get { return _Velocity.Y; }
			set { _Velocity.Y = value; }
		}
		public virtual Vector3d Scale {
			get { return _Scale; }
			set { _Scale = value; }
		}
		public virtual double ScaleX {
			get { return _Scale.X; }
			set { _Scale.X = value; }
		}
		public virtual double ScaleY {
			get { return _Scale.Y; }
			set { _Scale.Y = value; }
		}
		public virtual double Theta {
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
		public RenderSet RenderSet {
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
			RenderSetTransfer += (object sender, RenderSetChangeEventArgs e) => {
				this._RenderSet = e.To;
			};
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
		public abstract void Render(double time);
		protected virtual Vector3d CalculateMovementParallax ()
		{
			if(this._RenderSet == this._RenderSet.Scene.HUD)
				return Vector3d.Zero;
			double depth = 10.0 / MathUtil.Clamp(_Position.Z + 10.0, 1000.0, 0.1) - 1.0;
			return this._RenderSet.Scene.ViewPosition * (depth);
		}
		public virtual void Dispose ()
        {
            _RenderSet = null;
            if (_Blueprints != null) {
                _Blueprints.Clear ();
                _Blueprints = null;
            }
			RenderSetEntry = null;
			RenderSetTransfer = null;

		}
	}
}