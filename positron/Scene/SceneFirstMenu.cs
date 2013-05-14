using System;
using System.Collections.Generic;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Collision;

namespace positron
{
	public class SceneFirstMenu : Scene
	{
		protected UIElementGroup UIGroup;
        protected SceneFirstMenu (PositronGame game):
            base(game)
		{
			UIGroup = new UIElementGroup();
			this.SceneEntry += (sender, e) => {
                _Game.SetInputAccepters("Main Menu",  new IInputAccepter[] { UIGroup });
			};
			SceneExit += (sender, e) => {
                _Game.RemoveInputAccepters("Main Menu");
			};
			SetupPlayerOnExit();
		}
		public override void InitializeScene ()
		{
			base.InitializeScene();
			var texture = Texture.Get ("sprite_main_menu_buttons");
			double x_center = (ViewWidth) * 0.5;
			double y_center = (ViewHeight) * 0.5;
			new SpriteBase(HUD, x_center, y_center + 128, Texture.Get("sprite_android_now")).CenterShift();
			((UIButton) new UIButton(this, x_center, y_center,
			             new SpriteBase.SpriteFrame(texture, 4),
			             new SpriteBase.SpriteFrame(texture, 2), UIGroup).CenterShift()).Action += (sender, e) =>
			{
                _Game.CurrentScene = (Scene)_Game.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
			((UIButton) new UIButton(this, x_center, y_center - 48,
			             new SpriteBase.SpriteFrame(texture, 5),
			                         new SpriteBase.SpriteFrame(texture, 3), UIGroup).CenterShift()).Action += (sender, e) =>
			{
                _Game.CurrentScene = (Scene)_Game.Scenes["SceneCredits"];
			};

            var looper = new MusicLooper_LessHackish(this, "last_human_loop");
            looper.Preserve = true;
            looper.RenderSetTransfer += (sender, e) => {
                _Game.AddUpdateEventHandler(this, (sender1, e1)=> {
                    if(!(e.From.Scene is ISceneGameplay) && (e.To.Scene is ISceneGameplay))
                    {
                        looper.SetLoop("induction_loop");
                    }
                    else if((e.From.Scene is ISceneGameplay) && !(e.To.Scene is ISceneGameplay))
                    {
                        looper.SetLoop("last_human_loop");
                    }
                    return true;
                });
            };
            new StuffOne(Background, 0.5 * ViewWidth, 0, -100, (int)ViewWidth / 20 + 1, (int)ViewHeight / 30 + 1);
            new StuffTwo(Background, 0.5 * ViewWidth, 0, -200, (int)ViewWidth / 20 + 1, (int)ViewHeight / 30 + 1);
		}
        protected class StuffOne : Drawable
        {
            protected Stopwatch StuffTimer = new Stopwatch();
            protected Texture _Texture;
            protected int _Wide, _High;
            public StuffOne(RenderSet render_set, double x, double y, double z, int wide, int high):
                this(render_set, x, y, z, wide, high, Texture.Get("sprite_small_disc"))
            {
            }
            protected StuffOne(RenderSet render_set, double x, double y, double z, int wide, int high, Texture texture):
                base(render_set)
            {
                _Wide = wide;
                _High = high;
                _Position.X = x;
                _Position.Y = y;
                _Position.Z = z;
                _Texture = texture;
                StuffTimer.Start();
            }
            public override void Render (double time)
            {
                Draw ();
            }
            protected virtual void Draw()
            {
                double t = StuffTimer.Elapsed.TotalSeconds * 0.2;
                _Texture.Bind();
                GL.PushMatrix();

                double rotate = 40 * t;

                GL.Translate (_Position.X, _Position.Y, _Position.Z);
                //GL.Rotate(rotate, 0.0, 1.0, 0.0);
                GL.Translate (-20 * (_Wide/2), 0.0, 0.0);

                for (int i = 0; i < _Wide; i++)
                {

                    GL.Translate(20.0, 0.0, 0.0);
                    GL.PushMatrix();
                    //GL.Rotate(-rotate, 0.0, 1.0, 0.0);
                    double f = Math.Sin (t + i * 0.25);
                    for(int j = 0; j < _High; j++)
                    {
                        GL.Translate(0.0, 20.0, 0.0);
                        double g = Math.Sin (t + j * 0.5);
                        double h = f + g;
                        double r = f * g;
                        double a = (h * r + 1) / 2;

                        GL.Rotate(r, 0.0, 0.0, 1.0);
                        GL.Translate(5 * g, 5 * f, 50 * r * h);
                        GL.Scale(h, h, h);
                        GL.Color4 (0.2, 0.4, 1.0, a);
                        GL.Rotate(h, 0.0, 1.0, 0);
                        GL.Begin (BeginMode.Quads);
                        {
                            GL.Vertex3(0, 0, 0); GL.TexCoord2(0, 0);
                            GL.Vertex3(10, 0, 0); GL.TexCoord2(1, 0);
                            GL.Vertex3(10, 10, 0); GL.TexCoord2(1, 1);
                            GL.Vertex3(0, 10, 0); GL.TexCoord2(0, 1);
                        }
                        GL.End ();
                    }
                    GL.PopMatrix();
                }
                GL.PopMatrix();
            }
        }
        protected class StuffTwo : StuffOne
        {
            public StuffTwo(RenderSet render_set, double x, double y, double z, int wide, int high):
                base(render_set, x, y, z, wide, high)
            {
            }
            protected override void Draw()
            {
                double t = StuffTimer.Elapsed.TotalSeconds * 0.3;
                _Texture.Bind();
                GL.PushMatrix();

                double rotate = -30 * t;

                GL.Translate (_Position.X, _Position.Y, _Position.Z);
                GL.Rotate(rotate, 0.0, 1.0, 0.0);
                GL.Translate (-20 * (_Wide/2), 0.0, 0.0);

                for (int i = 0; i < _Wide; i++)
                {

                    GL.Translate(20.0, 0.0, 0.0);
                    GL.PushMatrix();
                    GL.Rotate(-rotate, 0.0, 1.0, 0.0);
                    double f = Math.Sin (t + i * 0.5);
                    for(int j = 0; j < _High; j++)
                    {
                        GL.Translate(0.0, 20.0, 0.0);
                        double g = Math.Sin (t + j * 0.25);
                        double h = 0.5 * (f - g) + 1;
                        double r = f * g;
                        double a = (g * h + 1) / 2;
                        double b = 1.0 - h;
                        double c = 2 * (b * (1 - b) + 2);

                        GL.Rotate(r, h, 0.0, 1.0);
                        GL.Translate(7 * g, 7 * f, 70 * c);
                        GL.Scale(h, h, h);
                        GL.Color4 (1.0, 0.2 * (b + c), 0.2, c);
                        GL.Rotate(b, 1.0, 0.0, 0);
                        GL.Begin (BeginMode.Quads);
                        {
                            GL.Vertex3(0, 0, 0); GL.TexCoord2(0, 0);
                            GL.Vertex3(10, 0, 0); GL.TexCoord2(1, 0);
                            GL.Vertex3(10, 10, 0); GL.TexCoord2(1, 1);
                            GL.Vertex3(0, 10, 0); GL.TexCoord2(0, 1);
                        }
                        GL.End ();
                    }
                    GL.PopMatrix();
                }
                GL.PopMatrix();
            }
        }
	}
}

