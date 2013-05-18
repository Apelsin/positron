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
            Body.OnCollision += HandleCollision;
		}
        bool HandleCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
        {
            WeakReference scene_wr = new WeakReference(Set.Scene);
            _RenderSet.Scene.Game.AddUpdateEventHandler(this, (sender, e) => {
                new BulletCollisionParticle((Scene)scene_wr.Target, PositionX, PositionY).CenterShift();
                Derez();
                return true;
            });
            return true;
        }
		protected void InitPhysicsLate()
		{
			Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(
				Body.LinearVelocity.X + (float)_Velocity.X / (float)Configuration.MeterInPixels,
				Body.LinearVelocity.Y + (float)_Velocity.Y / (float)Configuration.MeterInPixels);
		}
		public override void Update(double time)
		{
			//if(Body != null)
			//	Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, 0.1f);
			Theta = Math.Atan2(VelocityY * _TileX, VelocityX * _TileX);
			_TileX = VelocityX > 0.0 ? 1.0 : -1.0;
			Body.ApplyForce(-_RenderSet.Scene.World.Gravity * Body.Mass);
		}
	}
    public class BulletCollisionParticle : SpriteBase
    {
        SpriteAnimation Hit;
        public BulletCollisionParticle (Scene scene, double x, double y):
            base(scene.Stage, x, y, Texture.Get("sprite_bullet_collision_particle"))
        {
            Hit = new SpriteAnimation(Texture, 10, false, "f1", "f2", "f3", "f4");
            Hit.Sound = Sound.Get ("sfx_bullet_impact");
            WeakReference game_wr = new WeakReference(Set.Scene.Game);
            WeakReference render_set_wr = new WeakReference(Set);
            _AnimationNext = new Lazy<SpriteAnimation>(() => { 
                ((PositronGame)game_wr.Target).AddUpdateEventHandler(this, (sender, e) =>
                {
                    ((RenderSet)render_set_wr.Target).Remove (this);
                    this.Dispose();
                    return true;
                });
                return null;
            });
            PlayAnimation(Hit);
        }
    }
}

