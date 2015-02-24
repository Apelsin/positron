using System;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public class TileMap: GameObject
    {
        protected Texture Texture;
        protected int _CountX, _CountY;
        protected int[,] IndexMap;
        public int CountX { get { return _CountX; } }
        public int CountY { get { return _CountY; } }
        
        protected VertexBuffer VBO;
        public TileMap (SceneRoot render_set, int countx, int county, Texture texture):
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
        public virtual void Build()
        {
            int idx = 0;
            var vertices = new Vertex[4 * _CountX * _CountY]; // 4 for BeginMode.Quads
            var tile_size = Texture.Regions[0].Size;
            for (int j = 0; j < _CountY; j++)
            {
                for (int i = 0; i < _CountX; i++)
                {
                    float x0 = Texture.Regions[IndexMap[i, j]].Low.X / Texture.Width;
                    float y0 = Texture.Regions[IndexMap[i, j]].Low.Y / Texture.Height;
                    float x1 = Texture.Regions[IndexMap[i, j]].High.X / Texture.Width;
                    float y1 = Texture.Regions[IndexMap[i, j]].High.Y / Texture.Height;
                    var A = new Vertex(tile_size.X * i,         tile_size.Y * j,        0.0f, x0, -y0);
                    var B = new Vertex(tile_size.X * (i + 1),   A.Position.Y,           0.0f, x1, -y0);
                    var C = new Vertex(B.Position.X,            tile_size.Y * (j + 1),  0.0f, x1, -y1);
                    var D = new Vertex(A.Position.X,            C.Position.Y,           0.0f, x0, -y1);
                    vertices[idx++] = A;
                    vertices[idx++] = B;
                    vertices[idx++] = C;
                    vertices[idx++] = D;
                    //new BlueprintQuad(A.Position, B.Position, C.Position, D.Position, _RenderSet);
                }
            }
            VBO = new VertexBuffer(vertices);
        }
        public override void Render()
        {
            GL.Color4(Color.White);
            Texture.Bind(); // Bind to (current) sprite texture
            VBO.Render(); // Render the vertex buffer object
        }
    }
    public class FadedTileMap : TileMap
    {
        public FadedTileMap (SceneRoot render_set, int countx, int county, Texture texture):
            base(render_set, countx, county, texture)
        {
        }
        public override void Build()
        {
            int idx = 0;
            var vertices = new Vertex[4 * _CountX * _CountY]; // 4 for BeginMode.Quads
            var tile_size = Texture.Regions[0].Size;
            for (int j = 0; j < _CountY; j++)
            {
                float height_mag = (float)(j + 1) / (float)(_CountY);
                for (int i = 0; i < _CountX; i++)
                {
                    float x0 = Texture.Regions[IndexMap[i, j]].Low.X / Texture.Width;
                    float y0 = Texture.Regions[IndexMap[i, j]].Low.Y / Texture.Height;
                    float x1 = Texture.Regions[IndexMap[i, j]].High.X / Texture.Width;
                    float y1 = Texture.Regions[IndexMap[i, j]].High.Y / Texture.Height;
                    var A = new Vertex(tile_size.X * i,         tile_size.Y * j,        0.0f, x0, -y0, 1.0f, 1.0f, 1.0f, height_mag);
                    var B = new Vertex(tile_size.X * (i + 1),   A.Position.Y,           0.0f, x1, -y0, 1.0f, 1.0f, 1.0f, height_mag);
                    var C = new Vertex(B.Position.X,            tile_size.Y * (j + 1),  0.0f, x1, -y1, 1.0f, 1.0f, 1.0f, height_mag);
                    var D = new Vertex(A.Position.X,            C.Position.Y,           0.0f, x0, -y1, 1.0f, 1.0f, 1.0f, height_mag);
                    vertices[idx++] = A;
                    vertices[idx++] = B;
                    vertices[idx++] = C;
                    vertices[idx++] = D;
                    //new BlueprintQuad(A.Position, B.Position, C.Position, D.Position, _RenderSet);
                }
            }
            VBO = new VertexBuffer(vertices);
        }
    }
}
