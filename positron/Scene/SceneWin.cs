using System;
using System.Diagnostics;

using OpenTK;

namespace positron
{
    public class SceneWin: Scene
    {
        Stopwatch DerpTimer = new Stopwatch();
        SpriteBase WinThing;
        public SceneWin ():
            base()
        {
            SceneEntry += (sender, e) => {
                if(Program.MainGame.Player1 != null)
                {
                    Program.MainGame.Player1.Derez();
					DerpTimer.Start();
                }
            };
        }
        protected override void InitializeScene()
        {
            WinThing = new SpriteBase(HUD, ViewWidth / 2.0, ViewHeight / 2.0, Texture.Get ("sprite_win")).CenterShift();
            base.InitializeScene();
        }
        public override void Update(double time)
        {
            double fx = 2 - Math.Cos(MathHelper.TwoPi * DerpTimer.Elapsed.TotalSeconds);
            double gx = 5 * MathHelper.Pi * Math.Sin(MathHelper.Pi * DerpTimer.Elapsed.TotalSeconds);
            WinThing.Scale = new Vector3d(fx);
            WinThing.Theta = gx;
            base.Update(time);
			if (DerpTimer.Elapsed.TotalSeconds > 5.0) {
				lock(Program.MainUpdateLock)
					Scene.InstantiateScenes (ref Program.MainGame);
			}
        }
    }
}

