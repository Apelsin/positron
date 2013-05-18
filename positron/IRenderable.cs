using System;

namespace positron
{
	public interface IRenderable : IDisposable
	{
        RenderSet Set { get; }
        void Render(double time);
		void Dispose();
	}
}