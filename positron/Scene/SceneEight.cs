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
	public class SceneEight : SceneBasicBox
	{
		protected SceneEight ():
			base()
		{
		}
		protected override void InstantiateConnections()
		{
			_DoorToPreviousScene = new Door(Rear, 0, 0);
			//_DoorToNextScene = new Door(Rear, _DoorToPreviousScene.CornerX + 8 * TileSize, 3 * TileSize);
			_DoorToNextScene = new Door(Rear, _DoorToPreviousScene.CornerX + 3 * TileSize, _DoorToPreviousScene.CornerY);
		}
		protected override void InitializeScene ()
		{
			// Assign base class variables here, before calling the base class initializer
			PerimeterOffsetX = 126;
			PerimeterOffsetY = -92;
			PerimeterX = 30;
			PerimeterY = 20;
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;
			
			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 24) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY + 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();

			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes ["SceneSeven"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;

			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			xp = x0 + TileSize * 0;
			yp = y0 + TileSize * 0;

			var i = 0;
			for (i = 1; i < PerimeterY - 4; i++) {
				new ExtenderPlatform (Stage, xp + TileSize * (0), yp + TileSize * i, true);
			}

			for (i = 2; i < 17; i++) {
				new FloorTile (Rear, xp + TileSize * (i), yp + TileSize * (3.5));
			}

			new FloorTile (Rear, xp + TileSize * (18), yp + TileSize * (3.5));
			ExtenderPlatform ep_save = new ExtenderPlatform (Stage, xp + TileSize * (17), yp + TileSize * 1, false);
			new ExtenderPlatform (Stage, xp + TileSize * (17), yp + TileSize * 2, ep_save);
			new ExtenderPlatform (Stage, xp + TileSize * (17), yp + TileSize * 3, ep_save);

			for (i = 21; i < 24; i++) {
				new FloorTile (Rear, xp + TileSize * (i), yp + TileSize * (3.5));
			}

			for (i = 26; i < PerimeterX; i++) {
				new FloorTile (Rear, xp + TileSize * (i), yp + TileSize * (3.5));
			}

			for (i = 4; i < 15; i++) {
				new FloorTile (Rear, xp + TileSize * (1), yp + TileSize * (i));
			}

			new FloorTile (Rear, xp + TileSize * (1), yp + TileSize * (16));
			new FloorTile (Rear, xp + TileSize * (2), yp + TileSize * (16));
			new FloorTile (Rear, xp + TileSize * (3), yp + TileSize * (16));
			new FloorTile (Rear, xp + TileSize * (5), yp + TileSize * (16));
			new FloorTile (Rear, xp + TileSize * (6), yp + TileSize * (16));
			new FloorTile (Rear, xp + TileSize * (6), yp + TileSize * (17));
			new FloorTile (Rear, xp + TileSize * (6), yp + TileSize * (18));
			new FloorTile (Rear, xp + TileSize * (5), yp + TileSize * (15));
			new FloorTile (Rear, xp + TileSize * (5), yp + TileSize * (14));
			new FloorTile (Rear, xp + TileSize * (4), yp + TileSize * (13.5));
			new FloorTile (Rear, xp + TileSize * (3), yp + TileSize * (13.5));


			for (i = 2; i < 15; i++) {
				new FloorTile (Rear, xp + TileSize * (i), yp + TileSize * (11));
			}

			ExtenderPlatform ep0 = new ExtenderPlatform (Stage, xp + TileSize * (7), yp + TileSize * 12, false);
			new ExtenderPlatform (Stage, xp + TileSize * (9), yp + TileSize * 13, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (11), yp + TileSize * 14, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (13), yp + TileSize * 15, ep0);

			var fs0 = new PressureSwitch (Front, xp + TileSize * (2), yp + TileSize * (12), (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep0.OnAction (e.Self, new ActionEventArgs (bstate, ep0));
			}, 5.0);

			for (i = 11; i < 16; i++) {
				new FloorTile (Rear, xp + TileSize * (15), yp + TileSize * (i));
			}

			new FloorTile (Rear, xp + TileSize * (16), yp + TileSize * (15));
			new FloorTile (Rear, xp + TileSize * (17), yp + TileSize * (15));
			new FloorTile (Rear, xp + TileSize * (18), yp + TileSize * (15));

			new FloorTile (Rear, xp + TileSize * (21), yp + TileSize * (15));
			new FloorTile (Rear, xp + TileSize * (22), yp + TileSize * (15));
			new FloorTile (Rear, xp + TileSize * (23), yp + TileSize * (15));

			new FloorTile (Rear, xp + TileSize * (26), yp + TileSize * (12.5));
			new FloorTile (Rear, xp + TileSize * (27), yp + TileSize * (12.5));
			new FloorTile (Rear, xp + TileSize * (28), yp + TileSize * (12.5));
			new FloorTile (Rear, xp + TileSize * (29), yp + TileSize * (12.5));

			ExtenderPlatform ep1 = new ExtenderPlatform (Stage, xp + TileSize * (24), yp + TileSize * 14, false);
			new ExtenderPlatform (Stage, xp + TileSize * (25), yp + TileSize * 13, ep1);

			new ExtenderPlatform (Stage, xp + TileSize * (18), yp + TileSize * 8.5, ep1);
			new ExtenderPlatform (Stage, xp + TileSize * (19), yp + TileSize * 8.5, ep1);
			new ExtenderPlatform (Stage, xp + TileSize * (20), yp + TileSize * 8.5, ep1);

			var gw1 = new Gateway (Front, xp + TileSize * 16, yp + TileSize * (9.75), true);

			ProjectileSwitch ps0 = new ProjectileSwitch (Front, x0 + TileSize * 15.6, yp + TileSize * 14, (sender, e) =>
			                                             {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep1.OnAction (e.Self, new ActionEventArgs (bstate, ep1));
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			}, 8.0);
			ps0.Theta += MathHelper.Pi;

			for (i = 9; i < 18; i++) {
				new ExtenderPlatform (Rear, xp + TileSize * (i), yp + TileSize * (8.5), true);
			}

			var gw2 = new Gateway (Front, xp + TileSize * 18, yp + TileSize * (4.5), true);
			new FloorTile (Rear, xp + TileSize * (18), yp + TileSize * (7.5));

			for (i = 7; i < 14; i++) {
				for (var j = 21; j <= 23; j++) {
					new FloorTile (Rear, xp + TileSize * (j), yp + TileSize * (i + 0.25));
				}
			}

			i = 5;
			ExtenderPlatform ep2 = new ExtenderPlatform (Stage, xp + TileSize * (i), yp + TileSize * 5.0, false);
			new ExtenderPlatform (Stage, xp + TileSize * (i + 3), yp + TileSize * 5.5, ep2);
			new ExtenderPlatform (Stage, xp + TileSize * (i + 6), yp + TileSize * 5.5, ep2);
			new ExtenderPlatform (Stage, xp + TileSize * (i + 9), yp + TileSize * 5.5, ep2);

			var fs1 = new PressureSwitch (Front, xp + TileSize * (3), yp + TileSize * (4.5), (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep2.OnAction (e.Self, new ActionEventArgs (bstate, ep2));
			}, 4.0);

			new FloorTile (Rear, xp + TileSize * (15), yp + TileSize * (4.5));
			new FloorTile (Rear, xp + TileSize * (15), yp + TileSize * (5.5));

			var gw3 = new Gateway (Front, xp + TileSize * 21, yp + TileSize * (4.5), true);
			var gw4 = new Gateway (Front, xp + TileSize * 23, yp + TileSize * (4.5), true);

			for (i = 7; i < 12; i++) {
				new FloorTile (Rear, xp + TileSize * (26), yp + TileSize * (i + 0.5));
			}

			var gw5 = new Gateway (Front, xp + TileSize * 26, yp + TileSize * (4.5), true);

			var fs2 = new PressureSwitch (Front, xp + TileSize * (16), yp + TileSize * (4.5), (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep_save.OnAction (e.Self, new ActionEventArgs (bstate, ep_save));
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
				gw3.OnAction (e.Self, new ActionEventArgs (bstate, gw3));
				gw4.OnAction (e.Self, new ActionEventArgs (bstate, gw4));
				gw5.OnAction (e.Self, new ActionEventArgs (bstate, gw5));
			}, 15.0);

			var ft_door = new FloorTile (Rear, xp + TileSize * 28, yp + 4 * TileSize);

			/*
			Scene next_scene = (Scene)Program.MainGame.Scenes["SceneNine"];
			_DoorToNextScene.CornerX = ft_door.CornerX;
			_DoorToNextScene.CornerY = ft_door.CornerY + TileSize;
			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
			_DoorToNextScene.Destination.Corner += _DoorToNextScene.Corner;
			*/

			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

