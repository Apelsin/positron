using System;
using System.Collections.Generic;

namespace positron
{
    public class UpdateEventArgs : EventArgs
    {
        protected double _Time;
        public double Time { get { return _Time; } set { _Time = value; } }
        public UpdateEventArgs(double time)
        {
            _Time = time;
        }
    }
    public delegate void UpdateEventHandler(object sender, UpdateEventArgs e);
    public interface IUpdateSync
    {
        //event UpdateEventHandler UpdateEvent;
        Queue<UpdateEventHandler> UpdateEventQueue { get; }
        void Update(double time);
    }
}
