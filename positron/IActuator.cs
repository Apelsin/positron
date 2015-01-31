using System;

namespace Positron
{
    public class ActionEventArgs : EventArgs
    {
        protected object _Info;
        protected object _Self;
        public object Info { get { return _Info; } set { _Info = value; } }
        public object Self { get { return _Self; } set { _Self = value; } }
        public ActionEventArgs(object info = null, object self = null)
        {
            _Info = info;
            _Self = self;
        }
    }
    public delegate void ActionEventHandler(object sender, ActionEventArgs e);
    public interface IActuator
    {
        event ActionEventHandler Action;
        void OnAction(object sender, ActionEventArgs e);
    }
}