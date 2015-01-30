using System;

namespace Positron
{
	public interface IStateShare<T>
	{
		SharedState<T> State { get; }
	}
}

