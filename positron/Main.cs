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
		public static MainGame Game;
		/// <summary>
		/// The main window supplies the means of handling the game,
		/// that is, calling Update and Render for the game object and
		/// supplying an appropriate canvas for the game to render to.
		/// </summary>
		public static ThreadedRendering MainWindow;



		[STAThread]
		public static void Main ()
		{
			// Instantiate the main window
			// this also sets up OpenGL
			MainWindow = new ThreadedRendering();
			// Prepare game resources
			// This makes OpenGL calls
			MainGame.InitialSetup();
			// Instantiate a main game
			Game = new MainGame();
			Game.Setup();
			Game.SetupTests();
			// TEST: Dump all the settings:
			Configuration.DumpEverything();
			MainWindow.Run(); // Run the window thread
		}
	}
}