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
		Color _Color;
		Texture _Texture;
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
		#endregion
		#endregion
		#region Behavior
		public SpriteBase():
			this(0.0, 0.0, 1.0, 1.0)
		{
		}
		public SpriteBase (double scalex, double scaley):
			this(0.0, 0.0, scalex, scaley)
		{		
		}
		public SpriteBase (double x, double y, double scalex, double scaley)
		{
			// Size will scale _Texture width and height
			SizeX = scalex;
			SizeY = scaley;
			PositionX = x;
			PositionY = y;
			_Texture = Texture.Get("sprite_small_disc");
		}
		public override void Render ()
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
					GL.TexCoord2(0.0f, 0.0f);
					GL.Vertex2(0.0f, 0.0f);
					GL.TexCoord2(1.0f, 0.0f);
					GL.Vertex2(w, 0.0f);
					GL.TexCoord2(1.0f, 1.0f);
					GL.Vertex2(w, h);
					GL.TexCoord2(0.0f, 1.0f);
					GL.Vertex2(0.0f, h);
				}
				GL.End ();
			}
			GL.PopMatrix();
		}
		#endregion
	}
}

