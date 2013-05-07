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
	public class SceneFour : SceneBasicBox
	{
		protected SceneFour ():
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
			
			PerimeterOffsetX = 87;
			PerimeterOffsetY = -12;
			PerimeterX = 32;
			PerimeterY = 12;

            double x0 = PerimeterOffsetX * TileSize;
            double y0 = PerimeterOffsetY * TileSize;

			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;

			xp += TileSize * PerimeterX * 0.5 - (TileSize * 1);
			yp -= TileSize *2;
			
			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneThree"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			
			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg4_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 18) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY ) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();
			
			// Control key indicators (info graphics)
			var f_infogfx = new SpriteBase(Rear, xp - 2 * TileSize, yp, Texture.Get("sprite_infogfx_key_f"));


			// First Set of platforms
			double ep2x = xp - TileSize * 20;
			double ep2y = yp + TileSize;


			// Top platform set
			ExtenderPlatform last_platform_2 = new ExtenderPlatform (Stage, ep2x + TileSize * (5), ep2y + TileSize * 9, true);
			for (int i = 1; i < 12; i++) {
				new ExtenderPlatform (Stage, ep2x + TileSize * (5 + i), ep2y + TileSize * 9, last_platform_2);
			}

			var bs_lower0 = new ProjectileSwitch (Front, ep2x + TileSize * (16.5), ep2y + TileSize * (11 + 0.5) , (sender, e) =>
			                                  {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				last_platform_2.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_2));
				//bool bstate_bs_lower_0 = last_platform_2.State != SwitchState.Open;
				//gw_lower1.OnAction (e.Self, new ActionEventArgs (bstate || bstate_bs_lower_0, gw_lower1));
			}, 0.1);

			// Platform set below top
			ExtenderPlatform last_platform_3 = new ExtenderPlatform (Stage, ep2x + TileSize * (5), ep2y + TileSize * 5, true);
			for (int i = 1; i < 12; i++) {
				new ExtenderPlatform (Stage, ep2x + TileSize * (5 + i), ep2y + TileSize * 5, last_platform_3);
			}

			var bs_lower1 = new ProjectileSwitch (Front, ep2x + TileSize * (22.5), ep2y + TileSize * (7 + 0.5) , (sender, e) =>
			                                      {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				last_platform_3.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_3));
				//bool bstate_bs_lower_0 = last_platform_2.State != SwitchState.Open;
				//gw_lower1.OnAction (e.Self, new ActionEventArgs (bstate || bstate_bs_lower_0, gw_lower1));
			}, 0.1);

			double recess_switch = -4.0;
			double recess_gw = -2.0;
			
			for (int i = 0; i < 12; i++) {
				var spidey = new Spidey (Stage, ep2x + (5 + i) * TileSize, ep2y + TileSize * 6);
				spidey.Body.BodyType = BodyType.Dynamic;
			}


			// Gateways
			var gw1 = new Gateway (Front, xp - TileSize * 3, yp + 3 * TileSize, false);
			var gw2 = new Gateway (Front, xp + TileSize * 3, yp + 3 * TileSize, false);
			
			// Walls above gateways
			for (int i = 2; i < 5; i++) {
				new FloorTile (Rear, gw1.CornerX, gw1.CornerY + (i) * TileSize + 8);
			}

			for (int i = 6; i < PerimeterY - 1; i++) {
				new FloorTile (Rear, gw1.CornerX, gw1.CornerY + (i) * TileSize + 8);
			}

			for (int i = 2; i < PerimeterY - 5; i++) {
				new FloorTile (Rear, gw2.CornerX, gw2.CornerY + (i) * TileSize + 8);
			}


			new FloorTile (Rear, gw2.CornerX - TileSize * 5, gw2.CornerY + 2 * TileSize + 8);
			new FloorTile (Rear, gw2.CornerX - TileSize * 4, gw2.CornerY + 2 * TileSize + 8);
			var ft_door = new FloorTile (Rear, gw2.CornerX - TileSize * 3, gw2.CornerY + 2 * TileSize + 8);
			new FloorTile (Rear, gw2.CornerX - TileSize * 2, gw2.CornerY + 2 * TileSize + 8);
			new FloorTile (Rear, gw2.CornerX - TileSize, gw2.CornerY + 2 * TileSize + 8);

			var fs10 = new PressureSwitch (Front, gw1.CornerX + TileSize, gw1.CornerY + recess_switch, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			});
			var fs11 = new PressureSwitch (Front, gw1.CornerX - TileSize, gw1.CornerY + recess_switch, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			}, fs10);
			
			var fs20 = new PressureSwitch (Front, gw2.CornerX - TileSize, gw2.CornerY + recess_switch, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
			});
			var fs21 = new PressureSwitch (Front, gw2.CornerX + TileSize, gw2.CornerY + recess_switch, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
			}, fs20);
			
			xp = fs21.CornerX + 2 * TileSize;
			yp += TileSize;
			xp += TileSize;

			var ft_room_r = new FloorTile (Stage, xp, yp + TileSize * 2);


			for (int i = 1; i < 8; i++)
				new FloorTile (Stage, xp + i * TileSize, yp + TileSize * 2);

			for (int i = 0; i < 4; i++)
				new FloorTile (Stage, xp + (i + 1) * TileSize, yp + TileSize * 3);
			//yp += 2 * TileSize;
			//xp += 4 * TileSize;

			for (int i = 0; i < 6; i++) {
				var spidey = new Spidey (Stage, xp + (5 + i) * TileSize, yp + TileSize * (3));
				spidey.Body.BodyType = BodyType.Dynamic;
			}

			ExtenderPlatform last_platform_0 = null;
			ExtenderPlatform last_platform_1 = null;
			for (int i = 0; i < 3; i++) {
				last_platform_0 = new ExtenderPlatform (Stage, xp + TileSize * (5 + i), yp + TileSize * (4.5 + i), last_platform_0);
			}

			xp -= 2 * TileSize;
			yp += TileSize;
			for (int i = 0; i < 9; i++) {
				last_platform_1 = new ExtenderPlatform (Stage, xp + TileSize * (-1 + i), yp + TileSize * (7), last_platform_1);
			}

			var fs30 = new PressureSwitch (Front, ft_room_r.CornerX, ft_room_r.CornerY + TileSize + recess_switch, (sender, e) => {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				last_platform_1.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_1));
			}, 6.0);

			var bs_lower2 = new ProjectileSwitch (Front, ep2x + TileSize * (23.55), ep2y + TileSize * (7 + 0.5) , (sender, e) =>
			                                      {
				bool bstate = (SwitchState)e.Info != SwitchState.Open;
				last_platform_0.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_0));
			}, 3.0);
			//bs_lower2.CenterShift();
			bs_lower2.Theta += MathHelper.Pi;
			
			// Set up next door:
			/*(
			Scene next_scene = (Scene)Program.MainGame.Scenes["SceneThree"];
			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
			_DoorToNextScene.PositionX = last_platform_1.PositionX;
			_DoorToNextScene.CornerY = yp += last_platform_1.SizeY;

			// Update next scene's door position
			_DoorToNextScene.Destination.Position = _DoorToNextScene.Position;
			*/
			Scene next_scene = (Scene)Program.MainGame.Scenes["SceneFive"];
			//Scene next_scene = (Scene)Program.MainGame.Scenes["SceneSix"];
			//_DoorToNextScene = new Door(Rear, x0 + TileSize * 76, y1 + TileSize, next_scene.DoorToPreviousScene);
			_DoorToNextScene = new Door(Rear, ft_door.CornerX, ft_door.CornerY + TileSize, next_scene.DoorToPreviousScene);
			_DoorToNextScene.CornerX = ft_door.CornerX;
			_DoorToNextScene.CornerY = ft_door.CornerY + TileSize;
			_DoorToNextScene.Destination.Position += _DoorToNextScene.Position;
			

			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

