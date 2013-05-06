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
	public class SceneFive : SceneBasicBox
	{
        protected SceneFive ():
			base()
		{
		}
		protected override void InstantiateConnections()
		{
			_DoorToPreviousScene = new Door(Rear, 0, 0);
			_DoorToNextScene = new Door(Rear, _DoorToPreviousScene.CornerX + 8 * TileSize, 3 * TileSize);
		}
		protected override void InitializeScene ()
		{
			// Assign base class variables here, before calling the base class initializer
			
			PerimeterX = 32;
			PerimeterY = 96;

            PerimeterOffsetX = 3 - PerimeterX;
            PerimeterOffsetY = 3 - PerimeterY;
			
			// Store width and height in local variables for easy access
			int w_i = (int)ViewWidth;
			int h_i = (int)ViewHeight;
			
			// X and Y positioner variables
			double xp = TileSize * PerimeterOffsetX;
			double yp = TileSize * PerimeterOffsetY;

            xp += TileSize * (PerimeterX - 1);
            yp += TileSize * (PerimeterY - 4);

            {
                int i, j;

                new BunkerFloor(this, xp, yp);
                new BunkerFloor(this, xp, yp + TileSize);
                new BunkerFloor(this, xp, yp + 2 * TileSize);

                for(i = 0; i < 10; i++)
                    new BunkerFloor(this, xp -= TileSize, yp);

                new RadioProp(Stage, xp, yp + TileSize);
                xp -= TileSize * 2;

                for(j = 3; j > -3; j--)
                    new BunkerWall(this, xp, yp + TileSize * j);
                yp += TileSize * (j - 0.5);
                var ep1 = new ExtenderPlatform(this.Rear, xp, yp, true);
                for(i += 3; i > 1; i--)
                {
                    new ExtenderPlatform(this.Rear, xp += TileSize, yp, ep1);
                }
                yp += TileSize;
                var bs1 = new ProjectileSwitch(this.Front, xp + TileSize, yp + 2 * TileSize, (sender, e) => {
                    Program.MainGame.AddUpdateEventHandler(this, (ueh_sender, ueh_e) => { // GL context
                        var bs2 = new ProjectileSwitch(this.Front, xp - 11 * TileSize, yp + 2 * TileSize, (sender1, e1)=>{
                            Program.MainGame.AddUpdateEventHandler(this, (ueh2_sender, ueh2_e) => { // GL context
                                var ps1 = new PressureSwitch(this.Front, xp - 5 * TileSize, yp + 4, (sender2, e2) => {
                                    bool bstate = (SwitchState)e2.Info == SwitchState.Open;
                                    ep1.OnAction (e2.Self, new ActionEventArgs (bstate, ep1));
                                }, 1.0);
                                return true;
                            });
                        }, 1.0).CenterShift();
                        bs2.PositionX -= 0.5 * bs2.SizeX;
                        bs2.Theta = MathHelper.Pi;
                        return true;
                    });
                }, 1.0).CenterShift();


            }
			
			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneFour"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			
			// Setup background tiles
			var BackgroundTiles = new TileMap (Background, 54, 110, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 9) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 4) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();
			
			
			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

