using System;

namespace positron
{
	public class SceneBasicBox : Scene
	{
		protected int PerimeterOffsetX = 0;
		protected int PerimeterOffsetY = 0;
		protected int PerimeterX = 18;
		protected int PerimeterY = 8;
		protected double TileSize = 32;
		protected override void InitializeScene()
		{
			base.InitializeScene();

			// Basic perimeter:
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;
			for (int i = 0; i < PerimeterX; i++)
			{
				var block = new BunkerFloor(this, x0 + TileSize * i, y0);
				block = new BunkerFloor2(this, x0 + TileSize * i, y0 - 4 * TileSize);
			}
			for (int i = 0; i <= PerimeterY; i++)
			{
				var block = new BunkerWall(this, x0 + TileSize * PerimeterX, y0 + TileSize * i);
				block.TileX = -1.0;
				block = new BunkerWall(this, x0 - 0.5 * TileSize, y0 + TileSize * (PerimeterY - i));

			}
			for (int i = 0; i < PerimeterX; i++)
			{
				var block = new SpriteObject(Stage, x0 + TileSize * (PerimeterX - i - 1), y0 + TileSize * PerimeterY, Texture.Get("sprite_tile_floor_atlas"));
			}
		}
	}
}

