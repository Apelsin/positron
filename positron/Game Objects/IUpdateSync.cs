using System;
using System.Collections.Generic;

namespace Positron
{
    public class UpdateEventArgs : EventArgs
    {
        protected float _Time;
        public float Time { get { return _Time; } set { _Time = value; } }
        public int _dbg_list_idx;
        public UpdateEventArgs(int dbg_list_idx = 0)
        {
            _dbg_list_idx = dbg_list_idx;
        }
    }
    public delegate bool UpdateEventHandler(object sender, UpdateEventArgs e);
    public interface IUpdateSync
    {
        //event UpdateEventHandler UpdateEvent;
        List<KeyValuePair<object, UpdateEventHandler>> UpdateEventList { get; }
        void Update();
    }
}
