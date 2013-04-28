using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

using OpenTK;
using OpenTK.Input;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace positron
{
	public class BasicBullet : SpriteObject
	{
		public BasicBullet (Scene scene, double x, double y, double vx, double vy):
			base(scene.Stage, x, y, Texture.Get("sprite_first_bullet"))
		{
			_Velocity.X = vx;
			_Velocity.Y = vy;
			InitPhysicsLate();
		}
		protected override void InitPhysics()
		{
			base.InitPhysics();
			Body.BodyType = BodyType.Dynamic;
			Body.IsBullet = true;
			Body.OnCollision += (Fixture fixtureA, Fixture fixtureB, Contact contact) =>
			{
                Program.MainGame.AddUpdateEventHandler(this, (sender, e) =>
                {
                    Derez();
					return true;
                });
				return true;
			};
		}
		protected void InitPhysicsLate()
		{
			Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(
				Body.LinearVelocity.X + (float)_Velocity.X / (float)Configuration.MeterInPixels,
				Body.LinearVelocity.Y + (float)_Velocity.Y / (float)Configuration.MeterInPixels);
		}
		public override void Update(double time)
		{
			if(Body != null)
				Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, 0.1f);
			_TileX = VelocityX > 0.0 ? 1.0 : -1.0;
			//Body.ApplyForce(_RenderSet.Scene.World.Gravity * -0.5f);
		}
	}
}

