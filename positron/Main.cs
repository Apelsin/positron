using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using OpenTK;

namespace positron
{
	static class Program
	{
		[STAThread]
		public static void Main ()
		{
			// Instantiate the main window
			// this also sets up OpenGL
			var main_window = new ThreadedRendering ();
			// Prepare game resources
			// This makes OpenGL calls
			PositronGame.InitialSetup ();
			// TEST: Dump all the settings:
			//Configuration.DumpEverything ();
            // Run the window thread
            // Game will be set up by render/update thread -because reasons-
			main_window.Run ();
        }
	}
}