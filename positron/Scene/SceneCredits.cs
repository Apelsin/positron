using System;
using System.Collections.Generic;

namespace positron
{
	public class SceneCredits : Scene
	{
		public Dialog MainDialog;
		public SceneCredits ():
				base()
		{
			SceneEntry += (sender, e) => {
                var stanzas = new List<DialogStanza>();
                DialogSpeaker speaker = null;//DialogSpeaker.Get("protagonist");
                stanzas.Add(new DialogStanza(speaker,
                                             "Artwork:           Music:             Programming:\n" +
                                             "Fernando Corrales  A-Zu-Ra            Vince BG    \n" +
                                             "Megan Groden       Laurence Simmonds  Will Pham   \n" +
                                             "Vince BG           Will Pham                      "));
                var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
                dialog.DialogEnd += (sender2, e2) =>
                {
                    Program.MainGame.ChangeScene((Scene)Program.MainGame.Scenes["SceneFirstMenu"]);
                };
                dialog.Begin();
			};
			SetupPlayerOnExit();
		}
		protected override void InitializeScene ()
		{
			base.InitializeScene ();
		}
	}
}

