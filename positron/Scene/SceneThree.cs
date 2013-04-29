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
    public class SceneThree : SceneBasicBox
    {
		protected SceneThree ():
			base()
		{
			SceneEntry += (sender, e) => {
				var stanzas = new List<DialogStanza>();
				DialogSpeaker speaker = DialogSpeaker.Get("protagonist");
				stanzas.Add(new DialogStanza(speaker, "Holy cow, there's a robot thing in here!\nNEATO."));
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

			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			yp += TileSize;
			
			// Set up doors:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
//			Scene next_scene = (Scene)Scene.Scenes["SceneThree"];
//			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
//			_DoorToNextScene.Destination.Position = _DoorToNextScene.Position;

            // Setup background tiles
            var BackgroundTiles = new TileMap(Background, 48, 24, Texture.Get("sprite_tile_bg3_atlas"));
			BackgroundTiles.PositionX = xp - 10 * TileSize;
			BackgroundTiles.PositionY = yp - 4 * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();

            // Get cross-scene variables
			Scene previous_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
            Player player_1 = Program.MainGame.Player1;

            for (int i = 0; i < 3; i++)
            {
                var spidey = new Spidey(Stage, 10 * i - TileSize * 32, 20);
                spidey.Position += DoorToPreviousScene.Position;
                spidey.Body.BodyType = BodyType.Dynamic;
				xp = spidey.CornerX;
				yp = spidey.CornerY;
            }

			xp -= TileSize;

			new BaddieRobotCrawlerShooter(this, DoorToPreviousScene.CornerX, DoorToPreviousScene.CornerY);

			// Call the base class initializer
			base.InitializeScene();
        }
    }
}

