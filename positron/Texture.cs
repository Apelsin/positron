using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class Texture
	{
		#region State
		#region Member Variables
		protected int _TextureID = 0;
		protected String _Label = "Texture";
		protected int _Width;
		protected int _Height;
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
			_DefaultTexture = LoadTexture ("sprite_small_disc", "sprite_small_disc.png");
			LoadTexture ("sprite_four_square", "sprite_four_square.png");
			LoadTexture ("sprite_player", "sprite_cool_guy.png");
			LoadTexture ("sprite_indicator", "sprite_indicator.png");
			LoadTexture ("sprite_indicator_gloss", "sprite_indicator_gloss.png");
			LoadTexture ("sprite_spidey_0", "sprite_spidey_0.png");
			LoadTexture ("sprite_spidey_1", "sprite_spidey_1.png");

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

			// Background
			LoadTexture("sprite_tile_bg_1",			"background", 	"sprite_tile_bg_1.png");
			LoadTexture("sprite_tile_bg_2",			"background", 	"sprite_tile_bg_2.png");
			LoadTexture("sprite_tile_bg_3",			"background", 	"sprite_tile_bg_3.png");

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
			return (Texture)Textures[key];
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
			//GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			//string absolute_file_path = System.IO.Path.GetFullPath (file_path);
			if(!System.IO.File.Exists(file_path))
				throw new FileNotFoundException("File missing: ", file_path);
			Bitmap bitmap = new Bitmap(file_path);
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

