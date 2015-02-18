using System;

namespace Positron
{
    public class FireEventArgs : EventArgs
    {
        protected object Info { get; set; }
    }
    public delegate void FireEventHandler(object sender, FireEventArgs e);
    public interface IShooter
    {
        event FireEventHandler Fire;
        void OnFire(object sender, FireEventArgs e);
    }
}

