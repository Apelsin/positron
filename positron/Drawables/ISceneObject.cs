using System;

namespace positron
{
	public interface ISceneObject
	{
		bool Preserve { get; set; }
		RenderSet RenderSet { get; }
		/// <summary>
		/// Raised when the scene this object is in changes
		/// </summary>
		void SetChange (object sender, SetChangeEventArgs e);
	}
}