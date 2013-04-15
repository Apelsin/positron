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
	public class SceneTwo : SceneBasicBox
	{
//		protected SceneTwo ():
//			base()
//		{
//		}
		protected override void Initialize ()
		{
			base.Initialize();

			// Store width and height in local variables
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;

			// Setup background tiles
			var BackgroundTiles = new TileMap(Background, 48, 24, Texture.Get("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = -320;
			BackgroundTiles.PositionY = -256;
			BackgroundTiles.PositionZ = 2.0;
			BackgroundTiles.RandomMap();
			BackgroundTiles.Build();

			Scene prev_scene = (Scene)Scene.Scenes["SceneOne"];
			_DoorToPreviousScene = new Door(Rear, 512 - 68, 32, prev_scene);

			// Get cross-scene variables
			Scene previous_scene = (Scene)Scene.Scenes["SceneOne"];
			Player player_1 = Program.MainGame.Player1;

			for (int i = 0; i < 3; i++) {
				var spidey = new Spidey(Stage, 10 * i - 100, 20);
				spidey.Position += DoorToPreviousScene.Position;
				spidey.Body.BodyType = BodyType.Dynamic;
			}

		}
	}
}

