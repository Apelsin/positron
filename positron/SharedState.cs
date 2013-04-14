using System;

namespace positron
{
	public class SharedStateChangeEventArgs<T> : EventArgs
	{
		protected T _PreviousState;
		protected T _CurrentState;
		protected SharedState<T> _Self;
		public T PreviousState { get { return _PreviousState; } set { _PreviousState = value; } }
		public T CurrentState { get { return _CurrentState; } set { _CurrentState = value; } }
		public SharedState<T> Self { get { return _Self; } set { _Self = value; } }
		public SharedStateChangeEventArgs(T previous_state, T current_state, SharedState<T> self)
		{
			_PreviousState = previous_state;
			_CurrentState = current_state;
			_Self = self;
		}
	}
	public delegate void SharedStateChangeEventHandler<T>(object sender, SharedStateChangeEventArgs<T> e);
	public class SharedState<T>
	{
		public event SharedStateChangeEventHandler<T> SharedStateChanged;
		protected object _ValueLock = new object();
		protected T _Value;
		public T Value { get { return _Value; } }
		public SharedState(T initial_state)
		{
			_Value = initial_state;
			SharedStateChanged += (sender, e) => {
				lock(_ValueLock)
					_Value = e.CurrentState;
			};
		}
		public void OnChange (object sender, T state)
		{
			lock (_ValueLock) {
				if(!state.Equals(_Value))
					SharedStateChanged(sender, new SharedStateChangeEventArgs<T>(_Value, state, this));
			}
		}
		public static bool operator == (SharedState<T> A, SharedState<T> B)
		{
			bool test;
			lock (A._ValueLock) {
				lock(B._ValueLock)
				{
					test = A._Value.Equals(B.Value);
				}
			}
			return test;
		}
		public static bool operator != (SharedState<T> A, SharedState<T> B)
		{
			bool test;
			lock (A._ValueLock) {
				lock(B._ValueLock)
				{
					test = !A._Value.Equals(B.Value);
				}
			}
			return test;
		}
		public static implicit operator T(SharedState<T> self)
		{
			T self__value;
			lock(self._ValueLock)
				self__value = self.Value;
			return self__value;
		}
	}
}

