using System;

namespace positron
{
	public class BunkerWall : SpriteObject, IVariant
	{
		protected static Random Variance = new Random(2944);
		protected int _Variant = 0;
		protected int _Direction = 0;
		public int Variant {
			get { return _Variant; }
			set { _Variant = value; }
		}
		public int Direction {
			get { return _Direction; }
			set { _Direction = value; _TileX = _Direction == 0 ? 1.0 : -1.0; }
		}
		public BunkerWall (Scene scene, double x, double y):
			base(scene.Front, x, y, Texture.Get("sprite_bunker_wall"))
		{
			_Variant = Variance.Next(Texture.Regions.Length);
			PlayAnimation(new SpriteAnimation(Texture, _Variant));
		}
		public override void Update (double time)
		{
			base.Update(time);
		}
		protected override void Draw ()
		{
			//TODO: Update variant
			base.Draw();
		}
	}
}