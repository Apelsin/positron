using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
    public struct TextureRegion
    {
		private string _Label;
		private Vector2d _Low, _High;
        
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
			gateway.Regions = new TextureRegion[4];
			gateway.Regions.BuildTiledRegions(4, 32, 72);

			var extender_platform = PsdLoader.LoadSpriteSheet("sprite_extender_platform", "scene_element", "sprite_extender_platform.psd");

			// Enemies
			var robot_1 = PsdLoader.LoadSpriteSheet("sprite_robot_1", "baddie", "sprite_robot_1.psd");

			// Props
			LoadTexture ("sprite_metal_ball",		"prop",				"sprite_metal_ball.png");
			
			// Projectiles
			LoadTexture ("sprite_first_bullet", 	"projectile",		"sprite_first_bullet.png");
			
			var bunker_floor = LoadTexture("sprite_bunker_floor",		"scene_element",   "sprite_bunker_floor.png");
			bunker_floor.Regions = new TextureRegion[4];
			bunker_floor.Regions.BuildTiledRegions(4, 32, 32);
			
			var bunker_floor_2 = LoadTexture("sprite_bunker_floor_2",		"scene_element",   "sprite_bunker_floor_2.png");
			bunker_floor_2.Regions = new TextureRegion[4];
			bunker_floor_2.Regions.BuildTiledRegions(4, 32, 128);
			
			var bunker_wall = LoadTexture("sprite_bunker_wall",		"scene_element",   "sprite_bunker_wall.png");
			bunker_wall.Regions = new TextureRegion[4];
			bunker_wall.Regions.BuildTiledRegions(1, 16, 32);
			
			// Character
			var character = PsdLoader.LoadSpriteSheet ("sprite_player", "character", "sprite_protagonist.psd");

			// Dialog box
			LoadTexture ("sprite_dialog_box_tl", 	"dialog_box",	"sprite_dialog_box_tl.png");
			LoadTexture ("sprite_dialog_box_t",		"dialog_box",	"sprite_dialog_box_t.png");
			LoadTexture ("sprite_dialog_box_tr", 	"dialog_box",	"sprite_dialog_box_tr.png");
			LoadTexture ("sprite_dialog_box_l", 	"dialog_box",	"sprite_dialog_box_l.png");
			LoadTexture ("sprite_dialog_box_c", 	"dialog_box",	"sprite_dialog_box_c_transparent.png");
			LoadTexture ("sprite_dialog_box_r", 	"dialog_box",	"sprite_dialog_box_r.png");
			LoadTexture ("sprite_dialog_box_bl", 	"dialog_box",	"sprite_dialog_box_bl.png");
			LoadTexture ("sprite_dialog_box_b", 	"dialog_box",	"sprite_dialog_box_b.png");
			LoadTexture ("sprite_dialog_box_br", 	"dialog_box",	"sprite_dialog_box_br.png");
			
			// User interface
			var health_meter = LoadTexture ("sprite_health_meter_atlas",	"user_interface",	"sprite_health_meter_atlas.png");
			health_meter.Regions = new TextureRegion[18];
			health_meter.Regions.BuildTiledRegions(6, 32, 32);
			
			var main_menu_buttons = LoadTexture ("sprite_main_menu_buttons",	"user_interface", "sprite_main_menu_buttons.png");
			main_menu_buttons.Regions = new TextureRegion[6];
			main_menu_buttons.Regions.BuildTiledRegions(2, 128, 32);
			
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
			
			var floor = LoadTexture("sprite_tile_floor_atlas",		"background",   "sprite_tile_floor_atlas.png");
			floor.Regions = new TextureRegion[3];
			floor.Regions.BuildTiledRegions(4, 32, 32);
			
			
			var infogfx_key_spacebar = LoadTexture ("sprite_infogfx_key_spacebar",	"user_interface",	"sprite_infogfx_key_spacebar.png");
			var infogfx_key_w =	 LoadTexture ("sprite_infogfx_key_w",	"user_interface",	"sprite_infogfx_key_w.png");
			var infogfx_key_a =	 LoadTexture ("sprite_infogfx_key_a",	"user_interface",	"sprite_infogfx_key_a.png");
			var infogfx_key_s =	 LoadTexture ("sprite_infogfx_key_s",	"user_interface",	"sprite_infogfx_key_s.png");
			var infogfx_key_d =	 LoadTexture ("sprite_infogfx_key_d",	"user_interface",	"sprite_infogfx_key_d.png");
			var infogfx_key_e =	 LoadTexture ("sprite_infogfx_key_e",	"user_interface",	"sprite_infogfx_key_e.png");
			var infogfx_key_f =	 LoadTexture ("sprite_infogfx_key_f",	"user_interface",	"sprite_infogfx_key_f.png");
			
			
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
			Bitmap bitmap = new Bitmap(file_path);
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

