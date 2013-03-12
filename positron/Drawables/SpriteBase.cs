using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace positron
{
	public class SpriteBase : Drawable, IColorable
	{
		#region State
		#region Member Variables
		protected Color _Color;
		protected Texture _Texture;
		protected double _TileX;
		protected double _TileY;
		#endregion
		#region Member Accessors
		public Color Color {
			get { return _Color; }
			set { _Color = value; }
		}
		public Texture Texture {
			get { return _Texture; }
			set { _Texture = value; }
		}
		public double TileX {
			get { return _TileX; }
			set { _TileX = value; }
		}
		public double TileY {
			get { return _TileY; }
			set { _TileY = value; }
		}
		#endregion
		#endregion
		#region Behavior
		public SpriteBase():
			this(0.0, 0.0, 1.0, 1.0, Texture.DefaultTexture)
		{
		}
		public SpriteBase(Texture texture):
			this(0.0, 0.0, 1.0, 1.0, texture)
		{
		}
		public SpriteBase (double scalex, double scaley):
			this(0.0, 0.0, scalex, scaley, Texture.DefaultTexture)
		{		
		}
		// Main constructor:
		public SpriteBase (double scalex, double scaley, Texture texture):
			this(0.0, 0.0, scalex, scaley, texture)
		{
		}
		public SpriteBase (double x, double y, double scalex, double scaley, Texture texture)
		{
			// Size will scale _Texture width and height
			_Color = Color.White;
			_Size.X = scalex;
			_Size.Y = scaley;
			_Position.X = x;
			_Position.Y = y;
			_Texture = texture;
			_TileX = 1.0;
			_TileY = 1.0;
		}
		// TODO: rotation stuff here
		public override double RenderSizeX () { return _Size.X * _Texture.Width; }
		public override double RenderSizeY () { return _Size.Y * _Texture.Height; }
		public override void Render (double time)
		{
			double w = Size.X * _Texture.Width;
			double h = Size.Y * _Texture.Height;
			Texture.Bind(_Texture);
			GL.Color4 (_Color);
			GL.PushMatrix();
			{
				GL.Translate (_Position);
				GL.Rotate(_Theta, 0.0, 0.0, 1.0);
				//GL.Translate(Math.Floor (Position.X), Math.Floor (Position.Y), Math.Floor (Position.Z));
				GL.Begin (BeginMode.Quads);
				{
					GL.TexCoord2(0.0, 0.0);
					GL.Vertex2(0.0, 0.0);
					GL.TexCoord2(_TileX, 0.0);
					GL.Vertex2(w, 0.0);
					GL.TexCoord2(_TileX, -_TileY);
					GL.Vertex2(w, h);
					GL.TexCoord2(0.0, -_TileY);
					GL.Vertex2(0.0, h);
				}
				GL.End ();
			}
			GL.PopMatrix();
		}
		#endregion
	}
}

