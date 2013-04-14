using System;

namespace positron
{
	public class BunkerFloor : SpriteObject, IVariant
	{
		protected int _Variant = 0;
		public int Variant {
			get { return _Variant; }
			set { _Variant = value; }
		}
		public BunkerFloor (Scene scene, double x, double y):
			base(scene.Stage, x, y, Texture.Get("sprite_bunker_floor"))
		{
		}
		protected override void Draw()
		{
			//TODO: Update variant
			base.Draw();
		}
	}
}