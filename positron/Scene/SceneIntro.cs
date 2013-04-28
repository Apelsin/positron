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
		}
		protected override void InitializeScene ()
		{
			base.InitializeScene ();
			var dialog = new Dialog(HUD, "Dialog", new List<DialogStanza>());
		}
	}
}

