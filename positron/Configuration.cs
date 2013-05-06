using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using OpenTK.Input;

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
		//private static int _ThreadSleepToleranceStep;
		private static float _MaxWorldTimeStep;
		private static bool _AdaptiveTimeStep;
		private static int _CanvasWidth;
		private static int _CanvasHeight;
		private static bool _DrawBlueprints;
		private static bool _ShowDebugVisuals;

        private static Key
            _KeyUp,
            _KeyLeft,
            _KeyDown,
            _KeyRight,
            _KeyJump,
            _KeyUseEquippedItem,
            _KeyDoAction,
            _KeyCrouch,
            _KeyReset,
            _KeyToggleFullScreen,
            _KeyToggleShowDebugVisuals,
            _KeyToggleDrawBlueprints;
		#endregion

		private static Dictionary<String, object> __dict__ = new Dictionary<String, object>();
		static Configuration ()
		{
			// Development path
			string assets_path = Path.Combine ("..", "..", "Assets");
			if (!Directory.Exists (assets_path)) {
				// Release path
				assets_path = Path.Combine ("..", "Assets");
			}
			if (!Directory.Exists (assets_path)) {
				// Alternative PWD path
                assets_path = "Assets";
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
			Console.WriteLine ("Using assets path {0}", assets_path);
			Set("ArtworkPath", Path.Combine(assets_path, "Artwork"));
            Set("AudioPath", Path.Combine(assets_path, "Audio"));
			Set ("SceneBeginning", "SceneIntro");
			_MetersInPixels = 96.0;
			_ForceDueToGravity = -9.8;
			_KeyPressTimeTolerance = 0.1;
			_FrameRateCap = 30;
			_ThreadSleepTimeStep = 1;
			_MaxWorldTimeStep = 0.05f;
			_AdaptiveTimeStep = false;
			_CanvasWidth = 1280 / 2;
			_CanvasHeight = 720 / 2;
			_DrawBlueprints = false;
			_ShowDebugVisuals = false;
            _KeyUp = Key.W;
            _KeyLeft = Key.A;
            _KeyDown = Key.S;
            _KeyRight = Key.D;
            _KeyJump = Key.F;
            _KeyUseEquippedItem = Key.G;
            _KeyDoAction = Key.H;
            _KeyCrouch = Key.C;
            _KeyReset = Key.Number1;
            _KeyToggleFullScreen = Key.BackSlash;
            _KeyToggleShowDebugVisuals = Key.V;
            _KeyToggleDrawBlueprints = Key.B;
		}
		#region Alias Accessors
		public static string ArtworkPath {
			get { return (string)__dict__["ArtworkPath"]; }
		}
        public static string AudioPath {
            get { return (string)__dict__["AudioPath"]; }
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
		public static bool ShowDebugVisuals {
			get { return _ShowDebugVisuals; }
			set { _ShowDebugVisuals = value; }
		}
        public static Key KeyUp { get { return _KeyUp; } }
        public static Key KeyLeft { get { return _KeyLeft; } }
        public static Key KeyDown { get { return _KeyDown; } }
        public static Key KeyRight { get { return _KeyRight; } }
        public static Key KeyJump { get { return _KeyJump; } }
        public static Key KeyUseEquippedItem { get { return _KeyUseEquippedItem; } }
        public static Key KeyDoAction { get { return _KeyDoAction; } }
        public static Key KeyCrouch { get { return _KeyCrouch; } }
        public static Key KeyReset { get { return _KeyReset; } }
        public static Key KeyToggleFullScreen { get { return _KeyToggleFullScreen; } }
        public static Key KeyToggleShowDebugVisuals { get { return _KeyToggleShowDebugVisuals; } }
        public static Key KeyToggleDrawBlueprints { get { return _KeyToggleDrawBlueprints; } }
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
			yield return new KeyValuePair<String, object>("_ThreadSleepTimeStep", _ThreadSleepTimeStep);
			yield return new KeyValuePair<String, object>("_MinWorldTimeStep", _MaxWorldTimeStep);
			yield return new KeyValuePair<String, object>("_AdaptiveTimeStep", _AdaptiveTimeStep);
			yield return new KeyValuePair<String, object>("_DrawBlueprints", _DrawBlueprints);
			yield return new KeyValuePair<String, object>("_ShowDebugVisuals", _ShowDebugVisuals);
            yield return new KeyValuePair<String, object>("_KeyUp", _KeyUp);
            yield return new KeyValuePair<String, object>("_KeyLeft", _KeyLeft);
            yield return new KeyValuePair<String, object>("_KeyDown", _KeyDown);
            yield return new KeyValuePair<String, object>("_KeyRight", _KeyRight);
            yield return new KeyValuePair<String, object>("_KeyJump", _KeyJump);
            yield return new KeyValuePair<String, object>("_KeyUseEquippedItem", _KeyUseEquippedItem);
            yield return new KeyValuePair<String, object>("_KeyDoAction", _KeyDoAction);
            yield return new KeyValuePair<String, object>("_KeyCrouch", _KeyCrouch);
            yield return new KeyValuePair<String, object>("_KeyReset", _KeyReset);
            yield return new KeyValuePair<String, object>("_KeyToggleFullScreen", _KeyToggleFullScreen);
            yield return new KeyValuePair<String, object>("_KeyToggleShowDebugVisuals", _KeyToggleShowDebugVisuals);
            yield return new KeyValuePair<String, object>("_KeyToggleDrawBlueprints", _KeyToggleDrawBlueprints);
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

