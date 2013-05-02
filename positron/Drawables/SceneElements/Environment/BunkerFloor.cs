using System;

namespace positron
{
	public class BunkerFloor : SpriteObject, IVariant
	{
		protected static Random Variance = new Random(78357);
		protected int _Variant = 0;
		public int Variant {
			get { return _Variant; }
			set { _Variant = value; }
		}
		public BunkerFloor (Scene scene, double x, double y):
			this(scene, x, y, Texture.Get("sprite_bunker_floor"))
		{
		}
		protected BunkerFloor (Scene scene, double x, double y, Texture texture):
			base(scene.Stage, x, y, texture)
		{
			_Variant = Variance.Next(Texture.Regions.Length);
			PlayAnimation(_AnimationDefault = new SpriteAnimation(Texture, _Variant));
		}
		protected override void Draw()
		{
			//TODO: Update variant
			base.Draw();
		}
	}
	public class BunkerFloor2 : BunkerFloor
	{
		protected static Random Variance = new Random(56245);
		public BunkerFloor2 (Scene scene, double x, double y):
			base(scene, x, y, Texture.Get("sprite_bunker_floor_2"))
		{
		}
	}
}