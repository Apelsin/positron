using System;

namespace positron
{
	public class PsdSpriteTest : SpriteObject
	{
		public PsdSpriteTest (RenderSet render_set, double x, double y):
			base(render_set, x, y, Texture.Get("sprite_dumbo"))
		{
			_AnimationDefault = new SpriteAnimation(Texture, 1);
			PlayAnimation(AnimationDefault);
		}
		protected override void InitPhysics()
		{
			base.InitPhysics();
			Body.BodyType = FarseerPhysics.Dynamics.BodyType.Dynamic;
		}
	}
}

