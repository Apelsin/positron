using System;
using System.Diagnostics;
using System.Threading;

namespace positron
{
    public class MusicLooper_LessHackish : SpriteBase
    {
        protected Sound CurrentLoop;
        protected Object CurrentLoopLock = new object();
        protected Thread LoopThread;
        protected Stopwatch RepeatTimer = new Stopwatch();
        protected int ResetLooperFlag = 0;
        protected int ExitLooperFlag = 0;
        public bool ResetLooper {
            get { return ResetLooperFlag != 0; }
            set { Interlocked.CompareExchange (ref ResetLooperFlag, value ? 1 : 0, value ? 0 : 1); }
        }
        public bool ExitLooper {
            get { return ExitLooperFlag != 0; }
            set { Interlocked.CompareExchange (ref ExitLooperFlag, value  ? 1 : 0, value ? 0 : 1); }
        }
        public MusicLooper_LessHackish (Scene scene, object loop_me):
            base(scene.HUD, -999, -999, Texture.DefaultTexture)
        {
            SetLoop(loop_me);
            LoopThread = new Thread(LoopThreadMain);
            LoopThread.Start();
        }
        public void SetLoop (object loop_me)
        {
            lock (CurrentLoopLock) {
                if(CurrentLoop != null)
                    CurrentLoop.Stop();
                CurrentLoop = Sound.Get (loop_me);
                ResetLooper = true;
            }
        }
        public void LoopThreadMain ()
        {
            double duration = 1.0;
            int ms = 1000;
            while (!ExitLooper)
            {
                lock(CurrentLoopLock)
                {
                    if(CurrentLoop == null)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    duration = CurrentLoop.Duration;
                    ms = (int)(1000 * duration);
                    RepeatTimer.Restart ();
                    CurrentLoop.Play (); // Do it!
                    ResetLooper = false;
                }
                while (!ExitLooper && !ResetLooper && RepeatTimer.Elapsed.TotalMilliseconds < ms)
                    Thread.Sleep (1); // Coarse wait
                if (!ExitLooper && !ResetLooper)
                    while (RepeatTimer.Elapsed.TotalMilliseconds < duration); // Fine wait
            }
            lock(CurrentLoopLock)
            {
                if(CurrentLoop != null)
                {
                    CurrentLoop.Stop();
                }
            }
        }
        public override void Dispose()
        {
            var sound = CurrentLoop;
            CurrentLoop = null;
            ExitLooper = true;
            LoopThread.Join();
            base.Dispose();
        }
    }
}

