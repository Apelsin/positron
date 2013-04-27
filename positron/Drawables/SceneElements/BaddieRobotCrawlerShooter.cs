using System;

namespace positron
{
	public class BaddieRobotCrawlerShooter : SpriteObject, IShooter
	{
		public event FireEventHandler Fire;
		protected SpriteAnimation
			AnimationIdle,
			AnimationCrawl,
			AnimationArm,
			AnimationArmed,
			AnimationDisarm,
			AnimationFiring;
		public BaddieRobotCrawlerShooter (Scene scene, double x, double y)
			: base(scene.Stage, x, y, Texture.Get("sprite_robot_1"))
		{
			AnimationIdle = new SpriteAnimation(Texture, 200, true, false, new string[] { "stationary" });
			AnimationCrawl = new SpriteAnimation(Texture, 200, true, false, new string[] { "crawl_0", "crawl_1", "crawl_2", "crawl_3" });
			AnimationArm = new SpriteAnimation(Texture, 200, false, false, new string[] { "arm_0", "arm_1", "arm_2" });
			AnimationArmed = new SpriteAnimation(Texture, 200, true, false, new string[] { "armed" });
			AnimationDisarm = new SpriteAnimation(Texture, 200, false, false, new string[] { "disarm_0", "disarm_1", "disarm_2" });
			AnimationFiring = new SpriteAnimation(Texture, 75, true, false, new string[] { "firing_0", "firing_1", "firing_2", "firing_3", "firing_4" });
			StartAnimation(AnimationFiring);
		}
		protected override void InitPhysics()
		{
			base.InitPhysics();
			_SpriteBody.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
		}
		public void OnFire(object sender, FireEventArgs e)
		{
		}
	}
}

