using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace positron
{
	public static class Configuration
	{
		// This exists because dictionaries can be slow, and that's nooo gooood.
		// GOTTA GO FAST!
		#region Hard-Coded Settings
		private static double _MetersInPixels;
		private static double _ForceDueToGravity;
		private static double _KeyPressTimeTolerance;
		#endregion

		private static Dictionary<String, object> __dict__ = new Dictionary<String, object>();
		static Configuration()
		{
			string artwork_path = Path.Combine("..", "..", "Assets", "Artwork");
			Set("ArtworkPath", artwork_path);
			_MetersInPixels = 64.0;
			_ForceDueToGravity = -9.8;
			_KeyPressTimeTolerance = 0.2;
		}
		#region Alias Accessors
		public static string ArtworkPath {
			get { return (string)__dict__["ArtworkPath"]; }
		}
		/// <summary>
		/// Meters in pixels scalar for FarseerPhysics to use
		/// Protip: don't muck with this
		/// </summary>
		public static double MeterInPixels {
			get { return _MetersInPixels; }
		}
		/// <summary>
		/// The acceleration due to gravity to use for most scenes
		/// </summary>
		public static double ForceDueToGravity {
			get { return _ForceDueToGravity; }
		}
		/// <summary>
		/// Time in seconds to allow existingly-pressed keystrokes to
		/// retrigger through the bubbling KeysUpdate event.
		/// Think of this as "buffer timeframe" in which you can press
		/// a button and have an action happen as soon as it can.
		/// </summary>
		public static double KeyPressTimeTolerance {
			get { return _KeyPressTimeTolerance; }
		}
		public static void Set(String key, object value)
		{
			__dict__[key] = value;
		}
		public static object Get(String key)
		{
			return __dict__[key];
		}
		public static void LoadConfigurationFile(string config_file_path)
		{
			// Mono's compiler does not like this exception for whatever reason
			//throw NotImplementedException;
		}
		public static IEnumerable<KeyValuePair<String, object>> GetAllSettings()
		{
			yield return new KeyValuePair<String, object>("_MetersInPixels", _MetersInPixels);
			yield return new KeyValuePair<String, object>("_ForceDueToGravity", _ForceDueToGravity);
			yield return new KeyValuePair<String, object>("_KeyPressTimeTolerance", _KeyPressTimeTolerance);
			foreach(KeyValuePair<String, object> e in __dict__)
				yield return e;
		}
		#endregion
	}
}

