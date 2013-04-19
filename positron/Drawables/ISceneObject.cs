using System;
using System.Collections.Generic;

namespace positron
{
	public interface ISceneObject
	{
		bool Preserve { get; set; }
		RenderSet RenderSet { get; }
		List<IRenderable> Blueprints { get; }
		/// <summary>
		/// Raised when the scene this object is in changes
		/// </summary>
		void SetChange (object sender, RenderSetChangeEventArgs e);
	}
}