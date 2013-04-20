using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class BlueprintLine : IRenderable
	{
		protected int Lifespan;
		protected Stopwatch Timer = new Stopwatch();
		protected RenderSet RenderSet;
		public Vector3d A, B;
		public BlueprintLine (Vector3d a, Vector3d b, RenderSet render_set):
			this(a, b, render_set, 100)
		{
		}
		public BlueprintLine (Vector3d a, Vector3d b, RenderSet render_set, int millis)
		{
			RenderSet = render_set;
			RenderSet.Add(this);
			Lifespan = millis;
			A = a;
			B = b;
			Timer.Start();
		}
		public void Render (double time)
		{
			// Unbind any texture that was previously bound
			GL.BindTexture (TextureTarget.Texture2D, 0);
			GL.LineWidth (1);
			GL.Begin (BeginMode.Lines);
			GL.Color4 (Color.Crimson);
			GL.Vertex3 (A);
			GL.Color4 (Color.Gold);
 			GL.Vertex3 (B);
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

