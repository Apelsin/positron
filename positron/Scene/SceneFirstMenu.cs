using System;

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
				Player player_1 = Program.MainGame.Player1 = new Player (e.To.Stage, e.To.DoorToPreviousScene.CornerX, e.To.DoorToPreviousScene.CornerY, Texture.Get ("sprite_player"));

				//e.To.Follow(player_1, true);

				Program.MainGame.SetInputAccepters("Player1", new IInputAccepter[]{ player_1 });
				Follow(player_1);
				var health_meter = new HealthMeter(e.To.HUD, 64, ViewHeight - 64, player_1);
				health_meter.Preserve = true;
			};
		}
		protected override void InitializeScene ()
		{
			base.InitializeScene();
			var texture = Texture.Get ("sprite_main_menu_buttons");
			double x_center = (ViewWidth) * 0.5;
			double y_center = (ViewHeight) * 0.5;
			new UIButton(this, x_center, y_center + 32,
			             new SpriteBase.SpriteFrame(texture, 4),
			             new SpriteBase.SpriteFrame(texture, 2), UIGroup).Action += (sender, e) =>
			{
				Program.MainGame.CurrentScene = (Scene)Program.MainGame.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
			new UIButton(this, x_center, y_center - 32,
			             new SpriteBase.SpriteFrame(texture, 5),
			                         new SpriteBase.SpriteFrame(texture, 3), UIGroup).Action += (sender, e) =>
			{
				Program.MainGame.CurrentScene = (Scene)Program.MainGame.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
		}
	}
}

