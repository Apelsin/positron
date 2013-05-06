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
	public class SceneFive : Scene, ISceneGameplay
	{
        protected int PerimeterOffsetX = 0;
        protected int PerimeterOffsetY = 0;
        protected int PerimeterX = 13;
        protected int PerimeterY = 96;

        protected double TileSize = 32;

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
			
            // Testing:
            PerimeterOffsetX = 3 - PerimeterX;
            PerimeterOffsetY = 3 - PerimeterY;
			
			
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

                yp += TileSize * (-3.5);
                var ep1 = new ExtenderPlatform(this.Rear, xp, yp, true);
                for(i += 3; i > 1; i--)
                {
                    new ExtenderPlatform(this.Rear, xp += TileSize, yp, ep1);
                }
                yp += TileSize;
                double yp1 = yp;
                double xp1 = xp;
                var bs1 = new ProjectileSwitch(this.Front, xp + TileSize, yp + 2 * TileSize, (sender, e) => {
                    Program.MainGame.AddUpdateEventHandler(this, (ueh_sender, ueh_e) => { // GL context
                        var bs2 = new ProjectileSwitch(this.Front, xp1 - 11.5 * TileSize, yp1 + 2 * TileSize, (sender1, e1)=>{
                            Program.MainGame.AddUpdateEventHandler(this, (ueh2_sender, ueh2_e) => { // GL context
                                var ps1 = new PressureSwitch(this.Front, xp1 - 5 * TileSize, yp1 + 4, (sender2, e2) => {
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

                yp -= 7 * TileSize;

                for(i = 0; i < 10; i++)
                    new BunkerFloor(this, xp -= TileSize, yp);

            }

           
			
			// Set up previous door:
			Scene prev_scene = (Scene)Program.MainGame.Scenes["SceneFour"];
			_DoorToPreviousScene.Destination = prev_scene.DoorToNextScene;
			
			// Setup background tiles
			var BackgroundTiles = new FadedTileMap (Background, PerimeterX + 20, PerimeterY + 12, Texture.Get ("sprite_tile_bg2_atlas"));
			BackgroundTiles.PositionX = (PerimeterOffsetX - 10) * TileSize;
			BackgroundTiles.PositionY = (PerimeterOffsetY - 6) * TileSize;
			BackgroundTiles.PositionZ = 1.0;
			BackgroundTiles.RandomMap ();
			BackgroundTiles.Build ();
			
			
            // Basic perimeter:
            double x0 = PerimeterOffsetX * TileSize;
            double y0 = PerimeterOffsetY * TileSize;
            for (int i = 0; i < PerimeterX; i++)
            {
                BunkerFloor block = new BunkerFloor2 (this, x0 + TileSize * i, y0);
                block.PositionY -= block.SizeY;
                block = new BunkerFloor (this, x0 + TileSize * i, y0);
            }
            for (int i = 0; i <= PerimeterY; i++)
            {
                BunkerWall wall;

                wall = new BunkerWall(this, x0 - 0.5 * TileSize, y0 + TileSize * (PerimeterY - i));
                if(i > 0 && i < 3)
                    continue;
                wall = new BunkerWall(this, x0 + TileSize * PerimeterX, y0 + TileSize * i);
                wall.TileX = -1.0;
            }
            for (int i = 0; i < PerimeterX; i++)
            {
                var block = new FloorTile(Stage, x0 + TileSize * (PerimeterX - i - 1), y0 + TileSize * PerimeterY);
            }

			
			// Call the base class initializer
			base.InitializeScene ();
		}
	}
}

