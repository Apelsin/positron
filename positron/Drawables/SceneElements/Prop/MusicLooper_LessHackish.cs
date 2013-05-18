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
        public MusicLooper_LessHackish (Scene scene):
            base(scene.HUD, -999, -999, Texture.DefaultTexture)
        {
            _Preserve = true;

            RenderSetEntry += (sender, e) =>
            {
                if(e.To.Scene is ISceneGameplay)
                    SetLoop("last_human_loop");
                else
                    SetLoop("induction_loop");
            };
            
            LoopThread = new Thread(LoopThreadMain);
            LoopThread.Start();
        }
        public void SetLoop (object loop_me, bool restart = false)
        {
            lock (CurrentLoopLock) {
                Sound new_loop = Sound.Get (loop_me);
                bool change = (new_loop != CurrentLoop);
                ResetLooper = change || restart;
                if(ResetLooper)
                {
                    if(CurrentLoop != null)
                        CurrentLoop.Stop();
                    if(change)
                        CurrentLoop = new_loop;
                }
            }
        }
        public void LoopThreadMain ()
        {
            double duration = 1.0;
            int ms = 1000;
            bool continue_flag = false;
            while (!ExitLooper)
            {
                lock(CurrentLoopLock)
                {
                    continue_flag = CurrentLoop == null;
                }
                if(continue_flag)
                {
                    Thread.Sleep(1);
                    continue;
                }
                lock(CurrentLoopLock)
                {
                    duration = CurrentLoop.Duration;
                    ms = Math.Max (0, (int)(1000 * duration) - 10); // Fine step margin
                    RepeatTimer.Restart ();
                    CurrentLoop.Play (); // Do it!
                    ResetLooper = false;
                }
                while (!ExitLooper && !ResetLooper && RepeatTimer.Elapsed.TotalMilliseconds < ms)
                    Thread.Sleep (1); // Coarse step
                if (!ExitLooper && !ResetLooper)
                    while (RepeatTimer.Elapsed.TotalMilliseconds < duration); // Fine step
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

