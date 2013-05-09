using System;
using System.Diagnostics;

using OpenTK;

namespace positron
{
    public class SceneWin: Scene
    {
        Stopwatch derp_timer = new Stopwatch();
        SpriteBase win_thing;
        public SceneWin ():
            base()
        {
            SceneEntry += (sender, e) => {
                if(Program.MainGame.Player1 != null)
                {
                    Program.MainGame.Player1.Derez();
					derp_timer.Start();
                }
            };
        }
        protected override void InitializeScene()
        {
            win_thing = new SpriteBase(HUD, ViewWidth / 2.0, ViewHeight / 2.0, Texture.Get ("sprite_win")).CenterShift();
            base.InitializeScene();
        }
        public override void Update(double time)
        {
            double fx = 2 - Math.Cos(MathHelper.TwoPi * derp_timer.Elapsed.TotalSeconds);
            double gx = 5 * MathHelper.Pi * Math.Sin(MathHelper.Pi * derp_timer.Elapsed.TotalSeconds);
            win_thing.Scale = new Vector3d(fx);
            win_thing.Theta = gx;
            base.Update(time);
			if (derp_timer.Elapsed.TotalSeconds > 5.0) {
				lock(Program.MainUpdateLock)
					Scene.InstantiateScenes (ref Program.MainGame);
			}
        }
    }
}

