using System;

namespace positron
{
	public class SceneBasicBox : Scene
	{
		protected override void Initialize()
		{
			base.Initialize();

			// Basic perimeter:
			int perim_x = 18;
			int perim_y = 8;
			double x0 = -64;
			double y0 = 0;
			for (int i = 0; i < perim_x; i++)
			{
				var block = new BunkerFloor(this, x0 + 32 * i, y0);
				block = new BunkerFloor2(this, x0 + 32 * i, y0 - 128);
			}
			for (int i = 0; i <= perim_y; i++)
			{
				var block = new BunkerWall(this, x0 + 32 * perim_x, y0 + 32 * i);
				block.TileX = -1.0;
				block = new BunkerWall(this, x0 - 16, y0 + 32 * (perim_y - i));

			}
			for (int i = 0; i < perim_x; i++)
			{
				var block = new SpriteObject(Stage, x0 + 32 * (perim_x - i - 1), y0 + 32 * perim_y, Texture.Get("sprite_tile_floor_atlas"));
			}
		}
	}
}

