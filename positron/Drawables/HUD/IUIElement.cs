using System;

namespace positron
{
	public delegate void RefreshHandler(object sender, EventArgs e);
	public interface IUIElement : IRenderable
	{
		event RefreshHandler Refresh;
		void OnRefresh(object sender, EventArgs e);
		UIElementGroup Group { get; }
	}
}

