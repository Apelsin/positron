using System;

namespace Positron
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
    public class SharedState<T> : IDisposable
    {
        public event SharedStateChangeEventHandler<T> SharedStateChanged;
        protected T _Value;
        public T Value { get { return _Value; } }
        public SharedState(T initial_state)
        {
            _Value = initial_state;
            SharedStateChanged += HandleSharedStateChanged;
        }
        protected void HandleSharedStateChanged (object sender, SharedStateChangeEventArgs<T> e)
        {
            _Value = e.CurrentState;
        }
        public void OnChange (object sender, T state)
        {
            if(!state.Equals(_Value))
                SharedStateChanged(sender, new SharedStateChangeEventArgs<T>(_Value, state, this));
        }
        public override bool Equals(object o)
        {
            return this._Value.Equals(this.Value);
        }
        public override int GetHashCode()
        {
            return _Value.GetHashCode();
        }
        public static bool operator == (SharedState<T> A, SharedState<T> B)
        {
            return A.Equals(B);
        }
        public static bool operator != (SharedState<T> A, SharedState<T> B)
        {
            return !A.Equals(B);
        }
        public static implicit operator T(SharedState<T> self)
        {
            return self.Value;
        }
        public virtual void Dispose()
        {
            this.SharedStateChanged = null;
        }
    }
}

