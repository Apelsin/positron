using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace positron
{
	public class Circle : Drawable, IColorable
	{
		Color _Color;
		public Color Color {
			get { return _Color; }
			set { _Color = value; }
		}
		public float Radius { get; set; }

		public Circle (float radius)
		{
			Radius = radius;
		}
		public override void Render()
		{
			GL.PointSize(Radius);
			GL.Begin(BeginMode.Points);
			GL.Color4(Color);
			GL.Vertex2(PositionX, PositionY);
			GL.End();
		}
	}
}

