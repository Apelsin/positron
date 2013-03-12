using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace positron
{
	public class LayeredSprite : SpriteBase
	{
		protected LayeredSprite _NextLayer;
		public LayeredSprite NextLayer {
			get { return _NextLayer; }
			set { _NextLayer = value; }
		}
		public LayeredSprite():
			this(0.0, 0.0, 1.0, 1.0, Texture.DefaultTexture, null)
		{
		}
		public LayeredSprite(Texture texture):
			this(0.0, 0.0, 1.0, 1.0, texture, null)
		{
		}
		public LayeredSprite(Texture texture, LayeredSprite next_layer):
			this(0.0, 0.0, 1.0, 1.0, texture, next_layer)
		{
		}
		public LayeredSprite (double scalex, double scaley, LayeredSprite next_layer):
			this(0.0, 0.0, scalex, scaley, Texture.DefaultTexture, next_layer)
		{		
		}
		
		public LayeredSprite (double scalex, double scaley, Texture texture, LayeredSprite next_layer):
			this(0.0, 0.0, scalex, scaley, texture, next_layer)
		{
		}
		// Main constructor:
		public LayeredSprite (double x, double y, double scalex, double scaley, Texture texture, LayeredSprite next_layer):
			base(x, y, scalex, scaley, texture)
		{
			_NextLayer = next_layer;
		}
		// TODO: rotation stuff here
		public override void Render (double time)
		{
			GL.PushMatrix();
			{
				GL.Translate (_Position);
				GL.Rotate(_Theta, 0.0, 0.0, 1.0);
				DrawQuads();
			}
			GL.PopMatrix();
		}
		public virtual void DrawQuads()
		{
			double w = Size.X * _Texture.Width;
			double h = Size.Y * _Texture.Height;
			Texture.Bind(_Texture);
			GL.Color4 (_Color);
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
			if(_NextLayer != null)
				_NextLayer.DrawQuads();
		}
	}
}