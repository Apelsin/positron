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
		public BooleanIndicator (RenderSet render_set, double x, double y):
			base(render_set, x, y, 1.0, 1.0, null, Texture.Get("sprite_indicator"))
		{
			State = false;
			_Color = Color.SkyBlue;
			_NextLayer = new LayeredSprite(render_set, x, y, 1.0, 1.0, null, Texture.Get ("sprite_indicator_gloss"));
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
			double w = Size.X * Texture.Width;
			double h = Size.Y * Texture.Height;
			Texture.Bind(Texture);
			GL.Color4 (State ? _Color : Color.DarkSlateGray);
			DrawQuad ();
			if(_NextLayer != null)
				_NextLayer.DrawQuads();
		}
	}
}