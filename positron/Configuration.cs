using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ComponentModel;
using System.Xml;

using System.Dynamic;

using OpenTK.Input;

namespace Positron
{
    #region ConfigurationBase
    [DataContract]
    public abstract class ConfigurationBase
    {
        protected static DataContractJsonSerializerSettings SerializerSettings =
            new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                };
        protected static XmlWriter CreateJsonWriter(Stream stream)
        {
            return JsonReaderWriterFactory.CreateJsonWriter(stream, System.Text.Encoding.UTF8, true, true, "    ");
        }
        protected static XmlObjectSerializer Serializer = new DataContractJsonSerializer(typeof(ConfigurationBase));
        protected abstract void Setup();
        protected ConfigurationBase()
        {
            Setup();
        }
        [OnDeserialized]
        protected void OnDeserialized(StreamingContext context)
        {
            Setup();
        }
    }
    #endregion
    #region GlobalConfiguration
    [DataContract]
    public class GlobalConfiguration : ConfigurationBase
    {
        new protected static XmlObjectSerializer Serializer =
            new DataContractJsonSerializer(
                typeof(GlobalConfiguration),
                SerializerSettings);
        private static GlobalConfiguration _Instance;
        public static GlobalConfiguration Instance { get { return _Instance; } }

        #region Stored Settings
        protected bool? _TestBool;
        [DataMember]
        public bool TestBool {
            get { return (bool)_TestBool; }
            protected set { _TestBool = value; }
        }
        protected string _TestString;
        [DataMember]
        public string TestString {
            get { return _TestString; }
            set { _TestString = value; }
        }
        #endregion

        protected override void Setup()
        {
            _TestBool = _TestBool ?? true;
            _TestString = _TestString ?? "Hello, World!";
        }

        protected GlobalConfiguration():
            base()
        {
            Setup();
        }
        /// <summary>
        /// Load and parse (deserialize) the global configuration from a JSON stream.
        /// </summary>
        /// <param name="stream"></param>
        public static void Load(Stream stream)
        {
            _Instance = (GlobalConfiguration)Serializer.ReadObject(stream);
        }
        public static void Store(Stream stream)
        {
            using(var writer = CreateJsonWriter(stream))
                Serializer.WriteObject(writer, _Instance);
        }
        public static void LoadDefaults()
        {
            _Instance = new GlobalConfiguration();
        }
    }
    #endregion
    #region Configuration
    [DataContract]
    public class Configuration : ConfigurationBase
    {
        new protected static XmlObjectSerializer Serializer =
            new DataContractJsonSerializer(
                typeof(Configuration),
                SerializerSettings);

        #region Stored Settings
        [DataMember] public string ArtworkDirectory{ get; protected set; }
        [DataMember] public string AudioDirectory { get; protected set; }
        [DataMember] public Dictionary<String, Key> Keys { get; protected set; }
        /// <summary>
        /// The acceleration due to gravity to use for most scenes
        /// </summary>
        [DataMember] public float? ForceDueToGravity { get; protected set; }
        /// <summary>
        /// Time in seconds to allow existingly-pressed keystrokes to
        /// retrigger through the bubbling KeysUpdate event.
        /// </summary>
        /// <remarks>
        /// Think of this as "buffer timeframe" in which you can press
        /// a button and have an action happen as soon as it can.
        /// </remarks>
        [DataMember] public float? KeyPressTimeTolerance { get; protected set; }
        /// <summary>
        /// Maximum frame rate to work at
        /// </summary>
        [DataMember] public float? FrameRateCap { get; protected set; }
        /// <summary>
        /// Time in milliseconds to sleep at a time
        /// during the frame rate cap loop
        /// </summary>
        [DataMember] public int? ThreadSleepTimeStep { get; protected set; }
        /// <summary>
        /// Maximum time in seconds that the physics solver can
        /// step through time.
        /// </summary>
        /// <remarks>
        /// High values may risk physics lag whereas
        /// lower values will cause slowness
        /// </remarks>
        [DataMember] public float? MaxWorldTimeStep { get; protected set; }
        /// <summary>
        /// Whether the world time step should adapt to the load
        /// </summary>
        [DataMember] public bool? AdaptiveTimeStep { get; protected set; }
        [DataMember] public int? CanvasWidth { get; protected set; }
        [DataMember] public int? CanvasHeight { get; protected set; }
        #endregion
        
        #region Volatile Settings
        public string AssetsPath { get; protected set; }
        public string ArtworkPath
        {
            get { return Path.Combine(AssetsPath, ArtworkDirectory); }
        }
        public string AudioPath
        {
            get { return Path.Combine(AssetsPath, AudioDirectory); }
        }
        public bool DrawBlueprints { get;  set; }
        public bool ShowDebugVisuals { get;  set; }
        #endregion

        protected override void Setup()
        {
            FindAssetsPath();

            // Load or default
            ArtworkDirectory = ArtworkDirectory ?? "Artwork";
            AudioDirectory = AudioDirectory ?? "Audio";
            ForceDueToGravity = ForceDueToGravity ?? -9.8f;
            KeyPressTimeTolerance = KeyPressTimeTolerance ?? 0.1f;
            FrameRateCap = FrameRateCap ?? 1200.0f;
            ThreadSleepTimeStep = ThreadSleepTimeStep ?? 1;
            MaxWorldTimeStep = MaxWorldTimeStep ?? 0.05f;
            AdaptiveTimeStep = AdaptiveTimeStep ?? false;
            CanvasWidth = CanvasWidth ?? 1280 / 2;
            CanvasHeight = CanvasHeight ?? 800 / 2;

            if(Keys == null)
            {
                Keys = new Dictionary<string, Key>();
                Keys["Up"] = Key.W;
                Keys["Left"] = Key.A;
                Keys["Down"] = Key.S;
                Keys["Right"] = Key.D;
                Keys["Jump"] = Key.F;

                Keys["Reset"] = Key.Number1;
                Keys["ResetModifier"] = Key.Number2;
                Keys["ToggleFullScreen"] = Key.BackSlash;
                Keys["ToggleShowDebugVisuals"] = Key.Semicolon;
                Keys["ToggleDrawBlueprints"] = Key.Quote;
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

        /// <summary>
        /// Deserialize the configuration from a JSON stream.
        /// </summary>
        /// <param name="stream"></param>
        public static Configuration Load(Stream stream)
        {
            Configuration configuration = (Configuration)Serializer.ReadObject(stream);
            configuration.Setup();
            return configuration;
        }
        /// <summary>
        /// Serialize the configuration into a JSON stream
        /// </summary>
        /// <param name="stream"></param>
        public void Store(Stream stream)
        {
            using(var writer = CreateJsonWriter(stream))
                Serializer.WriteObject(writer, this);
        }
        public Configuration ():
            base()
        {
            Setup();
        }
    }
    #endregion
}
