using System;
using System.Collections.Generic;

namespace positron
{
	public class SceneIntro : Scene
	{
		public Dialog MainDialog;
        public SceneIntro (PositronGame game):
            base(game)
		{
			SceneEntry += (sender, e) => {
                var stanzas = new List<DialogStanza>();
                DialogSpeaker speaker = null;//DialogSpeaker.Get("protagonist");
                stanzas.Add(new DialogStanza(speaker, ">electrical humming"));
                stanzas.Add(new DialogStanza(speaker, ">electrical humming\n..."));
                var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
                dialog.DialogEnd += (sender2, e2) =>
                {
                    _Game.CurrentScene = ((Scene)_Game.Scenes["SceneOne"]);
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

