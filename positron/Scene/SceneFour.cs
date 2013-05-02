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
			_DoorToPreviousScene = new Door(Rear, 512 - 68, 0);
			_DoorToNextScene = new Door(Rear, _DoorToPreviousScene.CornerX + 8 * TileSize, 3 * TileSize);
		}
		protected override void InitializeScene ()
		{
			// Assign base class variables here, before calling the base class initializer
			PerimeterOffsetX = -2;
			PerimeterOffsetY = 0;
			PerimeterX = 32;
			PerimeterY = 12;
			
			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			xp += TileSize * PerimeterX * 0.5;
			yp += TileSize;
			
			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			
			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 9) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();
			
			// Control key indicators (info graphics)
			var f_infogfx = new SpriteBase(Rear, xp - 2 * TileSize, yp, Texture.Get("sprite_infogfx_key_f"));
			
			// Get cross-scene variables
			Scene previous_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
			
			double recess_switch = -4.0;
			double recess_gw = -2.0;
			
			for (int i = 0; i < 5; i++) {
				var spidey = new Spidey (Stage, xp - (5 + 0.25 * i) * TileSize, yp);
				spidey.Body.BodyType = BodyType.Dynamic;
			}
			
			// Gateways
			var gw1 = new Gateway (Front, xp - TileSize * 3, yp + recess_gw, false);
			var gw2 = new Gateway (Front, xp + TileSize * 3, yp + recess_gw, false);
			
			// Walls above gateways
			for (int i = 2; i < PerimeterY - 1; i++) {
				new FloorTile (Rear, gw1.CornerX, gw1.CornerY + (i) * TileSize + 8);
				new FloorTile (Rear, gw2.CornerX, gw2.CornerY + (i) * TileSize + 8);
			}
			
			var fs10 = new FloorSwitch (Front, gw1.CornerX + TileSize, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			});
			var fs11 = new FloorSwitch (Front, gw1.CornerX - TileSize, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			}, fs10);
			
			var fs20 = new FloorSwitch (Front, gw2.CornerX - TileSize, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
			});
			var fs21 = new FloorSwitch (Front, gw2.CornerX + TileSize, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
			}, fs20);
			
			xp = fs21.CornerX + 2 * TileSize;
			
			var ft_room_r = new FloorTile (Stage, xp, yp);
			for (int i = 1; i < 8; i++)
				new FloorTile (Stage, xp + i * TileSize, yp);
			
			yp += TileSize;
			xp += TileSize;
			
			for (int i = 0; i < 4; i++)
				new FloorTile (Stage, xp + (i + 1) * TileSize, yp);
			//yp += 2 * TileSize;
			xp += 4 * TileSize;
			
			ExtenderPlatform last_platform_0 = null;
			ExtenderPlatform last_platform_1 = null;
			for (int i = 0; i < 3; i++) {
				last_platform_0 = new ExtenderPlatform (Stage, xp += TileSize, yp += TileSize, last_platform_0);
			}
			xp -= 2 * TileSize;
			yp += TileSize;
			for (int i = 0; i < 6; i++) {
				last_platform_1 = new ExtenderPlatform (Stage, xp -= TileSize, yp, true);
			}
			
			// Set up next door:
			Scene next_scene = (Scene)Program.MainGame.Scenes["SceneThree"];
			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
			_DoorToNextScene.PositionX = last_platform_1.PositionX;
			_DoorToNextScene.CornerY = yp += last_platform_1.SizeY;
			
			// Update next scene's door position
			_DoorToNextScene.Destination.Position = _DoorToNextScene.Position;
			
			
			var fs30 = new FloorSwitch (Front, ft_room_r.CornerX + TileSize, ft_room_r.CornerY + TileSize + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				last_platform_0.OnAction (e.Self, new ActionEventArgs (bstate, last_platform_0));
			}, 5.0);
			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

