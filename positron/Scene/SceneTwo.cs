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
		protected SceneTwo ():
			base()
		{
			Scene prev_scene = (Scene)Scene.Scenes["SceneOne"];
			_DoorToPreviousScene = new Door(Rear, 512 - 68, 32, prev_scene);
		}
		protected override void InitializeConnections()
		{
			//Scene prev_scene = (Scene)Scene.Scenes["SceneOne"];
			//_DoorToPreviousScene = prev_scene.DoorToPreviousScene;
		}
		protected override void InitializeScene ()
		{
			// Define these before calling the base class initializer
			PerimeterOffsetX = -3;
			PerimeterOffsetY = -1;
			PerimeterX = 32;
			PerimeterY = 16;
			
			// Store width and height in local variables
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;
			
			yp += TileSize;

			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = xp - 8 * TileSize;
			BackgroundTiles.PositionY = yp - 4 * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();

			// Control key indicators (info graphics)
			var f_infogfx = new SpriteBase (Rear, _DoorToPreviousScene.PositionX - 2 * TileSize, _DoorToPreviousScene.PositionY, Texture.Get ("sprite_infogfx_key_f"));

			// Get cross-scene variables
			Scene previous_scene = (Scene)Scene.Scenes ["SceneOne"];

			double floor_y = 32.0;
			double floor_sw_y = -4.0;

			xp = _DoorToPreviousScene.CornerX;
			yp = _DoorToPreviousScene.CornerY;

			for (int i = 0; i < 5; i++) {
				var spidey = new Spidey (Stage, xp - (5 + 0.25 * i) * TileSize, yp);
				spidey.Body.BodyType = BodyType.Dynamic;
			}

			// Gateways
			var gw1 = new Gateway (Front, xp - TileSize * 3, yp, false);
			var gw2 = new Gateway (Front, xp + TileSize * 3, yp, false);

			// Walls above gateways
			for (int i = 2; i < PerimeterY; i++) {
				new FloorTile (Rear, gw1.CornerX, gw1.CornerY + (i) * TileSize + 8);
				new FloorTile (Rear, gw2.CornerX, gw2.CornerY + (i) * TileSize + 8);
			}

			var fs10 = new FloorSwitch (Front, gw1.CornerX + TileSize, gw1.CornerY + floor_sw_y, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			});
			var fs11 = new FloorSwitch (Front, gw1.CornerX - TileSize, gw1.CornerY + floor_sw_y, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
			}, fs10);

			var fs20 = new FloorSwitch (Front, gw2.CornerX - TileSize, gw1.CornerY + floor_sw_y, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
			});
			var fs21 = new FloorSwitch (Front, gw2.CornerX + TileSize, gw1.CornerY + floor_sw_y, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
			}, fs20);

			xp = fs21.CornerX + 2 * TileSize;
			for (int i = 0; i < 4; i++)
				new FloorTile (Stage, xp += TileSize, yp);
			yp -= 0.5 * TileSize;
			for (int i = 0; i < 3; i++) {
				new FloorTile (Stage, xp += TileSize, yp);
				new FloorTile (Stage, xp, yp + TileSize);
			}

			base.InitializeScene ();
		}
	}
}

