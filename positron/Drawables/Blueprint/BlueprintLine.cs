using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public class BlueprintLine : IRenderable
    {
        protected int Lifespan;
        protected Stopwatch Timer = new Stopwatch();
        protected RenderSet _RenderSet;
        public RenderSet Set { get { return _RenderSet; } }
        public Vector3 A, B;
        public BlueprintLine (Vector3 a, Vector3 b, RenderSet render_set):
            this(a, b, render_set, 100)
        {
        }
        public BlueprintLine (Vector3 a, Vector3 b, RenderSet render_set, int millis)
        {
            _RenderSet = render_set;
            _RenderSet.Add(this);
            Lifespan = millis;
            A = a;
            B = b;
            Timer.Start();
        }
        public void Render (float time)
        {
            // Unbind any texture that was previously bound
            GL.BindTexture (TextureTarget.Texture2D, 0);
            GL.LineWidth (1);
            GL.Begin (PrimitiveType.Lines);
            GL.Color4 (Color.Crimson);
            GL.Vertex3 (A);
            GL.Color4 (Color.Gold);
             GL.Vertex3 (B);
            GL.End ();
            if (Timer.ElapsedMilliseconds > Lifespan) {
                _RenderSet.Remove(this);
            }
        }
        public float RenderSizeX()
        {
            return 0;
        }
        public float RenderSizeY()
        {
            return 0;
        }
        public virtual void Dispose()
        {
            _RenderSet = null;
        }
    }
}

