using System;
using System.Collections.Generic;

namespace positron
{
	public class SceneIntro : Scene
	{
		public Dialog MainDialog;
		public SceneIntro ():
				base()
		{
			SceneEntry += (sender, e) => {
				var stanzas = new List<DialogStanza>();
				DialogSpeaker speaker = null;//DialogSpeaker.Get("protagonist");
				stanzas.Add(new DialogStanza(speaker, ">electrical humming"));
				stanzas.Add (new DialogStanza(speaker, "Where am I?"));
				stanzas.Add (new DialogStanza(speaker, "How I get here?"));
				stanzas.Add (new DialogStanza(speaker, "What is this place?"));
				stanzas.Add (new DialogStanza(speaker, ">CONTINUED DIALOG"));
				var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
				dialog.DialogEnd += (sender2, e2) => {
					Program.MainGame.ChangeScene((Scene)Program.MainGame.Scenes["SceneOne"]);
				};
				dialog.Begin();
			};
			SceneExit += (sender, e) => {
				Program.MainGame.AddUpdateEventHandler(this, (sender2, e2) => {
					double start_x = 0, start_y = 0;
					if(e.To.DoorToPreviousScene != null)
					{
						start_x = e.To.DoorToPreviousScene.CornerX;
						start_y = e.To.DoorToPreviousScene.CornerY;
					}

					new Spidey(e.To.Stage, start_x, start_y);
					Player player_1 = Program.MainGame.Player1 = new Player(e.To.Stage, start_x, start_y, Texture.Get("sprite_player"));
					//e.To.Follow(player_1, true);
					Program.MainGame.SetLastInputAccepters("Player1", new IInputAccepter[] { player_1 });
					Follow(player_1);

					var health_meter = new HealthMeter(e.To.HUD, 64, ViewHeight - 64, player_1);
					health_meter.Preserve = true;
					return true;
				});
			};
		}
		protected override void InitializeScene ()
		{
			base.InitializeScene ();
		}
	}
}

