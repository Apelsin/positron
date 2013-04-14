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
//		protected SceneOne ():
//			base()
//		{
//
//		}
		protected override void Initialize ()
		{
			base.Initialize();

			// Store width and height in local variables
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;

			// Setup background tiles
			var BackgroundTiles = new TileMap(Background, 48, 24, Texture.Get("sprite_tile_bg_atlas"));
			BackgroundTiles.PositionX = -320 + 16;
			BackgroundTiles.PositionY = -256 + 16;
			BackgroundTiles.RandomMap();
			BackgroundTiles.Build();

			Scene next_scene = (Scene)Scene.Scenes["SceneTwo"];
			_DoorToNextScene = new Door(Rear, 512 - 68, 48, next_scene);

			// Get cross-scene variables
			Player player_1 = Program.MainGame.Player1;
			
			// Set up Player 1
			Vector3d q = new Vector3d (-48, 64, 0.0);
			Program.MainGame.Player1.Position += q;
			Follow(player_1, true);
			
			var health_meter = new HealthMeter(HUD, 64, h_i - 64, player_1);
			health_meter.Preserve = true;


			// Stage elements
			new FloorTile(Rear, 0, 32 * 1);
			new FloorTile(Rear, 32, 32 * 0.5);

			var gw1 = new Gateway(Front, 32 * 4, 32 * 1 + 20, false);
			var gw2 = new Gateway(Front, 32 * 10, 32 * 1 + 20, false);

			var fs00 = new FloorSwitch(Front, 32 * 3, 32 * 1 - 14, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction(e.Self, new ActionEventArgs(bstate, gw1));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			} );
			var fs01 = new FloorSwitch(Front, 32 * 5, 32 * 1 - 14, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction(e.Self, new ActionEventArgs(bstate, gw1));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs00);

			var fs10 = new FloorSwitch(Front, 32 * 7, 32 * 1 - 14, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction(e.Self, new ActionEventArgs(bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, 0.2);
			var fs11 = new FloorSwitch(Front, 32 * 9 + 14, 32 * 3 + 4, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction(e.Self, new ActionEventArgs(bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs10, 3.0);
			fs11.Theta = Math.PI * 0.5;
			var fs12 = new FloorSwitch(Front, 32 * 12, 32 * 1 - 14, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction(e.Self, new ActionEventArgs(bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs11);

			new FloorTile(Rear, 32 * 4, 32 * 3 + 8);
			new FloorTile(Rear, 32 * 4, 32 * 4 + 8);
			new FloorTile(Rear, 32 * 4, 32 * 5 + 8);
			new FloorTile(Rear, 32 * 4, 32 * 6 + 8);
			new FloorTile(Rear, 32 * 4, 32 * 7 + 8);

			new FloorTile(Rear, 32 * 10, 32 * 3 + 8);
			new FloorTile(Rear, 32 * 10, 32 * 4 + 8);
			new FloorTile(Rear, 32 * 10, 32 * 5 + 8);
			new FloorTile(Rear, 32 * 10, 32 * 6 + 8);
			new FloorTile(Rear, 32 * 10, 32 * 7 + 8);
			
			//var TestDialog = new Dialog(HUD, "Test", null);
			//TestDialog.RenderSet = HUD;
			//TestDialog.PauseTime = 1.0f;
		}
	}
}

