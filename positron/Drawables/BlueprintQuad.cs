using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class BlueprintQuad : IRenderable
	{
		protected int Lifespan;
		protected Stopwatch Timer = new Stopwatch();
		protected RenderSet RenderSet;
		public Vector3d A, B, C, D;
		public BlueprintQuad (Vector3d a, Vector3d b, Vector3d c, Vector3d d, RenderSet render_set):
			this(a, b, c, d, render_set, 100)
		{
		}
		public BlueprintQuad (Vector3d a, Vector3d b, Vector3d c, Vector3d d, RenderSet render_set, int millis)
		{
			RenderSet = render_set;
			Lifespan = millis;
			A = a;
			B = b;
			C = c;
			D = d;
			Timer.Start();
		}
		public void Render (double time)
		{
			// Unbind any texture that was previously bound
			GL.BindTexture (TextureTarget.Texture2D, 0);
			GL.LineWidth (1);
			GL.Begin (BeginMode.LineLoop);
			GL.Color4 (Color.Crimson);
			GL.Vertex3 (A);
			GL.Color4 (Color.Gold);
 			GL.Vertex3 (B);
			GL.Color4 (Color.Crimson);
			GL.Vertex3 (C);
			GL.Color4 (Color.Gold);
			GL.Vertex3 (D);
			GL.End ();
			if (Timer.ElapsedMilliseconds > Lifespan) {
				RenderSet.Remove(this);
			}
		}
		public double RenderSizeX()
		{
			return 0;
		}
		public double RenderSizeY()
		{
			return 0;
		}
	}
}

