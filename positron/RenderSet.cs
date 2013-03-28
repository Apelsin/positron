using System;
using System.Collections.Generic;

namespace positron
{
	public class RenderSet : List<IRenderable>, IRenderable
	{
		protected Scene _Scene;
		protected Object _UpdateLock = new Object();
		public Scene Scene { get { return _Scene; } }
		public Object UpdateLock { get { return _UpdateLock; } }
		public RenderSet (Scene scene, params IRenderable[] renderables):
			base()
		{
			_Scene = scene;
			foreach(IRenderable renderable in renderables)
				Add (renderable);
		}
		public void Render (double time)
		{
			for(int i = 0; i < Count; i++)
				this[i].Render(time);
		}
		public void AddSafe (IRenderable item)
		{
			lock (_UpdateLock) {
				base.Add (item);
			}
		}
		public void RemoveSafe (IRenderable item)
		{
			lock (_UpdateLock) {
				base.Remove (item);
			}
		}
	}
}

