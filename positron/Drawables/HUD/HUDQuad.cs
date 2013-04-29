using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public class HUDQuad : IRenderable, IColorable
    {
        protected Color _Color;
		public Vector3d A, B, C, D;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
		public HUDQuad(RenderSet render_set, Vector3d a, Vector3d b, Vector3d c, Vector3d d)
		{
			A = a;
			B = b;
			C = c;
			D = d;
			render_set.Add(this);
		}
		public HUDQuad(RenderSet render_set, Vector3d p, Vector3d s) :
			this(render_set, p,
                 p + new Vector3d(s.X, 0.0, 0.0),
                 p + new Vector3d(s.X, s.Y, 0.0),
                 p + new Vector3d(0.0, s.Y, 0.0))
        {
        }
        public virtual void Render(double time)
        {
            // Unbind any texture that was previously bound
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.LineWidth(1);
            GL.Begin(BeginMode.Quads);
            GL.Color4(_Color);
            GL.Vertex3(A);
            GL.Vertex3(B);
            GL.Vertex3(C);
            GL.Vertex3(D);
            GL.End();
        }
		public virtual void Dispose()
		{
		}
    }
}

