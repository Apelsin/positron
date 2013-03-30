using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public class HUDQuad : BlueprintQuad, IColorable
    {
        protected Color _Color;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public HUDQuad(Vector3d a, Vector3d b, Vector3d c, Vector3d d, RenderSet render_set) :
            this(a, b, c, d, render_set, 100)
        {
        }
        public HUDQuad(Vector3d p, Vector3d s, RenderSet render_set, int millis) :
            this(p,
                 p + new Vector3d(s.X, 0.0, 0.0),
                 p + new Vector3d(s.X, s.Y, 0.0),
                 p + new Vector3d(0.0, s.Y, 0.0),
                 render_set, millis)
        {
        }
        public HUDQuad(Vector3d a, Vector3d b, Vector3d c, Vector3d d, RenderSet render_set, int millis) :
            base(a, b, c, d, render_set, millis)
        {
        }
        public override void Render(double time)
        {
            if (Lifespan > 0 && Timer.ElapsedMilliseconds > Lifespan)
                _RenderSet.Remove(this);
            else
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
        }
    }
}

