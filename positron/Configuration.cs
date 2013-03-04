using System;
using System.IO;
using System.Collections.Generic;

namespace positron
{
	public static class Configuration
	{
		public static Dictionary<String, object> __dict__ = new Dictionary<String, object>();
		static Configuration()
		{
			string artwork_path = Path.Combine("..", "..", "Assets", "Artwork");
			__dict__.Add("ArtworkPath", artwork_path);
		}
		#region Alias Accessors
		public static string ArtworkPath {
			get { return (string)__dict__["ArtworkPath"]; }
		}
		#endregion
	}
}

