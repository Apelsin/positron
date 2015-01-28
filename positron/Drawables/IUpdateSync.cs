using System;
using System.Collections.Generic;

namespace positron
{
    public class UpdateEventArgs : EventArgs
    {
        protected float _Time;
        public float Time { get { return _Time; } set { _Time = value; } }
		public int _dbg_list_idx;
        public UpdateEventArgs(float time, int dbg_list_idx = 0)
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
        void Update(float time);
    }
}
