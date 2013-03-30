using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace positron
{
	// Like a struct
	public class SpriteFrame : IColorable
	{
		protected Color _Color;
		protected Texture _Texture;
		protected int _FrameTime;
		public Color Color {
			get { return _Color; }
			set { _Color = value; }
		}
		public Texture Texture {
			get { return _Texture; }
			set { _Texture = value; }
		}
		public int FrameTime {
			get { return _FrameTime; }
			set { _FrameTime = value; }
		}
		public SpriteFrame (Texture texture)
		{
			_Texture = texture;
			_Color = Color.White;
			_FrameTime = 100;
		}
		public SpriteFrame (Texture texture, Color color)
		{
			_Texture = texture;
			_Color = color;
			_FrameTime = 100;
		}
		public SpriteFrame (Texture texture, Color color, int frame_time)
		{
			_Texture = texture;
			_Color = color;
			_FrameTime = frame_time;
		}
	}
	// TODO: Implement clonable interface maybe
	public class SpriteBase : Drawable, IColorable
	{
		#region State
		#region Member Variables
		protected Color _Color;
		protected SpriteFrame[] _Frames;
		protected int _FrameIndex;
		protected Stopwatch _FrameTimer;
		protected double _TileX;
		protected double _TileY;
        /// <summary>
        /// The blueprint vertex buffer object
        /// </summary>
        protected VertexBuffer BPVBO;
		#endregion
		#region Member Accessors
		public Color Color {
            get { return _Frames[_FrameIndex].Color; }
            set { _Frames[_FrameIndex].Color = value; }
		}
		public Texture Texture {
			get { return _Frames[_FrameIndex].Texture; }
			set { _Frames[_FrameIndex].Texture = value; }
		}
		public bool Animate {
			get { return _FrameTimer != null && _FrameTimer.IsRunning; }
			set {
				if(_FrameTimer != null)
				{
					if(value)
						_FrameTimer.Restart();
					else
						_FrameTimer.Stop();
				}
			}
		}
		public int FrameIndex {
			get { return _FrameIndex; }
			set { 
				// If the frame timer exists and is running, restart it
				if(Animate)
					_FrameTimer.Restart();
				_FrameIndex = value;
			}
		}
		public double TileX {
			get { return _TileX; }
			set { _TileX = value; }
		}
		public double TileY {
			get { return _TileY; }
			set { _TileY = value; }
		}
		#endregion
		#endregion
		#region Behavior
		public SpriteBase(RenderSet render_set):
			this(render_set, 0.0, 0.0, 1.0, 1.0, Texture.DefaultTexture)
		{
		}
		public SpriteBase(RenderSet render_set, Texture texture):
			this(render_set, 0.0, 0.0, 1.0, 1.0, texture)
		{
		}
		public SpriteBase (RenderSet render_set, double scalex, double scaley):
			this(render_set, 0.0, 0.0, scalex, scaley, Texture.DefaultTexture)
		{		
		}
		// Main constructor:
		public SpriteBase (RenderSet render_set, double scalex, double scaley, Texture texture):
			this(render_set, 0.0, 0.0, scalex, scaley, texture)
		{
		}
		public SpriteBase (RenderSet render_set, double x, double y, double scalex, double scaley, Texture texture, params Texture[] textures):
			base(render_set)
		{
			// Size will scale _Texture width and height
			_Color = Color.White;
			_Size.X = scalex;
			_Size.Y = scaley;
			_Position.X = x;
			_Position.Y = y;
			_TileX = 1.0;
			_TileY = 1.0;
			_FrameIndex = 0;
			_Frames = new SpriteFrame[textures.Length + 1];
			_Frames[0] = new SpriteFrame(texture);
			for(int i = 0; i < textures.Length; i++)
				_Frames[i + 1] = new SpriteFrame(textures[i]);
			if(textures.Length > 0)
				_FrameTimer = new Stopwatch();
            Build();
		}
		// TODO: rotation stuff here
		public override double RenderSizeX () { return _Size.X * Texture.Width; }
		public override double RenderSizeY () { return _Size.Y * Texture.Height; }
		public override void Render (double time)
		{
			GL.PushMatrix();
			{
				GL.Translate (_Position);
                //GL.Translate(Math.Floor (Position.X), Math.Floor (Position.Y), Math.Floor (Position.Z));
				GL.Rotate(_Theta, 0.0, 0.0, 1.0);
                GL.Scale(_Size);
                Draw();
            }
			GL.PopMatrix();
		}
        protected virtual void Draw()
        {
            GL.Color4(_Color);
            Texture.Bind(); // Bind to (current) sprite texture
            VBO.Render(); // Render the vertex buffer object
            if (Configuration.DrawBlueprints)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind
                BPVBO.Render(); // Render blueprint objects
            }
        }
        public override void Build()
        {
            double w, h, x0, y0, x1, y1;
            if (Texture.Regions != null && Texture.Regions.Length > 0)
            {
                x0 = Texture.Regions[0].Low.X;
                y0 = Texture.Regions[0].Low.Y;
                x1 = Texture.Regions[0].High.X;
                y1 = Texture.Regions[0].High.Y;
                w = x1 - x0;
                h = y1 - y0;
                x0 /= Texture.Width;
                x1 /= Texture.Width;
                y0 /= Texture.Height;
                y1 /= Texture.Height;
            }
            else
            {
                w = Texture.Width;
                h = Texture.Height;
                x0 = 0.0;
                y0 = 0.0;
                x1 = 1.0;
                y1 = 1.0;
            }
            var A = new Vertex(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, x0, -y0);
            var B = new Vertex(w, 0.0, 0.0, 0.0, 0.0, 1.0, x1, -y0);
            var C = new Vertex(w, h, 0.0, 0.0, 0.0, 1.0, x1, -y1);
            var D = new Vertex(0.0, h, 0.0, 0.0, 0.0, 1.0, x0, -y1);
            VBO = new VertexBuffer(A, B, C, D);
		}
		public virtual void Update (double time)
		{
			// This is really bad but it will do for now:
			// TODO: make this better
			if (_FrameTimer != null && _FrameTimer.IsRunning)
			{
				if((int)_FrameTimer.Elapsed.TotalMilliseconds > _Frames[_FrameIndex].FrameTime)
				{
					_FrameIndex = (_FrameIndex + 1) % _Frames.Length;
					_FrameTimer.Restart();
				}
			}
		}
		#endregion
	}
}

