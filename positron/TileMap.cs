using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class TileMap: Drawable
	{
		protected Texture Texture;
		protected int _CountX, _CountY;
		protected int[,] IndexMap;
		public int CountX { get { return _CountX; } }
		public int CountY { get { return _CountY; } }
		public TileMap (RenderSet render_set, int countx, int county, Texture texture):
			base(render_set)
		{
			_CountX = countx;
			_CountY = county;
			Texture = texture;
			IndexMap = new int[_CountX,_CountY];
		}
		public void RandomMap ()
		{
			Random random = new Random((int)DateTime.Now.Ticks);
			for (int j = 0; j < _CountY; j++) {
				for(int i = 0; i < _CountX; i++) {
					IndexMap[i,j] = random.Next(Texture.Regions.Length);
				}
			}
		}
        public override void Build()
        {
            int idx = 0;
            var vertices = new Vertex[4 * _CountX * _CountY]; // 4 for BeginMode.Quads
            var tile_size = Texture.Regions[0].Size;
            for (int j = 0; j < _CountY; j++)
            {
                for (int i = 0; i < _CountX; i++)
                {
                    double x0 = Texture.Regions[IndexMap[i, j]].Low.X / Texture.Width;
                    double y0 = Texture.Regions[IndexMap[i, j]].Low.Y / Texture.Height;
                    double x1 = Texture.Regions[IndexMap[i, j]].High.X / Texture.Width;
                    double y1 = Texture.Regions[IndexMap[i, j]].High.Y / Texture.Height;
                    var A = new Vertex(tile_size.X * i,         tile_size.Y * j,        0.0, 0.0, 0.0, 1.0, x0, -y0);
                    var B = new Vertex(tile_size.X * (i + 1),   A.Position.Y,           0.0, 0.0, 0.0, 1.0, x1, -y0);
                    var C = new Vertex(B.Position.X,            tile_size.Y * (j + 1),  0.0, 0.0, 0.0, 1.0, x1, -y1);
                    var D = new Vertex(A.Position.X,            C.Position.Y,           0.0, 0.0, 0.0, 1.0, x0, -y1);
                    vertices[idx++] = A;
                    vertices[idx++] = B;
                    vertices[idx++] = C;
                    vertices[idx++] = D;
                    //new BlueprintQuad(A.Position, B.Position, C.Position, D.Position, _RenderSet);
                }
            }
            VBO = new VertexBuffer(vertices);
        }
		public override void Render(double time)
		{
			GL.PushMatrix();
			{
				// So much for DRY...
				//GL.Scale(Size);
				GL.Translate (_Position);
                Draw();
			}
			GL.PopMatrix();
		}
        public virtual void Draw()
        {
            GL.Color4(Color.White);
            Texture.Bind(); // Bind to (current) sprite texture
            VBO.Render(); // Render the vertex buffer object
        }
		public override double RenderSizeX()
		{
            return CountX * Texture.Regions[0].Size.X;
		}
		public override double RenderSizeY()
		{
            return CountY * Texture.Regions[0].Size.Y;
		}
	}
}

