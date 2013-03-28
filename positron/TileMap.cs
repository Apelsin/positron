using System;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class TileMap: Drawable
	{
		protected Texture[] Textures;
		protected int _CountX, _CountY;
		protected int[,] IndexMap;
		public int CountX { get { return _CountX; } }
		public int CountY { get { return _CountY; } }
		public TileMap (RenderSet render_set, int countx, int county, Texture texture, params Texture[] textures):
			base(render_set)
		{
			_CountX = countx;
			_CountY = county;
			Textures = new Texture[textures.Length + 1];
			Textures[0] = texture;
			for(int i = 0; i < textures.Length; i++)
				Textures[i + 1] = textures[i];
			IndexMap = new int[_CountX,_CountY];
		}
		public void RandomMap ()
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			int idx = 0;
			for (int j = 0; j < _CountY; j++) {
				for(int i = 0; i < _CountX; i++) {
					//IndexMap[i,j] = (j + idx++) % Textures.Length;
					IndexMap[i,j] = random.Next(Textures.Length);
				}
			}
		}
		public override void Render(double time)
		{
			int idx = 0;
			Texture t = Textures[0];
			GL.PushMatrix();
			{
				// So much for DRY...
				//GL.Scale(Size);
				GL.Translate (_Position);
				for(int j = 0; j < _CountY; j++)
				{
					GL.PushMatrix();
					{
						for(int i = 0; i < _CountX; i++)
						{
							// TODO: Map this to an array
							(t = Textures[IndexMap[i,j]]).Bind ();
							GL.Begin (BeginMode.Quads);
							GL.TexCoord2(0.0,  0.0);		GL.Vertex2(0.0, 	0.0		);
							GL.TexCoord2(1.0,  0.0);		GL.Vertex2(t.Width,	0.0		);
							GL.TexCoord2(1.0, -1.0);		GL.Vertex2(t.Width, t.Height);
							GL.TexCoord2(0.0, -1.0);		GL.Vertex2(0.0, 	t.Height);
							GL.End ();
							GL.Translate(t.Width, 0.0f, 0.0f);
						}
					}
					GL.PopMatrix();
					GL.Translate(0.0f, t.Height, 0.0f);
				}
			}
			GL.PopMatrix();
		}
		public override double RenderSizeX()
		{
			return CountX * Textures[0].Width * SizeX;
		}
		public override double RenderSizeY()
		{
			return CountY * Textures[0].Height * SizeY;
		}
	}
}

