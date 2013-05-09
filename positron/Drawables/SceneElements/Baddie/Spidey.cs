using System;
using System.Diagnostics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace positron
{
	public class Spidey : SpriteObject
	{
		protected double JumpInterval = 1.0;
		protected double JumpIntervalVariance = 0.3;
		protected double JumpIntervalVaried;
		protected float JumpVeloVariance = 0.2f;
		protected Random R = new Random();
		protected Stopwatch JumpTimer = new Stopwatch();
		public Spidey (RenderSet render_set, double x, double y):
			base(render_set, x, y, Texture.Get ("sprite_spidey_0"))
		{
			this.Body.OnCollision += HandleOnCollision;
			JumpTimer.Start();
		}

		bool HandleOnCollision (Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			Player p = _RenderSet.Scene.Game.Player1;
			object world_object = fixtureB.Body.UserData;
			bool collided_with_player = p != null && fixtureB.Body == p.Body;
			bool collided_with_bullet = world_object is BasicBullet;
			if(collided_with_player || collided_with_bullet)
			{
				if(fixtureB.Body.LinearVelocity.Length() > 2.0f)
					Derez ();
			}
			if (collided_with_player)
			{
				if(p.Body.LinearVelocity.Length() < 0.2f)
				{
					p.OnHealthChanged(this, p.Health - 1);
				}
			}
			return true;
		}
		public override void Update (double time)
		{
			// Basic movement behavior:
			Player p = _RenderSet.Scene.Game.Player1;
			if (p != null)
			{
				if(JumpTimer.Elapsed.TotalSeconds >= JumpIntervalVaried)
				{
					float randy = 1.0f + JumpVeloVariance * (float)R.NextDouble();
					float dx = (float)p.PositionWorldX - (float)PositionWorldX;
					dx = 0.5f * dx / Math.Max(1.0f, dx);
					var velocity = new Microsoft.Xna.Framework.Vector2(dx, 3.0f) * Body.Mass * randy;
					Body.ApplyLinearImpulse(velocity);
					JumpIntervalVaried = JumpInterval + R.NextDouble() * JumpIntervalVariance;
					JumpTimer.Restart();
				}
			}
		}
	}
}

