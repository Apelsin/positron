using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public abstract class Drawable : IRenderable
	{
		#region State
		#region Member Variables
		protected RenderSet _RenderSet;
        #region OpenGL
        /// <summary>
        /// Vertex Buffer Object this drawable will use in Render()
        /// </summary>
        protected VertexBuffer VBO;
        #endregion
        protected Vector3d _Position = new Vector3d();
		protected Vector3d _Size = new Vector3d();
		protected Vector3d _Velocity = new Vector3d();
		protected double _Theta = 0.0;
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
		public virtual Vector3d Size {
			get { return _Size; }
			set { _Size = value; }
		}
		public virtual double SizeX {
			get { return _Size.X; }
			set { _Size.X = value; }
		}
		public virtual double SizeY {
			get { return _Size.Y; }
			set { _Size.Y = value; }
		}
		public virtual double Theta {
			get { return _Theta; }
			set { _Theta = value; }
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
		#endregion
		#endregion
		public Drawable (RenderSet render_set)
		{
			_RenderSet = render_set;
            if(_RenderSet != null)
			    _RenderSet.Add(this);
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
		public abstract void Render(double time);
		public abstract double RenderSizeX();
		public abstract double RenderSizeY();
	}
}