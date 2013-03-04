using System;
using System.Collections.Generic;

namespace positron
{
	public class RenderSet : List<Drawable>, IRenderable
	{
		public RenderSet ():
			base()
		{
		}
		public void Render ()
		{
			foreach (IRenderable d in this) {
				d.Render();
			}
		}
	}
}

