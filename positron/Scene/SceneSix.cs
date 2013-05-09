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
	public class SceneSix : SceneBasicBox
	{
        protected SceneSix (PositronGame game):
			base(game)
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
			PerimeterOffsetX = 102;
			PerimeterOffsetY = -100;
			PerimeterX = 30;
			PerimeterY = 10;
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;

			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 24) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();

			// Set up previous door:
            Scene prev_scene = (Scene)_Game.Scenes["SceneFive"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;

			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			xp = x0 + TileSize * 0;
			yp = y0 + TileSize * 0;

			new FloorTile (Rear, xp + TileSize * (6 - 1), yp + TileSize * 1);
			new FloorTile (Rear, xp + TileSize * 6, yp + TileSize * 1);
			new FloorTile (Rear, xp + TileSize * 6, yp + TileSize * 2);
			new FloorTile (Rear, xp + TileSize * 6, yp + TileSize * 3);
			new FloorTile (Rear, xp + TileSize * (6 + 1), yp + TileSize * 1);
			new FloorTile (Rear, xp + TileSize * (6 + 1), yp + TileSize * 2);
			new FloorTile (Rear, xp + TileSize * (6 + 2), yp + TileSize * 1);

			//new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 1, false);
			ExtenderPlatform ep0 = new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 2, false);
			new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 3, ep0);
			//new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 4, ep0);

			var fs0 = new PressureSwitch (Front, xp + TileSize * 5, yp + TileSize * 2, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep0.OnAction (e.Self, new ActionEventArgs (bstate, ep0));
			}, 3.0);



			ExtenderPlatform ep1 = new ExtenderPlatform (Stage, xp + TileSize * (8), yp + TileSize * 3, false);
			new ExtenderPlatform (Stage, xp + TileSize * (9), yp + TileSize * 3, ep1);
			new ExtenderPlatform (Stage, xp + TileSize * (12), yp + TileSize * 3, ep1);
			new ExtenderPlatform (Stage, xp + TileSize * (13), yp + TileSize * 3, ep1);
			ExtenderPlatform ep2 = new ExtenderPlatform (Stage, xp + TileSize * (16), yp + TileSize * 4, false);
			new ExtenderPlatform (Stage, xp + TileSize * (17), yp + TileSize * 4, ep2);
			//new ExtenderPlatform (Stage, xp + TileSize * (20), yp + TileSize * 5, ep2);
			ExtenderPlatform ep3 = new ExtenderPlatform (Stage, xp + TileSize * (21), yp + TileSize * 4.5, false);
			new ExtenderPlatform (Stage, xp + TileSize * (25), yp + TileSize * 4, ep3);
			new ExtenderPlatform (Stage, xp + TileSize * (26), yp + TileSize * 4, ep3);

			var fs1 = new PressureSwitch (Front, xp + TileSize * 6, yp + TileSize * 4, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep1.OnAction (e.Self, new ActionEventArgs (bstate, ep1));
			}, 2.0);

			var fs2 = new PressureSwitch (Front, xp + TileSize * 6, yp + TileSize * 4, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep2.OnAction (e.Self, new ActionEventArgs (bstate, ep2));
			}, 3.0);

			var fs3 = new PressureSwitch (Front, xp + TileSize * 6, yp + TileSize * 4, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep3.OnAction (e.Self, new ActionEventArgs (bstate, ep3));
			}, 4.5);

			//new FloorTile (Rear, xp + TileSize * 25, yp + 6 * TileSize);
			//new FloorTile (Rear, xp + TileSize * 26, yp + 6 * TileSize);
			new FloorTile (Rear, xp + TileSize * 27, yp + 4 * TileSize);
			var ft_door = new FloorTile (Rear, xp + TileSize * 28, yp + 4 * TileSize);
			new FloorTile (Rear, xp + TileSize * 29, yp + 4 * TileSize);

            Scene next_scene = (Scene)_Game.Scenes["SceneSeven"];
			_DoorToNextScene.CornerX = ft_door.CornerX;
			_DoorToNextScene.CornerY = ft_door.CornerY + TileSize;
			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
			_DoorToNextScene.Destination.Corner += _DoorToNextScene.Corner;

			
			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

