using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public class HUDQuad : IRenderable, IColorable
    {
        protected RenderSet _RenderSet;
        protected Color _Color;
		public Vector3 A, B, C, D;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public RenderSet Set {
            get { return _RenderSet; }
        }
		public HUDQuad(RenderSet render_set, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
            _RenderSet = render_set;
            _RenderSet.Add(this);
		}
		public HUDQuad(RenderSet render_set, Vector3 p, Vector3 s) :
			this(render_set, p,
                 p + new Vector3(s.X, 0f,  0f),
                 p + new Vector3(s.X, s.Y, 0f),
                 p + new Vector3(0f,  s.Y, 0f))
        {
        }
        public virtual void Render(float time)
        {
            // Unbind any texture that was previously bound
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(_Color);
            GL.Vertex3(A);
            GL.Vertex3(B);
            GL.Vertex3(C);
            GL.Vertex3(D);
            GL.End();
        }
		public virtual void Dispose()
		{
            _RenderSet = null;
		}
    }
}

