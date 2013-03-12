using System;
using System.Collections.Generic;

namespace positron
{
	public class RenderSet : List<IRenderable>, IRenderable
	{
		public RenderSet ():
			base()
		{
		}
		public RenderSet (params IRenderable[] renderables):
			base()
		{
			foreach(IRenderable renderable in renderables)
				Add (renderable);
		}
		public void Render (double time)
		{
			foreach (IRenderable d in this) {
				d.Render(time);
			}
		}
	}
}

