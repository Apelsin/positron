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
		public LayeredSprite(RenderSet render_set, Texture texture, params Texture[] textures):
			this(render_set, 0.0, 0.0, 1.0, 1.0, null, texture, textures)
		{
		}
		public LayeredSprite(RenderSet render_set, LayeredSprite next_layer, Texture texture, params Texture[] textures):
			this(render_set, 0.0, 0.0, 1.0, 1.0, next_layer, texture, textures)
		{
		}
		public LayeredSprite (RenderSet render_set, double scalex, double scaley, LayeredSprite next_layer):
			this(render_set, 0.0, 0.0, scalex, scaley, next_layer, Texture.DefaultTexture)
		{		
		}
		
		public LayeredSprite (RenderSet render_set, double scalex, double scaley, LayeredSprite next_layer, Texture texture, params Texture[] textures):
			this(render_set, 0.0, 0.0, scalex, scaley, next_layer, texture, textures)
		{
		}
		// Main constructor:
		public LayeredSprite (RenderSet render_set, double x, double y, double scalex, double scaley, LayeredSprite next_layer, Texture texture, params Texture[] textures):
			base(render_set, x, y, scalex, scaley, texture, textures)
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
                Draw();
                if (_NextLayer != null)
                    _NextLayer.Draw();
			}
			GL.PopMatrix();
		}
	}
}