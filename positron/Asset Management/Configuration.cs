using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

using OpenTK.Input;

namespace Positron
{
    #region GlobalConfiguration
    [DataContract]
    public class GlobalConfiguration : ConfiguratorBase<GlobalConfiguration>
    {
        private static GlobalConfiguration _Instance;
        public static GlobalConfiguration Instance { get { return _Instance; } }

        #region Stored Settings
        protected bool? _TestBool;
        [DataMember]
        public bool TestBool {
            get { return (bool)_TestBool; }
            internal set { _TestBool = value; }
        }
        protected string _TestString;
        [DataMember]
        public string TestString {
            get { return _TestString; }
            set { _TestString = value; }
        }
        #endregion

        internal override void Setup()
        {
            _TestBool = _TestBool ?? true;
            _TestString = _TestString ?? "Hello, World!";
        }

        protected GlobalConfiguration(IEnumerable<Type> known_types = null) :
            base(known_types)
        {
        }
        /// <summary>
        /// Load and parse (deserialize) the global configuration from a JSON stream.
        /// </summary>
        /// <param name="stream"></param>
        public static void Load(Stream stream)
        {
            var serializer = CreateSerializer();
            _Instance = (GlobalConfiguration)serializer.ReadObject(stream);
            _Instance.Serializer = serializer;
            _Instance.Setup();
        }
        public static void Store(Stream stream)
        {
            using(var writer = CreateJsonWriter(stream))
                _Instance.Serializer.WriteObject(writer, _Instance);
        }
        public static void LoadDefaults()
        {
            _Instance = new GlobalConfiguration();
        }
    }
    #endregion
    #region GameConfiguration
    [DataContract]
    public class GameConfiguration : ConfiguratorBase<GameConfiguration>
    {
        #region Stored Settings
        /// <summary>
        /// Relative path within AssetsPath containing artwork assets
        /// </summary>
        [DataMember] public string ArtworkPath{ get; internal set; }
        /// <summary>
        /// Relative path within AssetsPath containing audio assets
        /// </summary>
        [DataMember] public string AudioPath { get; internal set; }
        /// <summary>
        /// Relative path within AssetsPath containing scene assets
        /// </summary>
        [DataMember] public string ScenePath { get; internal set; }
        /// <summary>
        /// Dictionary containing keyboard-control mapping.
        /// </summary>
        [DataMember] public Dictionary<String, Key> KeyMap { get; internal set; }
        /// <summary>
        /// The acceleration due to gravity to use for most scenes
        /// </summary>
        [DataMember] public float? ForceDueToGravity { get; internal set; }
        /// <summary>
        /// Time in seconds to allow existingly-pressed keystrokes to
        /// retrigger through the bubbling KeysUpdate event.
        /// </summary>
        /// <remarks>
        /// Think of this as "buffer timeframe" in which you can press
        /// a button and have an action happen as soon as it can.
        /// </remarks>
        [DataMember] public float? KeyPressTimeTolerance { get; internal set; }
        /// <summary>
        /// Maximum frame rate to work at
        /// </summary>
        [DataMember] public float? FrameRateCap { get; internal set; }
        /// <summary>
        /// Time in milliseconds to sleep at a time
        /// during the frame rate cap loop
        /// </summary>
        [DataMember] public int? ThreadSleepTimeStep { get; internal set; }
        /// <summary>
        /// Maximum time in seconds that the physics solver can
        /// step through time.
        /// </summary>
        /// <remarks>
        /// High values may risk physics lag whereas
        /// lower values will cause slowness
        /// </remarks>
        [DataMember] public float? MaxWorldTimeStep { get; internal set; }
        /// <summary>
        /// Whether the world time step should adapt to the render load
        /// </summary>
        [DataMember] public bool? AdaptiveTimeStep { get; internal set; }
        [DataMember] public System.Drawing.Size? CanvasSize { get; internal set; }

        #endregion
        
