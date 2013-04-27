using System;

namespace positron
{
	public class FloorTile : SpriteObject, IVariant
	{
		protected static Random Variance = new Random(57245);
		protected int _Variant = 0;
		public int Variant {
			get { return _Variant; }
			set { _Variant = value; }
		}
		public FloorTile (RenderSet render_set, double x, double y):
			base(render_set, x, y, Texture.Get("sprite_tile_floor_atlas"))
		{
			_Variant = Variance.Next(Texture.Regions.Length);
			PlayAnimation(new SpriteAnimation(Texture, _Variant));
		}
		protected override void Draw()
		{
			//TODO: Update variant
			base.Draw();
		}
	}
}