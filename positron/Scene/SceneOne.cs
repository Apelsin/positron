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
		protected SpriteObject FirstTile;
		protected SceneOne ():
			base()
		{
			SceneEntry += (sender, e) => {
				if(!(e.From is SceneTwo))
				{
					var stanzas = new List<DialogStanza>();
					DialogSpeaker speaker = DialogSpeaker.Get("protagonist");
					stanzas.Add(new DialogStanza(speaker, "Here I am"));
					stanzas.Add (new DialogStanza(speaker, "TO SAVE THE DAY!"));
					var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
					dialog.Begin();
					Program.MainGame.AddUpdateEventHandler(this, (sender2, e2) =>
					{
						Program.MainGame.Player1.PositionX = FirstTile.PositionX;
						Program.MainGame.Player1.PositionY =
							FirstTile.PositionY + 0.5 * (FirstTile.SizeY + Program.MainGame.Player1.Texture.DefaultRegion.SizeY);
						return true;
					});
				}
			};
		}
		protected override void InstantiateConnections ()
		{
			_DoorToNextScene = new Door(Rear, 512 - 68, TileSize);
		}
		protected override void InitializeScene ()
		{
			// Assign base class variables here, before calling the base class initializer
			PerimeterOffsetX = 0;
			PerimeterOffsetY = 0;

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

			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 36, 16, Texture.Get ("sprite_tile_bg_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 9) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();

			double recess_switch = -4.0;
			double recess_gw = -2.0;

			// Stage elements
			var ft0 = new FloorTile (Rear, xp, yp);
			FirstTile = ft0;
			var ft1 = new FloorTile (Rear, xp + TileSize, yp);
			var ft2 = new FloorTile (Rear, xp + 2 * TileSize, yp - TileSize * 0.5);

			// Control key indicators (info graphics)
            var a_infogfx = new SpriteBase(Rear, ft1.CornerX + TileSize, ft1.CornerY + TileSize, Texture.Get("sprite_infogfx_key_a"));
            var d_infogfx = new SpriteBase(Rear, a_infogfx.CornerX + TileSize, a_infogfx.CornerY, Texture.Get("sprite_infogfx_key_d"));
			var w_infogfx = new SpriteBase(Rear, 0, _DoorToNextScene.CornerY + _DoorToNextScene.SizeY + 4, Texture.Get("sprite_infogfx_key_w"));
			w_infogfx.PositionX = _DoorToNextScene.PositionX;

			// Gateways
			var gw1 = new Gateway (Front, xp + TileSize * 4, yp + recess_gw, false);
			var gw2 = new Gateway (Front, xp + TileSize * 10, yp + recess_gw, false);

			var fs00 = new FloorSwitch (Front, xp + TileSize * 3, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			});
			var fs01 = new FloorSwitch (Front, xp + TileSize * 5, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw1.OnAction (e.Self, new ActionEventArgs (bstate, gw1));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs00);

			var fs10 = new FloorSwitch (Front, xp + TileSize * 7, yp + recess_switch, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, 0.2);

			var fs11 = new FloorSwitch (Front, xp + TileSize * 9 + 14, yp + recess_switch + TileSize * 3, (sender, e) => {
				bool bstate = (FloorSwitch.SwitchState)e.Info != FloorSwitch.SwitchState.Open;
				gw2.OnAction (e.Self, new ActionEventArgs (bstate, gw2));
				//Console.WriteLine("{0} acted on {1}: {2}", sender, e.Self, e.Info);
			}, fs10, 3.0);

			var space_infogfx = new SpriteBase (Rear, xp + fs11.PositionX - 128, yp + fs10.PositionY, Texture.Get ("sprite_infogfx_key_spacebar"));

			fs11.Theta = Math.PI * 0.5;
			var fs12 = new FloorSwitch (Front, xp + TileSize * 12, yp + recess_switch, (sender, e) => {
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

