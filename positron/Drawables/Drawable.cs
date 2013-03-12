using System;
using OpenTK;

namespace positron
{
	public abstract class Drawable : IRenderable
	{
		#region State
		#region Member Variables
		protected Vector3d _Position = new Vector3d();
		protected Vector3d _Size = new Vector3d();
		protected Vector3d _Velocity = new Vector3d();
		protected double _Theta = 0.0;
		#endregion
		#region Accessors
		public Vector3d Position {
			get { return _Position; }
			set { _Position = value; }
		}
		public double PositionX {
			get { return _Position.X; }
			set { _Position.X = value; }
		}
		public double PositionY {
			get { return _Position.Y; }
			set { _Position.Y = value; }
		}
		public double PositionZ {
			get { return _Position.Z; }
			set { _Position.Z = value; }
		}
		public Vector3d Velocity {
			get { return _Velocity; }
			set { _Velocity = value; }
		}
		public double VelocityX {
			get { return _Velocity.X; }
			set { _Velocity.X = value; }
		}
		public double VelocityY {
			get { return _Velocity.Y; }
			set { _Velocity.Y = value; }
		}
		public Vector3d Size {
			get { return _Size; }
			set { _Size = value; }
		}
		public double SizeX {
			get { return _Size.X; }
			set { _Size.X = value; }
		}
		public double SizeY {
			get { return _Size.Y; }
			set { _Size.Y = value; }
		}
		public double Theta {
			get { return _Theta; }
			set { _Theta = value; }
		}
		#endregion
		#endregion
		public Drawable ()
		{

		}
		public abstract void Render(double time);
		public abstract double RenderSizeX();
		public abstract double RenderSizeY();
	}
}