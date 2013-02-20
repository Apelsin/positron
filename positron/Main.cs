using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;

namespace positron
{
	static class Program
	{
		[STAThread]
		public static void Main ()
		{
			using(OpenTK.GameWindow main_window = new ThreadedRendering())
			{
				main_window.Run();
			}
		}
	}
}