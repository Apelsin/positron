using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace positron
{
    public class SceneThree : Scene
    {
		protected int PerimeterOffsetX = 12;
		protected int PerimeterOffsetY = 6;
		protected int PerimeterX = 18;
		protected int PerimeterY = 8;
		protected double TileSize = 32;
		protected SceneThree ():
			base()
		{
			SceneEntry += (sender, e) => {
				var stanzas = new List<DialogStanza>();
				DialogSpeaker speaker = DialogSpeaker.Get("protagonist");
				stanzas.Add(new DialogStanza(speaker, "This room is eerily empty..."));
				var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
				dialog.Begin();
			};
		}
		protected override void InstantiateConnections()
		{
			_DoorToPreviousScene = new Door(Rear, 0, 0);
		}
        protected override void InitializeScene()
        {
			// Define these before calling the base class initializer
			PerimeterOffsetX = 12;
			PerimeterOffsetY = 6;

			int chute_x = 3;

			// Basic perimeter:
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;
			for (int i = 0; i < PerimeterX; i++)
			{
				if(i == PerimeterX - chute_x)
					continue;
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
				var block = new FloorTile(Stage, x0 + TileSize * (PerimeterX - i - 1), y0 + TileSize * PerimeterY);
			}

			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			yp += TileSize;
			xp += (PerimeterX / 2) * TileSize;
			
			// Set up doors:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
//			Scene next_scene = (Scene)Scene.Scenes["SceneThree"];
//			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
//			_DoorToNextScene.Destination.Position = _DoorToNextScene.Position;

			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 36, 16, Texture.Get ("sprite_tile_bg3_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 9) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();



            // Get cross-scene variables
			Scene previous_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
            Player player_1 = Program.MainGame.Player1;

			// Call the base class initializer
			base.InitializeScene();
        }
    }
}

