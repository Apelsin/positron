using System;

namespace positron
{
	public class SceneBasicBox : Scene, ISceneGameplay
	{
		protected int PerimeterOffsetX = 0;
		protected int PerimeterOffsetY = 0;
		protected int PerimeterX = 18;
		protected int PerimeterY = 8;
		protected double TileSize = 32;
        protected SceneBasicBox(PositronGame game): base(game) {}
		public override void InitializeScene()
		{
			base.InitializeScene();

			// Basic perimeter:
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;
			for (int i = 0; i < PerimeterX; i++)
			{
                BunkerFloor block = new BunkerFloor2 (this, x0 + TileSize * i, y0);
                block.PositionY -= block.SizeY;
                block = new BunkerFloor (this, x0 + TileSize * i, y0);
			}
			for (int i = 0; i <= PerimeterY; i++)
			{
				var block = new BunkerWall(this, x0 + TileSize * PerimeterX, y0 + TileSize * i);
				block.TileX = -1.0;
				block = new BunkerWall(this, x0 - 0.5 * TileSize, y0 + TileSize * (PerimeterY - i));
			}
			for (int i = 0; i < PerimeterX; i++)
			{
				var block = new FloorTile(Stage, x0 + TileSize * (PerimeterX - i - 1), y0 + TileSize * PerimeterY);
			}
		}
	}
}

