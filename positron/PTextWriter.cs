using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	/// <summary>
	/// TextWriter implementation originally by some guy named David
	/// See http://www.opentk.com/node/1554?page=1#comment-10625 for details
	/// </summary>
	public class PTextWriter : IRenderable
	{
		protected Font TextFont = new Font(FontFamily.GenericMonospace, 12);
		protected Bitmap TextBitmap;
		protected List<PointF> Positions;
		protected List<string> Lines;
		protected List<Brush> ColorBrushes;
		protected int TextureID;

		public PTextWriter(Size areaSize)
		{
			Positions = new List<PointF>();
			Lines = new List<string>();
			ColorBrushes = new List<Brush>();
			
			TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            // HACK FIXIE
            Program.MainGame.AddUpdateEventHandler(this, (sender, e) =>
                {
                    TextureID = CreateTexture();
                    return true;
                });
		}
		private int CreateTexture()
		{
			int texture_id;
			//Important, or wrong color on some computers
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);
			Bitmap bitmap = TextBitmap;
			GL.GenTextures(1, out texture_id);
			GL.BindTexture(TextureTarget.Texture2D, texture_id);
			
			BitmapData data = bitmap.LockBits(
				new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
				ImageLockMode.ReadOnly,
				System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
			              OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
			GL.Finish();
			bitmap.UnlockBits(data);
			return texture_id;
		}
		public bool Update(int ind, string newText)
		{
			if (ind < Lines.Count)
			{
				Lines[ind] = newText;
				return UpdateText();
			}
			return false;
		}
		public void Clear()
		{
			Lines.Clear();
			Positions.Clear();
			ColorBrushes.Clear();
		}
		public bool AddLine(string s)
		{
			return AddLine (s, PointF.Empty);
		}
		public bool AddLine(string s, PointF pos)
		{
			return AddLine (s, pos, Brushes.White);
		}
		public bool AddLine(string s, PointF pos, Brush col)
		{
			Lines.Add(s);
			Positions.Add(pos);
			ColorBrushes.Add(col);
			return UpdateText();
		}
		public bool UpdateText()
		{
			bool have_lines = Lines.Count > 0;
			if (have_lines)
			{
				using (Graphics gfx = Graphics.FromImage(TextBitmap))
				{
					gfx.Clear(Color.Transparent);
					gfx.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
					for (int i = 0; i < Lines.Count; i++)
						gfx.DrawString(Lines[i], TextFont, ColorBrushes[i], Positions[i]);
				}
				BitmapData data = TextBitmap.LockBits(
					new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
					ImageLockMode.ReadOnly,
					System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				
				GL.BindTexture(TextureTarget.Texture2D, TextureID);
				GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height,
				                 OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
				
				TextBitmap.UnlockBits(data);
			}
			return have_lines;
		}
		public void Render (double time)
		{
			GL.Color4 (Color.White);
			GL.PushMatrix ();
			{
				GL.BindTexture (TextureTarget.Texture2D, TextureID);
				//GL.Translate (_Position.X, _Position.Y, 0.0);
				GL.Begin (BeginMode.Quads);
				GL.TexCoord2 (0, 0);
				GL.Vertex2 (0, 0);
				GL.TexCoord2 (1, 0);
				GL.Vertex2 (TextBitmap.Width, 0);
				GL.TexCoord2 (1, -1);
				GL.Vertex2 (TextBitmap.Width, TextBitmap.Height);
				GL.TexCoord2 (0, -1);
				GL.Vertex2 (0, TextBitmap.Height);
				GL.End ();
			}
			GL.PopMatrix ();
		}
		public void Dispose()
		{
			if (TextureID > 0)
				GL.DeleteTexture(TextureID);
		}
	}
}

