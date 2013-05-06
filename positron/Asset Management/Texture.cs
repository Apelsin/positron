using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public class TextureRegion
    {
		private string _Label;
		private Vector2d _Low, _High, _OriginOffset = Vector2d.Zero;

		public TextureRegion(Vector2d l, Vector2d h):
			this("Region", l, h)
		{
		}
		public TextureRegion(double x, double y, double w, double h):
			this("Region", x, y, w, h)
		{
		}
		public TextureRegion(string label, double x, double y, double w, double h)
		{
			_Label = label;
			_Low = new Vector2d(x, y);
			_High = _Low + new Vector2d(w, h);
		}
		public TextureRegion(string label, Vector2d l, Vector2d h)
		{
			_Label = label;
			_Low = l;
			_High = h;
		}
		public string Label { get { return _Label; } set { _Label = value; } }
		public Vector2d Low { get { return _Low; } set { _Low = value; } }
		public Vector2d High { get { return _High; } set { _High = value; } }
		public Vector2d OriginOffset { get { return _OriginOffset; } set { _OriginOffset = value; } }
		public double OriginOffsetX { get { return _OriginOffset.X; } set { _OriginOffset.X = value; } }
		public double OriginOffsetY { get { return _OriginOffset.Y; } set { _OriginOffset.Y = value; } }
		public Vector2d Center { get { return 0.5 * (_Low + _High); } }
		public double CenterX { get { return 0.5 * (_Low.X + _High.X); } }
		public double CenterY { get { return 0.5 * (_Low.Y + _High.Y); } }
        public Vector2d Size { get { return _High - _Low; } }
		public double SizeX { get { return _High.X - _Low.X; } }
		public double SizeY { get { return _High.Y -  _Low.Y; } }
		public override string ToString ()
		{
			return string.Format ("[TextureRegion: Label={0}, Low={1}, High={2}, Size={3}]", Label, Low, High, Size);
		}
    }
	public class Texture
	{
		#region State
		#region Member Variables
		protected int _TextureID = 0;
		protected String _Label = "Texture";
		protected int _Width;
		protected int _Height;
		public TextureRegion[] Regions;
		protected int _DefaultRegionIndex = 0;
		#endregion
		#region Static Variables
		private static Texture _DefaultTexture;
		private static Hashtable Textures;
		#endregion
		#region Member Accessors
		public int TextureID { get { return _TextureID; } }
		public String Label { get { return Label; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		public int DefaultRegionIndex {
			get { return _DefaultRegionIndex; }
			set { _DefaultRegionIndex = value; }
		}
		public TextureRegion DefaultRegion { get { return Regions[_DefaultRegionIndex]; } }
		#endregion
		#region Static Accessors
		public static Texture DefaultTexture { get { return _DefaultTexture; } }
		#endregion
		#endregion
		#region Behavior
		#region Member
		private Texture(string label, int id, int w, int h)
		{
			_Label = label;
			_TextureID = id;
			_Width = w;
			_Height = h;
		}
#endregion
		#region Static
		public static void InitialSetup ()
		{
			Textures = new Hashtable();
			_DefaultTexture = LoadTexture ("sprite_null", "sprite_null.png");
			LoadTexture ("sprite_four_square", "sprite_four_square.png");
			LoadTexture ("sprite_indicator", "sprite_indicator.png");
			LoadTexture ("sprite_indicator_gloss", "sprite_indicator_gloss.png");
			LoadTexture ("sprite_spidey_0", "sprite_spidey_0.png");
			LoadTexture ("sprite_spidey_1", "sprite_spidey_1.png");
			
			// Scene Elements
			LoadTexture ("sprite_doorway", 			"scene_element",	"sprite_doorway.png");
			var floor_switch = LoadTexture ("sprite_floor_switch", 	"scene_element",	"sprite_floor_switch.png");
			floor_switch.Regions = new TextureRegion[4];
			floor_switch.Regions.BuildTiledRegions(4, 32, 10);
			var gateway = LoadTexture ("sprite_gateway",	 		"scene_element",	"sprite_gateway.png");
			PsdLoader.LoadSpriteSheet("sprite_mini_gate",			"scene_element",	"sprite_mini_gate.psd");
			gateway.Regions = new TextureRegion[4];
			gateway.Regions.BuildTiledRegions(4, 32, 72);

			PsdLoader.LoadSpriteSheet("sprite_projectile_switch",	"scene_element",	"sprite_projectile_switch.psd");

			var extender_platform = PsdLoader.LoadSpriteSheet("sprite_extender_platform", "scene_element", "sprite_extender_platform.psd");

			// Enemies
			var robot_1 = PsdLoader.LoadSpriteSheet("sprite_robot_1", "baddie", "sprite_robot_1.psd");

			// Props
			LoadTexture ("sprite_metal_ball",		    "prop",				"sprite_metal_ball.png");
            PsdLoader.LoadSpriteSheet ("sprite_radio",  "prop",             "sprite_radio.psd");
			
			// Projectiles
			LoadTexture ("sprite_first_bullet", 	"projectile",		"sprite_first_bullet.png");
            PsdLoader.LoadSpriteSheet("sprite_bullet_collision_particle",   "projectile",   "sprite_bullet_collision_particle.psd");
			
			var bunker_floor = LoadTexture("sprite_bunker_floor",		"scene_element",   "sprite_bunker_floor.png");
			bunker_floor.Regions = new TextureRegion[4];
			bunker_floor.Regions.BuildTiledRegions(4, 32, 32);

			var floor = LoadTexture("sprite_tile_floor_atlas",		"background",   "sprite_tile_floor_atlas.png");
			floor.Regions = new TextureRegion[3];
			floor.Regions.BuildTiledRegions(4, 32, 32);

			var bunker_floor_2 = LoadTexture("sprite_bunker_floor_2",		"scene_element",   "sprite_bunker_floor_2.png");
			bunker_floor_2.Regions = new TextureRegion[4];
			bunker_floor_2.Regions.BuildTiledRegions(4, 32, 104);
			
			var bunker_wall = LoadTexture("sprite_bunker_wall",		"scene_element",   "sprite_bunker_wall.png");
			bunker_wall.Regions = new TextureRegion[4];
			bunker_wall.Regions.BuildTiledRegions(1, 16, 32);
			
			// Character
			PsdLoader.LoadSpriteSheet ("sprite_player", "character",	"sprite_protagonist.psd");
			LoadTexture("sprite_protagonist_picture", 	"character",	"sprite_protagonist_picture.png");

			// Dialog box
			LoadTexture ("sprite_dialog_fade_up",	"dialog_box",	"sprite_dialog_fade_up.png");

			// User interface
			var health_meter = LoadTexture ("sprite_health_meter_atlas",	"user_interface",	"sprite_health_meter_atlas.png");
			health_meter.Regions = new TextureRegion[18];
			health_meter.Regions.BuildTiledRegions(6, 32, 32);
			
			var main_menu_buttons = LoadTexture ("sprite_main_menu_buttons",	"user_interface", "sprite_main_menu_buttons.png");
			main_menu_buttons.Regions = new TextureRegion[6];
			main_menu_buttons.Regions.BuildTiledRegions(2, 128, 32);

			var android_now = PsdLoader.LoadSpriteSheet("sprite_android_now", "sprite_android_now.psd");
			
			// Background
			// TODO: make this not-hardcoded
			var bg = LoadTexture("sprite_tile_bg_atlas",		"background",   "sprite_tile_bg_atlas.png");
			bg.Regions = new TextureRegion[32];
			bg.Regions.BuildTiledRegions(4, 32, 32);
			
			var bg2 = LoadTexture("sprite_tile_bg2_atlas",		"background",   "sprite_tile_bg2_atlas.png");
			bg2.Regions = new TextureRegion[32];
			bg2.Regions.BuildTiledRegions(4, 32, 32);
			
			var bg3 = LoadTexture("sprite_tile_bg3_atlas",		"background",   "sprite_tile_bg3_atlas.png");
			bg3.Regions = new TextureRegion[32];
			bg3.Regions.BuildTiledRegions(4, 32, 32);

            var bg4 = LoadTexture("sprite_tile_bg4_atlas",      "background",   "sprite_tile_bg4_atlas.png");
            bg4.Regions = new TextureRegion[64];
            bg4.Regions.BuildTiledRegions(8, 32, 32);

			var bg_rubble = LoadTexture("sprite_dark_rubble_atlas",		"background",   "sprite_dark_rubble_atlas.png");
			bg_rubble.Regions = new TextureRegion[32];
			bg_rubble.Regions.BuildTiledRegions(4, 32, 32);

			var bg_pipes = LoadTexture("sprite_bg_pipes", "background", "sprite_bg_pipes.png");
			
            var infogfx_cabinet = PsdLoader.LoadSpriteSheet("sprite_infogfx_cabinet_buttons", "user_interface", "sprite_infogfx_cabinet_buttons.psd");
			
			// TEST PSDLOADER
			PsdLoader.LoadSpriteSheet("sprite_dumbo", "sprite_dumbo.psd");
			
		}
		public static void Teardown ()
		{
			foreach(DictionaryEntry pair in Textures)
				GL.DeleteTexture(((Texture)pair.Value).TextureID);
		}
		
		public void Bind()
		{
			GL.BindTexture(TextureTarget.Texture2D, TextureID);
		}
		
		public static void Bind(object key)
		{
			((Texture)Textures[key]).Bind ();
		}
		
		public static void Bind(Texture t)
		{
			GL.BindTexture(TextureTarget.Texture2D, t.TextureID);
		}
		
		public static Texture Get (object key)
		{
			if(Textures.ContainsKey(key))
				return (Texture)Textures[key];
			return _DefaultTexture;
		}
		public static Texture LoadTexture(string title, params string[] path_components)
		{
			string[] all_path_components = new string[path_components.Length + 1];
			all_path_components[0] = Configuration.ArtworkPath;
			path_components.CopyTo(all_path_components, 1);
			string path = Path.Combine(all_path_components);
			return LoadTextureAbsolute(title, path);
		}
		protected static Texture LoadTextureAbsolute(string title, string file_path)
		{	
			if(!System.IO.File.Exists(file_path))
				throw new FileNotFoundException("File missing: ", file_path);
			using (Bitmap bitmap = new Bitmap(file_path))
				return LoadTexture(title, bitmap);
		}
		public static Texture LoadTexture(string title, Bitmap bitmap)
		{
			int texture;
			
			GL.GenTextures(1, out texture);
			GL.BindTexture(TextureTarget.Texture2D, texture);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			
			BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
			                                  ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
			              OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			
			GL.BindTexture(TextureTarget.Texture2D, 0);
			
			bitmap.UnlockBits(data);
			
			Texture T = new Texture(title, texture, bitmap.Width, bitmap.Height);
			Textures[title] = T;
			return T;
		}
		#endregion
		#endregion
	}
}

