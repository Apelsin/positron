using System;
using System.Collections.Generic;

namespace positron
{
	public delegate void RenderSetChangeEventHandler(object sender, RenderSetChangeEventArgs e);
	public interface ISceneElement : IDisposable
	{
		event RenderSetChangeEventHandler RenderSetEntry;
		event RenderSetChangeEventHandler RenderSetTransfer;
		void OnRenderSetEntry (object sender, RenderSetChangeEventArgs e);
		void OnRenderSetTransfer (object sender, RenderSetChangeEventArgs e);
		bool Preserve { get; set; }
		RenderSet RenderSet { get; }
		List<IRenderable> Blueprints { get; }
		void Dispose();
	}
}