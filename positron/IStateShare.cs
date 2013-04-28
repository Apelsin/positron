using System;

namespace positron
{
	public interface IStateShare<T>
	{
		SharedState<T> State { get; }
	}
}

