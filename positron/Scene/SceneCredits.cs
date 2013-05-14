using System;
using System.Collections.Generic;

namespace positron
{
	public class SceneCredits : Scene
	{
		public Dialog MainDialog;
        public SceneCredits (PositronGame game):
            base(game)
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
                    _Game.ChangeScene((Scene)_Game.Scenes["SceneFirstMenu"]);
                };
                dialog.Begin();
			};
			SetupPlayerOnExit();
		}
		public override void InitializeScene ()
		{
			base.InitializeScene ();
		}
	}
}

