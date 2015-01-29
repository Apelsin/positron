using System;

namespace positron
{
	public interface IRenderable : IDisposable
	{
        RenderSet Set { get; }
        void Render(float time);
	}
}