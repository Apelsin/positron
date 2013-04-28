using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class BlueprintLineLoop : IRenderable
	{
		protected int Lifespan;
		protected Stopwatch Timer;
		protected ISceneElement _Instance;
		public Vector3d[] Vertices;
		public BlueprintLineLoop (ISceneElement instance, params Vector3d[] vertices):
			this(instance, 0, vertices)
		{
		}
		public BlueprintLineLoop (ISceneElement instance, int millis, params Vector3d[] vertices)
		{
			_Instance = instance;
			Vertices = vertices;
			Lifespan = millis;
            if (Lifespan > 0)
            {
                Timer = new Stopwatch();
                Timer.Start();
            }
		}
		protected IEnumerable<Color> ColorSequence ()
		{
			for(;;) {
				yield return Color.Crimson;
				yield return Color.Gold;

			}
		}
		public virtual void Render (double time)
		{
            if (Lifespan > 0 && Timer.ElapsedMilliseconds > Lifespan)
				_Instance.Blueprints.Remove(this);
            else
            {
				IEnumerator<Color> color_enumerator = ColorSequence().GetEnumerator();
                // Unbind any texture that was previously bound
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.LineWidth(1);
                GL.Begin(BeginMode.LineLoop);
				for(int i = 0; i < Vertices.Length; i++)
				{
					color_enumerator.MoveNext();
					Color color = color_enumerator.Current;
					GL.Color4(color);
					GL.Vertex3(Vertices[i]);
				}
                GL.End();
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

