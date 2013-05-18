using System;
using System.Collections.Generic;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class SceneCredits : Scene
	{
        protected Stopwatch MainTimer = new Stopwatch();
        protected double RainTime = 0.0;
        protected Random RainRandy = new Random(349587234);
		public Dialog MainDialog;
        public SceneCredits (PositronGame game):
            base(game)
		{
			SceneEntry += (sender, e) => {
                var stanzas = new List<DialogStanza>();
                DialogSpeaker speaker = null;//DialogSpeaker.Get("protagonist");
                stanzas.Add(new DialogStanza(speaker,
                                             "Artwork:           Music:             Programming:\n" +
                                             "Fernando Corrales  A-Zu-Ra            Vince BG    \n" +
                                             "Megan Groden       Laurence Simmonds  Will Pham   \n" +
                                             "Vince BG           Vince BG                       \n" +
                                             "                   Will Pham                      "));
                var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
                dialog.DialogEnd += (sender2, e2) =>
                {
                    _Game.CurrentScene = ((Scene)_Game.Scenes["SceneFirstMenu"]);
                };
                dialog.Begin();
			};
			SetupPlayerOnExit();
            MainTimer.Restart();
		}
		public override void InitializeScene ()
		{
			base.InitializeScene ();
		}
        public override void Update(double time)
        {
            base.Update(time);
            if(MainTimer.Elapsed.TotalSeconds > RainTime)
            {
                for(int i = 0; i < 8; i++)
                    new RaindropRippleEffect(Stage, ViewWidth * RainRandy.NextDouble(), ViewHeight * RainRandy.NextDouble());
                RainTime = MainTimer.Elapsed.TotalSeconds + 0.05 * RainRandy.NextDouble() + 0.005;
            }
        }
        protected class RaindropRippleEffect : SpriteBase
        {
            protected Stopwatch Timer = new Stopwatch();
            protected double Lifespan;
            protected int LastFrameIndex;
            public RaindropRippleEffect(RenderSet render_set, double x, double y):
                base(render_set, x, y, Texture.Get("sprite_ripples"))
            {
                PlayAnimation(_AnimationDefault = new SpriteAnimation(Texture, 20, true, false, "f0", "f1"));
                Lifespan = 3.0;
                _Color = System.Drawing.Color.FromArgb(255, 24, 127, 255);
                Timer.Restart();
            }
            public override void Update (double time)
            {
                base.Update(time);
                _Scale.X = _Scale.Y += LastFrameIndex != 0 && FrameIndex == 0 ? 0.25 : 0.0;
                LastFrameIndex = FrameIndex;
                if (_Scale.X > Lifespan) {
                    _RenderSet.Remove (this);
                    Dispose ();
                } else {
                    float fx = (float)((Lifespan - _Scale.X) / Lifespan);
                    float gfx = (float)Math.Sqrt(fx);
                    _Color.A = gfx;
                    _Color.R = 0.5f * fx;
                    _Color.G = 0.5f * gfx;
                }
            }
        }
	}
}

