using System;
using System.Collections.Generic;

namespace positron
{
    public class UpdateEventArgs : EventArgs
    {
        protected double _Time;
        public double Time { get { return _Time; } set { _Time = value; } }
		public int _dbg_list_idx;
        public UpdateEventArgs(double time, int dbg_list_idx = 0)
        {
            _Time = time;
			_dbg_list_idx = dbg_list_idx;
        }
    }
    public delegate bool UpdateEventHandler(object sender, UpdateEventArgs e);
    public interface IUpdateSync
    {
        //event UpdateEventHandler UpdateEvent;
		List<KeyValuePair<object, UpdateEventHandler>> UpdateEventList { get; }
        void Update(double time);
    }
}
