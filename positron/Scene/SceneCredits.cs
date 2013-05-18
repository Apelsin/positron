using System;
using System.Collections.Generic;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class SceneCredits : Scene
	{
        protected Stopwatch MainTimer = new Stopwatch();
        protected double RainTime = 0.0;
        protected Random RainRandy = new Random(349587234);
        protected double FloorR = 1280;
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
                                             "                   Will Pham                      \n"));
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
                for (int i = 0; i < 16; i++)
                {
                    double theta = MathHelper.TwoPi * RainRandy.NextDouble();
                    new RaindropRippleEffect(Stage, FloorR * RainRandy.NextDouble() * Math.Cos(theta), FloorR * RainRandy.NextDouble() * Math.Sin(theta));
                }
                RainTime = MainTimer.Elapsed.TotalSeconds + 0.05 * RainRandy.NextDouble() + 0.005;
            }
        }
        public override void Render(double time)
        {
            lock (_RenderLock)
            {
                GL.PushMatrix();
                {
                    if (_FollowTarget != null)
                        CalculatePan((float)time);
                    GL.Translate(Math.Round(_ViewPosition.X), Math.Round(_ViewPosition.Y), Math.Round(_ViewPosition.Z));
                    Background.Render(time);
                    Rear.Render(time);
                    // <edit>
                    GL.PushMatrix();
                    {
                        double dx = 0.5 * ViewWidth;
                        double dy = 0.5 * ViewHeight;
                        GL.Translate(dx, dy, -100.0);

                        GL.Rotate(-60, 1.0, 0.0, 0.0);
                        GL.Rotate(MainTimer.Elapsed.TotalSeconds * 6.0, 0.0, 0.0, 1.0);

                        //double dx1 = 0.5 * FloorR;
                        //double dy1 = 0.5 * FloorR;
                        //GL.Translate(-dx1, -dy1, 0.0);
                       
                        Stage.Render(time);
                    }
                    GL.PopMatrix();
                    // </edit>
                    Tests.Render(time);
                    Front.Render(time);
                    if (Configuration.DrawBlueprints)
                        WorldBlueprint.Render(time);
                }
                GL.PopMatrix();
                HUD.Render(time);
                if (Configuration.DrawBlueprints)
                    HUDBlueprint.Render(time);
                if (Configuration.ShowDebugVisuals)
                    HUDDebug.Render(time);
            }
        }
        protected class RaindropRippleEffect : SpriteBase
        {
            private static Random ColorRandy = new Random(35678478);
            protected Stopwatch Timer = new Stopwatch();
            protected double Lifespan;
            protected int LastFrameIndex;
            protected Color4 _Color2;
            public RaindropRippleEffect(RenderSet render_set, double x, double y):
                base(render_set, x, y, Texture.Get("sprite_ripples"))
            {
                //PlayAnimation(_AnimationDefault = new SpriteAnimation(Texture, 100, true, false, "f0", "f1"));
                Lifespan = 2.0;
                _Color = new Color4(255, 24, 127, 255);
                _Color2 = new Color4(0.0f, (float)ColorRandy.NextDouble() * 0.5f + 0.5f, (float)ColorRandy.NextDouble() * 0.5f + 0.5f, 0.0f);
                Timer.Restart();
            }
            public override void Update (double time)
            {
                base.Update(time);
                _Scale.X = _Scale.Y = 2.0 * Timer.Elapsed.TotalSeconds + 1.0;//+= LastFrameIndex != 0 && FrameIndex == 0 ? 0.25 : 0.0;
                LastFrameIndex = FrameIndex;
                if (Timer.Elapsed.TotalSeconds > Lifespan)
                {
                    _RenderSet.Remove (this);
                    Dispose ();
                } else {
                    float fx = (float)((Lifespan - Timer.Elapsed.TotalSeconds) / Lifespan);
                    float gfx = (float)Math.Pow(fx, 0.5);
                    float afx = (float)Math.Pow(fx, 2.0);
                    _Color.A = afx;
                    _Color.R = 0.5f * fx * _Color2.R;
                    _Color.G = 0.7f * gfx * _Color2.G;
                }
            }
        }
	}
}

