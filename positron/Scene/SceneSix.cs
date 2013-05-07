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
		protected SceneSix ():
			base()
		{
		}
		protected override void InstantiateConnections()
		{
			_DoorToPreviousScene = new Door(Rear, 0, -30);
			//_DoorToNextScene = new Door(Rear, _DoorToPreviousScene.CornerX + 8 * TileSize, 3 * TileSize);
			_DoorToNextScene = new Door(Rear, _DoorToPreviousScene.CornerX + 3 * TileSize, _DoorToPreviousScene.CornerY);
		}
		protected override void InitializeScene ()
		{
			// Assign base class variables here, before calling the base class initializer
			double x0 = PerimeterOffsetX * TileSize;
			double y0 = PerimeterOffsetY * TileSize;
			PerimeterOffsetX = 100;
			PerimeterOffsetY = -8;
			PerimeterX = 8;
			PerimeterY = 40;
			
			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			xp += TileSize * PerimeterX - (TileSize * 1);
			yp -= TileSize * 3;
			
			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneFive"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			
			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 24) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();
			
			// Control key indicators (info graphics)
			var f_infogfx = new SpriteBase(Rear, xp - 2 * TileSize, yp, Texture.Get("sprite_infogfx_key_f"));

			
			// Top platform set
			ExtenderPlatform last_platform_0 = new ExtenderPlatform (Stage, xp + TileSize * (-3), yp + TileSize * 6, true);
			new ExtenderPlatform (Stage, xp + TileSize * (6), yp + TileSize * -2, last_platform_0);

			var bs_lower0 = new ProjectileSwitch (Front, xp + TileSize * (16.5), yp + TileSize * (11 + 0.5) , (sender, e) =>
			                                      {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				last_platform_0.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_0));
				//bool bstate_bs_lower_0 = last_platform_2.State != SwitchState.Open;
				//gw_lower1.OnAction (e.Self, new ActionEventArgs (bstate || bstate_bs_lower_0, gw_lower1));
			}, 0.1);
			
			// Platform set below top
			ExtenderPlatform last_platform_1 = new ExtenderPlatform (Stage, xp + TileSize * (0), yp + TileSize * 8, true);
			new ExtenderPlatform (Stage, xp + TileSize * (1), yp + TileSize * 8, last_platform_1);

			var bs_lower1 = new ProjectileSwitch (Front, xp + TileSize * (22.5), yp + TileSize * (7 + 0.5) , (sender, e) =>
			                                      {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				last_platform_1.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_1));
				//bool bstate_bs_lower_0 = last_platform_2.State != SwitchState.Open;
				//gw_lower1.OnAction (e.Self, new ActionEventArgs (bstate || bstate_bs_lower_0, gw_lower1));
			}, 0.1);
			
			double recess_switch = -4.0;
			double recess_gw = -2.0;

			var i = 8;

			new FloorTile (Rear, xp - TileSize * 1, yp + 2 * TileSize + 8 + i);
			new FloorTile (Rear, xp - TileSize * 0, yp + 2 * TileSize + 8 + i);
			var ft_door = new FloorTile (Rear, xp + TileSize * 1, yp + 2 * TileSize + 8 + i);
			new FloorTile (Rear, xp + TileSize * 2, yp + 2 * TileSize + 8 + i);
			//new FloorTile (Rear, xp + TileSize, yp + 2 * TileSize + 8);
			
			//Scene next_scene = (Scene)Program.MainGame.Scenes["SceneSeven"];
			//_DoorToNextScene = new Door(Rear, x0 + TileSize * 76, y1 + TileSize, next_scene.DoorToPreviousScene);
			//_DoorToNextScene = new Door(Rear, ft_door.CornerX, ft_door.CornerY + TileSize, next_scene.DoorToPreviousScene);
			//_DoorToNextScene.Destination.Position += _DoorToNextScene.Position;
			
			
			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

