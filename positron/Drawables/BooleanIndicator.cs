using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace positron
{
	public class BooleanIndicator: LayeredSprite
	{
		public bool State { get; set; }
		// Main constructor:
		public BooleanIndicator (double x, double y):
			base(x, y, 1.0, 1.0, Texture.Get("indicator"), null)
		{
			State = false;
			_Color = Color.SkyBlue;
			_NextLayer = new LayeredSprite(x, y, 1.0, 1.0, Texture.Get ("indicator_gloss"), null);
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
		public override void DrawQuads()
		{
			double w = Size.X * _Texture.Width;
			double h = Size.Y * _Texture.Height;
			Texture.Bind(_Texture);
			GL.Color4 (State ? _Color : Color.DarkSlateGray);
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