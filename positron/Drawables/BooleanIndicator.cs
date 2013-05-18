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
			_Color = new Color4(Color.SkyBlue);
			_NextLayer = new LayeredSprite(render_set, x, y, 1.0, 1.0, null, Texture.Get ("sprite_indicator_gloss"));
		}
		protected override void Draw ()
		{
            GL.Color4(State ? _Color : Color.SlateGray);
            Texture.Bind();
            FrameCurrent.VBO.Render();
		}
	}
}