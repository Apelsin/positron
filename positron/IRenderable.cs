using System;

namespace positron
{
	public interface IRenderable : IDisposable
	{
        void Render(double time);
		void Dispose();
	}
}