using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public static class TextureHelper
    {
        public static Texture LoadTexture(this PositronGame game, string title, params string[] path_components)
        {
            string[] all_path_components = new string[path_components.Length + 1];
            all_path_components[0] = game.Configuration.ArtworkPathFull;
            path_components.CopyTo(all_path_components, 1);
            string path = Path.Combine(all_path_components);
            return Texture.LoadTextureAbsolute(title, path);
        }
    }
    [DataContract]
    public class Texture
    {
        [DataContract]
        public struct Region
        {
            internal string _Label;
            public string Label
            {
                get { return _Label; }
                set { _Label = value; }
            }
            internal Vector2 _Low, _High, _Size, _Anchor;
            [DataMember]
            public Vector2 Low
            {
                get { return _Low; }
                set
                {
                    _Low = value;
                    _Size = _High - _Low;
                }
            }
            [DataMember]
            public Vector2 High
            {
                get { return _High; }
                set
                {
                    _High = value;
                    _Size = _High - _Low;
                }
            }
            public Vector2 Size
            {
                get { return _Size; }
                set
                {
                    _Size = value;
                    _High = _Low + _Size;
                }
            }
            public Vector2 Anchor { get { return _Anchor; } set { _Anchor = value; } }
            public float AnchorX { get { return _Anchor.X; } set { _Anchor.X = value; } }
            public float AnchorY { get { return _Anchor.Y; } set { _Anchor.Y = value; } }
            public Vector2 Center { get { return 0.5f * (Low + High); } }
            public float CenterX { get { return 0.5f * (Low.X + High.X); } }
            public float CenterY { get { return 0.5f * (Low.Y + High.Y); } }
            public float SizeX { get { return High.X - Low.X; } }
            public float SizeY { get { return High.Y - Low.Y; } }
            public Region(string label, Vector2 low, Vector2 high, Vector2 anchor)
            {
                _Low = low;
                _High = high;
                _Size = _High - _Low;
                _Anchor = anchor;
                _Label = label;
            }
            public Region(string label, Vector2 low, Vector2 high) :
                this(label, low, high, Vector2.Zero)
            {
            }
            public Region(string label, float x0, float y0, float x1, float y1) :
                this(label, new Vector2(x0, y0), new Vector2(x1, y1))
            {
            }
            public override string ToString()
            {
                return string.Format("[Region: Label={0}, Low={1}, High={2}, Size={3}]", Label, Low, High, Size);
            }
        }
        #region State
        #region Instance Fields
        [DataMember]
        public Region[] Regions;
        #endregion
        #region Static Fields

        #endregion
        #region Instance Properties
        public int Id { get; internal set; }
        public String Label { get; internal set; }
        public String FilePath { get; internal set; }
        public int Width { get; internal set; }
        public int Height { get; internal set; }
        public int DefaultRegionIndex { get; set; }
        public Region DefaultRegion { get { return Regions[DefaultRegionIndex]; } }
        #endregion
        #region Static Properties
        public static Texture DefaultTexture { get; internal set; }
        public static Hashtable Textures { get; internal set; }
        #endregion
        #endregion
        #region Behavior
        #region Member
        private Texture(string label, int id, int w, int h, string file_path)
        {
            Label = label;
            Id = id;
            Width = w;
            Height = h;
            FilePath = file_path;
        }
        #endregion
        #region Static
        static Texture()
        {
            Textures = new Hashtable();
        }
        public static void Initialize(string artwork_path)
        {
            string title = "sprite_null";
            string path = Path.Combine(artwork_path, "sprite_null.png");
            DefaultTexture = Texture.LoadTextureAbsolute(title, path);
        }
        public static void Teardown ()
        {
            foreach(DictionaryEntry pair in Textures)
                GL.DeleteTexture(((Texture)pair.Value).Id);
        }
        
        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, Id);
        }
        
        public static void Bind(object key)
        {
            ((Texture)Textures[key]).Bind ();
        }
        
        public static void Bind(Texture t)
        {
            GL.BindTexture(TextureTarget.Texture2D, t.Id);
        }
        public static void Unbind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        public static Texture Get (object key)
        {
            if(Textures.ContainsKey(key))
                return (Texture)Textures[key];
            return DefaultTexture;
        }
        
        internal static Texture LoadTextureAbsolute(string title, string file_path)
        {    
            if(!System.IO.File.Exists(file_path))
                throw new FileNotFoundException("File missing: ", file_path);
            using (Bitmap bitmap = new Bitmap(file_path))
            {
                var texture = LoadTexture(title, bitmap, file_path);
                return texture;
            }
        }
        public static Texture LoadTexture(string title, Bitmap bitmap, string file_path)
        {
            int texture;
            
            // TODO: allow user customization of this portion

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
            
            Texture T = new Texture(title, texture, bitmap.Width, bitmap.Height, file_path);
            Textures[title] = T;
            return T;
        }
        #endregion
        #endregion
    }
}

