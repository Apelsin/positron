using System;
using System.Collections.Generic;

using OpenTK;

using FarseerPhysics.Collision;

namespace positron
{
	public class SceneFirstMenu : Scene
	{
		protected UIElementGroup UIGroup;
        protected SceneFirstMenu (PositronGame game):
            base(game)
		{
			UIGroup = new UIElementGroup();
			this.SceneEntry += (sender, e) => {
                Sound.KillTheNoise();
                _Game.SetInputAccepters("Main Menu",  new IInputAccepter[] { UIGroup });
			};
			SceneExit += (sender, e) => {
                _Game.RemoveInputAccepters("Main Menu");
			};
			SetupPlayerOnExit();
		}
		protected override void InitializeScene ()
		{
			base.InitializeScene();
			var texture = Texture.Get ("sprite_main_menu_buttons");
			double x_center = (ViewWidth) * 0.5;
			double y_center = (ViewHeight) * 0.5;
			new SpriteBase(HUD, x_center, y_center + 128, Texture.Get("sprite_android_now")).CenterShift();
			((UIButton) new UIButton(this, x_center, y_center,
			             new SpriteBase.SpriteFrame(texture, 4),
			             new SpriteBase.SpriteFrame(texture, 2), UIGroup).CenterShift()).Action += (sender, e) =>
			{
                _Game.CurrentScene = (Scene)_Game.Scenes[(string)Configuration.Get("SceneBeginning")];
			};
			((UIButton) new UIButton(this, x_center, y_center - 48,
			             new SpriteBase.SpriteFrame(texture, 5),
			                         new SpriteBase.SpriteFrame(texture, 3), UIGroup).CenterShift()).Action += (sender, e) =>
			{
                _Game.CurrentScene = (Scene)_Game.Scenes["SceneCredits"];
			};

            var looper = new MusicLooperHACK(this, "last_human_loop");
            looper.Preserve = true;
            looper.RenderSetTransfer += (sender, e) => {
                _Game.AddUpdateEventHandler(this, (sender1, e1)=> {
                    if(!(e.From.Scene is ISceneGameplay) && (e.To.Scene is ISceneGameplay))
                    {
                        looper.SetLoop("induction_loop");
                    }
                    else if((e.From.Scene is ISceneGameplay) && !(e.To.Scene is ISceneGameplay))
                    {
                        looper.SetLoop("last_human_loop");
                    }
                    return true;
                });
            };

		}
	}
}

