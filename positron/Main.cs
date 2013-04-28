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
		/// <summary>
		/// Game supplies the game mechanics
		/// </summary>
		public static PositronGame MainGame;
		/// <summary>
		/// The main window supplies the means of handling the game,
		/// that is, calling Update and Render for the game object and
		/// supplying an appropriate canvas for the game to render to.
		/// </summary>
		public static ThreadedRendering MainWindow;

		/// <summary>
		/// Lock to synchronize rendering and updating
		/// </summary>
		public static object MainUpdateLock = new object();
		
		/// <summary>
		/// Lock to synchronize user input controls
		/// </summary>
		public static object MainUserInputLock = new object();

		[STAThread]
		public static void Main ()
		{
			lock (MainUpdateLock) {
				// Instantiate the main window
				// this also sets up OpenGL
				MainWindow = new ThreadedRendering ();
				// Prepare game resources
				// This makes OpenGL calls
				PositronGame.InitialSetup ();
				// Instantiate a main game
				MainGame = new PositronGame ();
				// Game setup
				MainGame.Setup ();
				MainGame.SetupTests ();
				// TEST: Dump all the settings:
				Configuration.DumpEverything ();
			}
			MainWindow.Run (); // Run the window thread
		}
	}
}