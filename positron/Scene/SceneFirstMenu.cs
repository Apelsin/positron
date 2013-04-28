using System;
using System.Collections.Generic;

using OpenTK;

using FarseerPhysics.Collision;

namespace positron
{
	public class SceneFirstMenu : Scene
	{
		protected UIElementGroup UIGroup;
		protected SceneFirstMenu():
			base()
		{
			UIGroup = new UIElementGroup();
			this.SceneEntry += (sender, e) => {
				Program.MainGame.SetInputAccepters("Main Menu",  new IInputAccepter[] { UIGroup });
			};
			SceneExit += (sender, e) => {
                Program.MainGame.AddUpdateEventHandler(this, (sender2, e2) =>
                {
                    double start_x = 0, start_y = 0;
                    if(e.To.DoorToPreviousScene != null)
                    {
                        start_x = e.To.DoorToPreviousScene.CornerX;
                        start_y = e.To.DoorToPreviousScene.CornerY;
                    }

                    Player player_1 = Program.MainGame.Player1 = new Player(e.To.Stage, start_x, start_y, Texture.Get("sprite_player"));

                    //e.To.Follow(player_1, true);

                    Program.MainGame.SetInputAccepters("Player1", new IInputAccepter[] { player_1 });
                    Follow(player_1);
                    var health_meter = new HealthMeter(e.To.HUD, 64, ViewHeight - 64, player_1);
                    health_meter.Preserve = true;

                    var dialog = new Dialog(e.To.HUD, "Dialog", new List<DialogStanza>());
                    dialog.Begin();

                    return true;
                });
			};
		}
		protected override void InitializeScene ()
		{
			base.InitializeScene();
			var texture = Texture.Get ("sprite_main_menu_buttons");
			double x_center = (ViewWidth) * 0.5;
			double y_center = (ViewHeight) * 0.5;
			new SpriteBase(HUD, x_center, y_center + 128, Texture.Get("sprite_android_now"));
			new UIButton(this, x_center, y_center,
			             new SpriteBase.SpriteFrame(texture, 4),
			             new SpriteBase.SpriteFrame(texture, 2), UIGroup).Action += (sender, e) =>
			{
				Program.MainGame.CurrentScene = (Scene)Program.MainGame.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
			new UIButton(this, x_center, y_center - 48,
			             new SpriteBase.SpriteFrame(texture, 5),
			                         new SpriteBase.SpriteFrame(texture, 3), UIGroup).Action += (sender, e) =>
			{
				Program.MainGame.CurrentScene = (Scene)Program.MainGame.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
		}
	}
}

