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
		private static Hashtable Textures;
		#endregion
		#region Member Accessors
		public int TextureID { get { return _TextureID; } }
		public String Label { get { return Label; } }
		public int Width { get { return _Width; } }
		public int Height { get { return _Height; } }
		#endregion
		#region Static Accessors
		// Nobody here but us chickens!
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
		public static void Setup ()
		{
			Textures = new Hashtable();
			LoadTexture ("sprite_four_square", "sprite_four_square.png");
			LoadTexture ("sprite_small_disc", "sprite_small_disc.png");
		}
		public static void Teardown ()
		{
			foreach(DictionaryEntry pair in Textures)
				GL.DeleteTexture(((Texture)pair.Value).TextureID);
		}

		public static void Bind(object key)
		{
			GL.BindTexture(TextureTarget.Texture2D, ((Texture)Textures[key]).TextureID);
		}

		public static void Bind(Texture t)
		{
			GL.BindTexture(TextureTarget.Texture2D, t.TextureID);
		}

		public static Texture Get (object key)
		{
			return (Texture)Textures[key];
		}

		public static void LoadTexture(string title, string file_name)
		{
			string path = Path.Combine(Configuration.ArtworkPath, file_name);
			LoadTextureAbsolute(title, path);
		}
		protected static void LoadTextureAbsolute(string title, string file_path)
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

			Textures[title] = new Texture(title, texture, bitmap.Width, bitmap.Height);

		}
		#endregion
		#endregion
	}
}

