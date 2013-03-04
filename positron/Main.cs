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
		private static ThreadedRendering MainWindow;
		public static int CanvasWidth {
			get { return MainWindow.CanvasWidth; }
		}
		public static int CanvasHeight {
			get { return MainWindow.CanvasHeight; }
		}
		[STAThread]
		public static void Main ()
		{
			MainWindow = new ThreadedRendering();
			MainWindow.Run();
		}
	}
}