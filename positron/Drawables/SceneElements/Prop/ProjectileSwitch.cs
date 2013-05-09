using System;
using System.Diagnostics;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace positron
{
	public class ProjectileSwitch : PressureSwitch
	{
		public ProjectileSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, double latch_time = double.PositiveInfinity):
			this(render_set, x, y, state_changed, new SharedState<SwitchState>(SwitchState.Open), new SharedState<double>(latch_time), new Stopwatch(), latch_time)
		{
		}
		public ProjectileSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, PressureSwitch sync_switch):
            this(render_set, x, y, state_changed, sync_switch, sync_switch.LatchTime)
		{
		}
		public ProjectileSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, PressureSwitch sync_switch, double latch_time):
            this(render_set, x, y, state_changed, new SharedState<SwitchState>(SwitchState.Open), new SharedState<double>(latch_time), new Stopwatch(), latch_time)
		{
		}
        protected ProjectileSwitch(RenderSet render_set, double x, double y,
                               ActionEventHandler state_changed,
                               SharedState<SwitchState> state,
                               SharedState<double> latch_expiration,
                               Stopwatch latch_timer, double latch_time) :
            base(render_set, x, y, state_changed, state, latch_expiration, latch_timer, latch_time, Texture.Get("sprite_projectile_switch"))
        {
            Theta = 0.0;
        }
        public override double Theta {
            get { return base.Theta - MathHelper.PiOver2; }
            set { base.Theta = value + MathHelper.PiOver2; }
        }
		protected override void InitPhysics ()
		{
			float w, h;
			var size = Texture.Regions [0].Size;
			w = (float)(_Scale.X * size.X / Configuration.MeterInPixels);
			h = (float)(_Scale.Y * size.Y / Configuration.MeterInPixels);
			h *= 0.5f; // Floor switch
			HalfWH = new Vector2 (w * 0.5f, h * 0.5f); // Please kill me
			var half_w_h = new Microsoft.Xna.Framework.Vector2(HalfWH.X, HalfWH.Y);
			var msv2 = new Microsoft.Xna.Framework.Vector2 (
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody (_RenderSet.Scene.World, msv2);
			FixtureFactory.AttachRectangle (w, h, 100.0f, new Microsoft.Xna.Framework.Vector2(0.0f, -half_w_h.Y), _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.5f;
			_SpriteBody.OnCollision += HandleOnCollision;
			_SpriteBody.OnSeparation += HandleOnSeparation;

			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = this.RenderSet.Scene == RenderSet.Scene.Game.CurrentScene;

			InitBlueprints ();
		}

		protected override bool HandleOnCollision (Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			lock (Body) {
				object sender = fixtureB.Body.UserData;
				if (sender is BasicBullet) {
					//if(!_LastAffected)
					{
						//_LastAffected = true;
                        OnAction (sender, SwitchState.Closed);
						OnAction (sender, SwitchState.Latched); // HACK
					}
				}
			}
			return true;
		}
		protected override void HandleOnSeparation (Fixture fixtureA, Fixture fixtureB)
		{
			//lock (Body) {
			//	object sender = fixtureB.Body.UserData;
			//	if (sender is BasicBullet) {
			//		if (_LastAffected)
			//		{
			//			_LastAffected = false;
			//			OnAction (fixtureB.Body.UserData, SwitchState.Latched);
			//		}
			//	}
			//}
		}
	}
}

