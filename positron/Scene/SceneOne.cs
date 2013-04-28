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
	public class SceneOne : SceneBasicBox
	{
		protected SceneOne ():
			base()
		{
			//UIGroup = new UIElementGroup();

		}
		protected override void InstantiateConnections ()
		{
			_DoorToNextScene = new Door(Rear, 512 - 68, 0);
		}
		protected override void InitializeScene ()
		{
			// Assign base class variables here, before calling the base class initializer
			PerimeterOffsetX = -1;
			PerimeterOffsetY = -2;

			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;

			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;

			// Set up doors:
			//Scene prev_scene = (Scene)Scene.Scenes["SceneOne"];
			//_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			Scene next_scene = (Scene)Program.MainGame.Scenes["SceneTwo"];
			_DoorToNextScene.Destination = next_scene.DoorToPreviousScene;
			_DoorToNextScene.Destination.Position = _DoorToNextScene.Position;

			yp += TileSize;

			// Set up background tiles
			var BackgroundTiles = new TileMap (Background, 48, 24, Texture.Get ("sprite_tile_bg_atlas"));
			BackgroundTiles.PositionX = xp - 10 * TileSize;
			BackgroundTiles.PositionY = yp - 4 * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();

			double floor_sw_dy = -4.0;

			// Stage elements
			var ft0 = new FloorTile (Rear, xp + 0, yp);
			var ft1 = new FloorTile (Rear, xp + TileSize, yp - TileSize * 0.5);

			// Control key indicators (info graphics)
            var a_infogfx = new SpriteBase(Rear, ft1.CornerX + TileSize, ft1.CornerY + TileSize, Texture.Get("sprite_infogfx_key_a"));
            var d_infogfx = new SpriteBase(Rear, a_infogfx.PositionX + TileSize, a_infogfx.PositionX, Texture.Get("sprite_infogfx_key_d"));

			// Gateways
			var gw1 = new Gateway (Front, xp + TileSize * 4, yp, false);
			var gw2 = new Gateway (Front, xp + TileSize * 10, yp, false);

			var fs00 = new FloorSwitch (Front, xp + TileSize * 3, yp + floor_sw_dy, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			});
			var fs01 = new FloorSwitch (Front, xp + TileSize * 5, yp + floor_sw_dy, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs00);

			var fs10 = new FloorSwitch (Front, xp + TileSize * 7, yp + floor_sw_dy, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, 0.2);

			var fs11 = new FloorSwitch (Front, xp + TileSize * 9 + 14, yp + floor_sw_dy + TileSize * 3, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs10, 3.0);

			var space_infogfx = new SpriteBase (Rear, xp + fs11.PositionX - 128, yp + fs10.PositionY, Texture.Get ("sprite_infogfx_key_spacebar"));

			fs11.Theta = Math.PI * 0.5;
			var fs12 = new FloorSwitch (Front, xp + TileSize * 12, yp + floor_sw_dy, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs11);

			// Walls above gateways
			for (int i = 0; i < 5; i++) {
				new FloorTile (Rear, gw1.CornerX, gw1.CornerY + (2 + i) * TileSize + 8);
				new FloorTile (Rear, gw2.CornerX, gw2.CornerY + (2 + i) * TileSize + 8);
			}
			
			//var TestDialog = new Dialog(HUD, "Test", null);
			//TestDialog.RenderSet = HUD;
			//TestDialog.PauseTime = 1.0f;

			// Call the base class initializer
			base.InitializeScene ();

		}
	}
}

