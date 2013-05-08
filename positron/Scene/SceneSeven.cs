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
	public class SceneSeven : SceneBasicBox
	{
		protected SceneSeven ():
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
			PerimeterOffsetX = 125;
			PerimeterOffsetY = -96;
			PerimeterX = 7;
			PerimeterY = 30;
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;
			
			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 24) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY + 10) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();
			
			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneSix"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			
			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			xp = x0 + TileSize * 0;
			yp = y0 + TileSize * 0;
			
			ExtenderPlatform ep0 = new ExtenderPlatform (Stage, xp + TileSize * (0), yp + TileSize * 1, false);
			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 1, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (3), yp + TileSize * 2, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 3, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (6), yp + TileSize * 3, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (3), yp + TileSize * 4, ep0);

			new ExtenderPlatform (Stage, xp + TileSize * (0), yp + TileSize * 3, true);
			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 3, true);

			new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 5, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (6), yp + TileSize * 5, ep0);
			new ExtenderPlatform (Stage, xp + TileSize * (3), yp + TileSize * 6, ep0);

			new FloorTile (Rear, xp + TileSize * (0), yp + TileSize * 7);
			new FloorTile (Rear, xp + TileSize * (0), yp + TileSize * 8);
			new FloorTile (Rear, xp + TileSize * (0), yp + TileSize * 9);
			new FloorTile (Rear, xp + TileSize * (0), yp + TileSize * 10);
			new FloorTile (Rear, xp + TileSize * (0), yp + TileSize * 11);
			new FloorTile (Rear, xp + TileSize * (1), yp + TileSize * 7);

			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 8, true);
			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 9, true);

			new FloorTile (Rear, xp + TileSize * (2), yp + TileSize * 10);
			new FloorTile (Rear, xp + TileSize * (3), yp + TileSize * 10);
			new FloorTile (Rear, xp + TileSize * (4), yp + TileSize * 10);
			new FloorTile (Rear, xp + TileSize * (5), yp + TileSize * 10);
			new FloorTile (Rear, xp + TileSize * (6), yp + TileSize * 10);

			var ps0 = new ProjectileSwitch (Front, xp + TileSize * (6.25), yp + TileSize * (1.25) , (sender, e) =>
			                                      {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep0.OnAction (e.Self, new ActionEventArgs (bstate, ep0));
			}, 2.5);

			ExtenderPlatform ep1 = new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 11, false);
			new ExtenderPlatform (Stage, xp + TileSize * (6), yp + TileSize * 11, ep1);

			var ps1 = new ProjectileSwitch (Front, xp + TileSize * (0.7), yp + TileSize * (11.5) , (sender, e) =>
			                                {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep1.OnAction (e.Self, new ActionEventArgs (bstate, ep1));
			}, 1.0);

			ExtenderPlatform ep2 = new ExtenderPlatform (Stage, xp + TileSize * (3), yp + TileSize * 12, false);
			new ExtenderPlatform (Stage, xp + TileSize * (0), yp + TileSize * 13, ep2);
			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 13, ep2);

			var ps2 = new ProjectileSwitch (Front, xp + TileSize * (0.7), yp + TileSize * (11.5) , (sender, e) =>
			                                {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep2.OnAction (e.Self, new ActionEventArgs (bstate, ep2));
			}, 2.5);

			ExtenderPlatform ep3 = new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 13, false);
			new ExtenderPlatform (Stage, xp + TileSize * (6), yp + TileSize * 13, ep3);

			var ps3 = new ProjectileSwitch (Front, xp + TileSize * (0.7), yp + TileSize * (11.5) , (sender, e) =>
			                                {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep3.OnAction (e.Self, new ActionEventArgs (bstate, ep3));
			}, 7.5);

			ExtenderPlatform ep4 = new ExtenderPlatform (Stage, xp + TileSize * (0), yp + TileSize * 15, false);
			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 15, ep4);
			new ExtenderPlatform (Stage, xp + TileSize * (3), yp + TileSize * 14, ep4);
			new ExtenderPlatform (Stage, xp + TileSize * (5), yp + TileSize * 15, ep4);
			new ExtenderPlatform (Stage, xp + TileSize * (6), yp + TileSize * 15, ep4);

			var ps4 = new ProjectileSwitch (Front, xp + TileSize * (0.7), yp + TileSize * (11.5) , (sender, e) =>
			                                {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep4.OnAction (e.Self, new ActionEventArgs (bstate, ep4));
			}, 2.0);

			ExtenderPlatform ep5 = new ExtenderPlatform (Stage, xp + TileSize * (3), yp + TileSize * 16, false);

			var ps5 = new ProjectileSwitch (Front, xp + TileSize * (0.7), yp + TileSize * (11.5) , (sender, e) =>
			                                {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				ep5.OnAction (e.Self, new ActionEventArgs (bstate, ep5));
			}, 3.0);

			ps1.Theta += MathHelper.Pi;
			ps2.Theta += MathHelper.Pi;
			ps3.Theta += MathHelper.Pi;
			ps4.Theta += MathHelper.Pi;
			ps5.Theta += MathHelper.Pi;


			FloorTile ft_door = new FloorTile (Rear, xp + TileSize * (3), yp + 8 + TileSize * 20);
			new ExtenderPlatform (Stage, xp + TileSize * (2), yp + TileSize * 17, true);
			new ExtenderPlatform (Stage, xp + TileSize * (2), yp + TileSize * 18, true);
			new ExtenderPlatform (Stage, xp + TileSize * (2), yp + TileSize * 19, true);
			new ExtenderPlatform (Stage, xp + TileSize * (4), yp + TileSize * 17, true);
			new ExtenderPlatform (Stage, xp + TileSize * (4), yp + TileSize * 18, true);
			new ExtenderPlatform (Stage, xp + TileSize * (4), yp + TileSize * 19, true);



			Scene next_scene = (Scene)Program.MainGame.Scenes["SceneEight"];
			_DoorToNextScene.CornerX = ft_door.CornerX;
			_DoorToNextScene.CornerY = ft_door.CornerY + TileSize;
			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
			_DoorToNextScene.Destination.Corner += _DoorToNextScene.Corner;
			
			
			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

