using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public class TextureRegion
    {
        private string _Label;
        private Vector2 _Low, _High, _OriginOffset = Vector2.Zero;

        public TextureRegion(Vector2 l, Vector2 h):
            this("Region", l, h)
        {
        }
        public TextureRegion(float x, float y, float w, float h):
            this("Region", x, y, w, h)
        {
        }
        public TextureRegion(string label, float x, float y, float w, float h)
        {
            _Label = label;
            _Low = new Vector2(x, y);
            _High = _Low + new Vector2(w, h);
        }
        public TextureRegion(string label, Vector2 l, Vector2 h)
        {
            _Label = label;
            _Low = l;
            _High = h;
        }
        public string Label { get { return _Label; } set { _Label = value; } }
        public Vector2 Low { get { return _Low; } set { _Low = value; } }
        public Vector2 High { get { return _High; } set { _High = value; } }
        public Vector2 OriginOffset { get { return _OriginOffset; } set { _OriginOffset = value; } }
        public float OriginOffsetX { get { return _OriginOffset.X; } set { _OriginOffset.X = value; } }
        public float OriginOffsetY { get { return _OriginOffset.Y; } set { _OriginOffset.Y = value; } }
        public Vector2 Center { get { return 0.5f * (_Low + _High); } }
        public float CenterX { get { return 0.5f * (_Low.X + _High.X); } }
        public float CenterY { get { return 0.5f * (_Low.Y + _High.Y); } }
        public Vector2 Size { get { return _High - _Low; } }
        public float SizeX { get { return _High.X - _Low.X; } }
        public float SizeY { get { return _High.Y -  _Low.Y; } }
        public override string ToString ()
        {
            return string.Format ("[TextureRegion: Label={0}, Low={1}, High={2}, Size={3}]", Label, Low, High, Size);
        }
    }
    public static class TextureHelper
    {
        public static Texture LoadTexture(this PositronGame game, string title, params string[] path_components)
        {
            string[] all_path_components = new string[path_components.Length + 1];
            all_path_components[0] = game.Configuration.ArtworkPath;
            path_components.CopyTo(all_path_components, 1);
            string path = Path.Combine(all_path_components);
            return Texture.LoadTextureAbsolute(title, path);
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
        static Texture()
        {
            Textures = new Hashtable();
        }
        public static void Initialize(string artwork_path)
        {
            string title = "sprite_null";
            string path = Path.Combine(artwork_path, "sprite_null.png");
            _DefaultTexture = Texture.LoadTextureAbsolute(title, path);
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
        
        internal static Texture LoadTextureAbsolute(string title, string file_path)
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

