using System;

using OpenTK;

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
				var player_1 = Program.MainGame.Player1 = new Player (e.To.Stage, Texture.Get ("sprite_player"));
				if(e.To.DoorToPreviousScene != null)
					player_1.Corner = e.To.DoorToPreviousScene.Corner;
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
			double btn_center_x = (ViewWidth - texture.Regions[0].SizeX) * 0.5;
			double btn_center_y = (ViewHeight - texture.Regions[0].SizeY) * 0.5;
			new UIButton(this, btn_center_x, btn_center_y + 32,
			             new SpriteBase.SpriteFrame(texture, 4),
			             new SpriteBase.SpriteFrame(texture, 2), UIGroup).Action += (sender, e) =>
			{
				Program.MainGame.CurrentScene = (Scene)Scene.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
			new UIButton(this, btn_center_x, btn_center_y - 32,
			             new SpriteBase.SpriteFrame(texture, 5),
			             new SpriteBase.SpriteFrame(texture, 3), UIGroup);
		}
	}
}

