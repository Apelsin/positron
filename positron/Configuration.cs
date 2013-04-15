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
		private static double _FrameRateCap;
		private static int _ThreadSleepTimeStep;
		private static int _ThreadSleepToleranceStep;
		private static float _MaxWorldTimeStep;
		private static bool _AdaptiveTimeStep;
		private static int _CanvasWidth;
		private static int _CanvasHeight;
		private static bool _DrawBlueprints;
		#endregion

		private static Dictionary<String, object> __dict__ = new Dictionary<String, object>();
		static Configuration ()
		{
			// Development path
			string artwork_path = Path.Combine ("..", "..", "Assets", "Artwork");
			if (!Directory.Exists (artwork_path)) {
				// Release path
				artwork_path = Path.Combine ("..", "Assets", "Artwork");
			}
			if (!Directory.Exists (artwork_path)) {
				// Alternative PWD path
				artwork_path = Path.Combine ("Assets", "Artwork");
			}

			// Absolute executable path
			// This is not working on OS X, but it would be nice if it were
			/*
			if(!Directory.Exists(artwork_path))
			{
				string exe_location = System.Reflection.Assembly.GetExecutingAssembly().Location;
				//Console.WriteLine("Executable location is {0}", exe_location);
				string exe_directory = Path.GetDirectoryName(exe_location);
				//Console.WriteLine("Executable directory is {0}", exe_directory);
				artwork_path = Path.Combine(exe_directory, "Assets", "Artwork");
				//Console.WriteLine("Using artwork path {0}", artwork_path);
			}
			*/
			Console.WriteLine ("Using artwork path {0}", artwork_path);
			Set("ArtworkPath", artwork_path);
			_MetersInPixels = 96.0;
			_ForceDueToGravity = -9.8;
			_KeyPressTimeTolerance = 0.1;
			_FrameRateCap = 30;
			_ThreadSleepTimeStep = 1;
			_MaxWorldTimeStep = 0.04f;
			_AdaptiveTimeStep = false;
			_CanvasWidth = 600;
			_CanvasHeight = 400;
			_DrawBlueprints = false;
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
		/// <summary>
		/// Maximum frame rate to work at
		/// </summary>
		public static double FrameRateCap {
			get { return _FrameRateCap; }
		}
		/// <summary>
		/// Time in milliseconds to sleep at a time
		/// during the frame rate cap loop
		/// </summary>
		public static int ThreadSleepTimeStep {
			get { return _ThreadSleepTimeStep; }
		}
		/// <summary>
		/// Maximum time in seconds that the physics solver can
		/// step through time.
		/// High values may risk horrible physics lag whereas
		/// lower values will cause slowness
		/// </summary>
		public static float MaxWorldTimeStep {
			get { return _MaxWorldTimeStep; }
		}
		/// <summary>
		/// Whether the world time step should adapt to the load
		/// </summary>
		public static bool AdaptiveTimeStep {
			get { return _AdaptiveTimeStep; }
		}
		public static int CanvasWidth {
			get { return _CanvasWidth; }
		}
		public static int CanvasHeight {
			get { return _CanvasHeight; }
		}
		public static bool DrawBlueprints {
			get { return _DrawBlueprints; }
			set { _DrawBlueprints = value; }
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
			yield return new KeyValuePair<String, object>("_FrameRateCap", _FrameRateCap);
			yield return new KeyValuePair<String, object>("_FrameRateCap", _ThreadSleepTimeStep);
			yield return new KeyValuePair<String, object>("_MinWorldTimeStep", _MaxWorldTimeStep);
			yield return new KeyValuePair<String, object>("_AdaptiveTimeStep", _AdaptiveTimeStep);
			yield return new KeyValuePair<String, object>("_DrawBlueprints", _DrawBlueprints);
			foreach(KeyValuePair<String, object> e in __dict__)
				yield return e;
		}
		public static void DumpEverything ()
		{
			Console.WriteLine("{");
			Console.WriteLine("\t\"Configuration\": {");
			foreach (KeyValuePair<String, object> kvp in GetAllSettings()) {
				Console.WriteLine("\t\t\"{0}\": \"{1}\",", kvp.Key, kvp.Value);
			}
			Console.WriteLine("\t}");
			Console.WriteLine("}");
		}
		#endregion
	}
}

