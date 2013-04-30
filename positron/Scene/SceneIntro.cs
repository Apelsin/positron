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
                stanzas.Add(new DialogStanza(speaker, "Where am I?"));
                stanzas.Add(new DialogStanza(speaker, "How I get here?"));
                stanzas.Add(new DialogStanza(speaker, "What is this place?"));
                stanzas.Add(new DialogStanza(speaker, ">CONTINUED DIALOG"));
                var dialog = new Dialog(e.To.HUD, "Dialog", stanzas);
                dialog.DialogEnd += (sender2, e2) =>
                {
                    Program.MainGame.ChangeScene((Scene)Program.MainGame.Scenes["SceneOne"]);
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