        #region Volatile Settings
        public string AssetsPath { get; internal set; }
        public string ArtworkPathFull
        {
            get { return Path.Combine(AssetsPath, ArtworkPath); }
        }
        public string AudioPathFull
        {
            get { return Path.Combine(AssetsPath, AudioPath); }
        }
        public string ScenePathFull
        {
            get { return Path.Combine(AssetsPath, ScenePath); }
        }
        public bool DrawBlueprints { get;  set; }
        public bool ShowDebugVisuals { get;  set; }
        /// <summary>
        /// Width of the FBO rendering canvas everything will be drawn to
        /// </summary>
        public int CanvasWidth
        {
            get { return CanvasSize.Value.Width; }
            internal set
            {
                CanvasSize = new System.Drawing.Size(value, CanvasSize.Value.Height);
            }
        }
        /// <summary>
        /// Height of the FBO rendering canvas everything will be drawn to
        /// </summary>
        public int CanvasHeight
        {
            get { return CanvasSize.Value.Height; }
            internal set
            {
                CanvasSize = new System.Drawing.Size(CanvasSize.Value.Width, value);
            }
        }
        #endregion
        /// <summary>
        /// Load and parse (deserialize) the global configuration from a JSON stream.
        /// </summary>
        /// <param name="stream"></param>
        public static GameConfiguration Load(Stream stream)
        {
            var serializer = CreateSerializer();
            var instance = (GameConfiguration)serializer.ReadObject(stream);
            instance.Serializer = serializer;
            instance.Setup();
            return instance;
        }
        /// <summary>
        /// Serialize the configuration into a JSON stream
        /// </summary>
        /// <param name="stream"></param>
        public void Store(Stream stream)
        {
            using (var writer = CreateJsonWriter(stream))
                Serializer.WriteObject(writer, this);
        }
        internal override void Setup()
        {
            FindAssetsPath();

            // Load or default
            ArtworkPath = ArtworkPath ?? "Artwork";
            AudioPath = AudioPath ?? "Audio";
            ScenePath = ScenePath ?? "Scene";
            ForceDueToGravity = ForceDueToGravity ?? -9.8f;
            KeyPressTimeTolerance = KeyPressTimeTolerance ?? 0.1f;
            FrameRateCap = FrameRateCap ?? 1200.0f;
            ThreadSleepTimeStep = ThreadSleepTimeStep ?? 1;
            MaxWorldTimeStep = MaxWorldTimeStep ?? 0.05f;
            AdaptiveTimeStep = AdaptiveTimeStep ?? false;

            if (CanvasSize == null)
            {
                CanvasSize = new System.Drawing.Size();
                CanvasWidth = 1280 / 2;
                CanvasHeight = 800 / 2;
            }

            if(KeyMap == null)
            {
                KeyMap = new Dictionary<string, Key>();
                KeyMap["Up"] = Key.W;
                KeyMap["Left"] = Key.A;
                KeyMap["Down"] = Key.S;
                KeyMap["Right"] = Key.D;
                KeyMap["Jump"] = Key.F;

                KeyMap["Reset"] = Key.Number1;
                KeyMap["ResetModifier"] = Key.Number2;
                KeyMap["ToggleFullScreen"] = Key.BackSlash;
                KeyMap["ToggleShowDebugVisuals"] = Key.Semicolon;
                KeyMap["ToggleDrawBlueprints"] = Key.Quote;
            }

            // Volatile
            DrawBlueprints = false;
            ShowDebugVisuals = false; 
        }
        protected void FindAssetsPath()
        {
            // Positron attempts to locate the Assets directory in three different places
            // Assets           Default
            // ../../Assets     Development
            // ../../../Assets  Development, platform-specific
            // Release path
            AssetsPath = "Assets";
#if DEBUG
            if (!Directory.Exists(AssetsPath))
            {
                // Development path
                AssetsPath = Path.Combine("..", "..", "Assets");
                if (!Directory.Exists(AssetsPath))
                {
                    // Platform-specific development path
                    AssetsPath = Path.Combine("..", "..", "..", "Assets");
                }
            }
#endif
            if (!Directory.Exists(AssetsPath))
                throw new FileNotFoundException("Unable to locate Assets directory", AssetsPath);

            Console.WriteLine("Using assets path {0}", AssetsPath);
        }

        public GameConfiguration(IEnumerable<Type> known_types = null) :
            base(known_types)
        {
        }
    }
    #endregion
}
