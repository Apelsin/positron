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
	// TODO: Implement clonable interface maybe
	public class SpriteBase : Drawable, IColorable
	{
		#region SpriteAnimation
		public class SpriteAnimation
		{
            protected static int _FrameTimeDefault = 200;
			protected SpriteFrame[] _Frames;
			protected bool _Looping = false;
			protected bool _PingPong = false;

			public SpriteFrame[] Frames { get { return _Frames; } }
			public int FrameCount { get { return _Frames == null ? 0 : _Frames.Length; } }
			public bool Looping { get { return _Looping; } set { _Looping = value; } }
			public bool PingPong { get { return _PingPong; } set { _PingPong = value; } }


			public SpriteAnimation(Texture texture, int frame_time, bool looping, bool ping_pong, params int[] region_incices):
				this(looping, ping_pong)
			{
				// TODO: make this better (supply color and frame time)
				if(region_incices == null || region_incices.Length < 1)
				{
					_Frames = new SpriteFrame[1];
					_Frames[0] = new SpriteFrame(texture, 0, Color.White, frame_time);
				}
				else
				{
					_Frames = new SpriteFrame[region_incices.Length];
					for(int i = 0; i < region_incices.Length; i++)
						_Frames[i] = new SpriteFrame(texture, region_incices[i], Color.White, frame_time);
				}
			}
			public SpriteAnimation(Texture texture, params int[] region_incices):
				this(texture, false, region_incices)
			{
			}
			public SpriteAnimation(Texture texture, bool looping, params int[] region_incices):
				this(texture, looping, false, region_incices)
			{
			}
            public SpriteAnimation(Texture texture, bool looping, bool ping_pong, params int[] region_incices) :
                this(texture, _FrameTimeDefault, looping, ping_pong, region_incices)
            {
            }
            public SpriteAnimation(Texture texture, int frame_time, params int[] region_incices) :
                this(texture,
                region_incices != null && region_incices.Length > 0 ? frame_time : _FrameTimeDefault,
                false, false,
                region_incices != null && region_incices.Length > 0 ? region_incices : new int[1] { frame_time })
            {
            }
			public SpriteAnimation(bool looping, bool ping_pong, params SpriteFrame[] frames):
				this(looping, ping_pong)
			{
				_Frames = frames;
			}
			private SpriteAnimation(bool looping, bool ping_pong)
			{
				_Looping = looping;
				_PingPong = ping_pong;
			}
		}
		#endregion
		#region SpriteFrame
		// Like a struct
		public class SpriteFrame
		{
			protected Color _Color;
			protected Texture _Texture;
			protected int _FrameTime;
			protected int _TextureRegionIndex;
			protected VertexBuffer _VBO;
			protected VertexBuffer _BPVBO;
			protected double _TileX;
			protected double _TileY;
			public Color Color {
				get { return _Color; }
				set { _Color = value; }
			}
			public Texture Texture {
				get { return _Texture; }
				set { _Texture = value; }
			}
			public double TileX {
				get { return _TileX; }
				set { _TileX = value; }
			}
			public double TileY {
				get { return _TileY; }
				set { _TileY = value; }
			}
			public int FrameTime {
				get { return _FrameTime; }
				set { _FrameTime = value; }
			}
			public int TextureRegionIndex {
				get { return _TextureRegionIndex; }
			}
			public double SizeX {
				get { return _Texture.Regions [_TextureRegionIndex].SizeX; }
			}
			public double SizeY {
				get { return _Texture.Regions [_TextureRegionIndex].SizeY; }
			}
			public VertexBuffer VBO { get { return _VBO; } }
			public VertexBuffer BPVBO { get { return _BPVBO; } }
			public SpriteFrame (Texture texture, int idx):
				this(texture, idx, Color.White)
			{
			}
			public SpriteFrame (Texture texture, int idx, Color color):
				this(texture, idx, color, 100)
			{
			}
			public SpriteFrame (Texture texture, int idx, Color color, int frame_time)
			{
				_Texture = texture;
				_TextureRegionIndex = idx;
				_Color = color;
				_FrameTime = frame_time;
				_TileX = 1.0;
				_TileY = 1.0;
				Build();
			}
			public void Build()
			{
				double w, h, w_half, h_half, x0, y0, x1, y1, xs, ys;
				if (_Texture.Regions != null && _Texture.Regions.Length > 0)
				{
					x0 = _Texture.Regions[_TextureRegionIndex].Low.X;
					y0 = _Texture.Regions[_TextureRegionIndex].Low.Y;
					x1 = _Texture.Regions[_TextureRegionIndex].High.X;
					y1 = _Texture.Regions[_TextureRegionIndex].High.Y;
					w = x1 - x0;
					h = y1 - y0;
					xs = x0 + x1;
					ys = y0 + y1;
					x0 = (xs - w * _TileX) * 0.5;
					y0 = (ys - h * _TileY) * 0.5;
					x1 = (xs + w * _TileX) * 0.5;
					y1 = (ys + h * _TileY) * 0.5;
					x0 /= _Texture.Width;
					x1 /= _Texture.Width;
					y0 /= _Texture.Height;
					y1 /= _Texture.Height;
				}
				else
				{
					w = _Texture.Width;
					h = _Texture.Height;
					x0 = 0.0;
					y0 = 0.0;
					x1 = _TileX;
					y1 = _TileY;
				}
				w_half = w * 0.5;
				h_half = h * 0.5;
				var A = new Vertex(-w_half, -h_half, 0.0, 0.0, 0.0, 1.0, x0, -y0);
				var B = new Vertex(w_half, -h_half, 0.0, 0.0, 0.0, 1.0, x1, -y0);
				var C = new Vertex(w_half, h_half, 0.0, 0.0, 0.0, 1.0, x1, -y1);
				var D = new Vertex(-w_half, h_half, 0.0, 0.0, 0.0, 1.0, x0, -y1);
				_VBO = new VertexBuffer(A, B, C, D);
				//BPVBO = new VertexBuffer(A, B, C, D);
			}
		}
		#endregion
		#region State
		#region Member Variables
		protected Color _Color;
		protected double _TileX;
		protected double _TileY;

		protected Dictionary<string, SpriteAnimation> _Animations;
		protected Stopwatch _FrameTimer;
		protected int _AnimationFrameIndex;

		protected SpriteAnimation _AnimationDefault;
		protected SpriteAnimation _AnimationCurrent;
		protected SpriteAnimation _AnimationNext;
        /// <summary>
        /// The blueprint vertex buffer object
        /// </summary>
        //protected VertexBuffer BPVBO;
		#endregion
		#region Member Accessors
		public SpriteAnimation AnimationDefault {
			get { return _AnimationDefault; }
		}
		public SpriteAnimation AnimationCurrent {
			get { return _AnimationCurrent; }
		}
		public SpriteAnimation AnimationNext {
			get { return _AnimationNext; }
		}
		public SpriteFrame FrameCurrent {
			get { return _AnimationCurrent.Frames[_AnimationFrameIndex]; }
		}
//		public bool Animate {
//			get { return _FrameTimer != null && _FrameTimer.IsRunning; }
//			set {
//				if(_FrameTimer != null)
//				{
//					if(value)
//						_FrameTimer.Restart();
//					else
//						_FrameTimer.Stop();
//				}
//			}
//		}
		public int FrameIndex {
			get { return _AnimationFrameIndex; }
			set { 
				if(_FrameTimer != null)
					_FrameTimer.Restart();
				_AnimationFrameIndex = value;
			}
		}
		public Color Color {
			get { return FrameCurrent.Color; }
			set { FrameCurrent.Color = value; }
		}
		public Texture Texture {
			get { return FrameCurrent.Texture; }
			//set { FrameCurrent.Texture = value; }
		}
		public double TileX {
			get { return _TileX; }
			//set { _TileX = value; }
		}
		public double TileY {
			get { return _TileY; }
			//set { _TileY = value; }
		}
		public double SizeX {
			get { return _Scale.X * FrameCurrent.SizeX; }
		}
		public double SizeY {
			get { return _Scale.Y * FrameCurrent.SizeY; }
		}

		public VertexBuffer VBO { get { return FrameCurrent.VBO; } }
		public VertexBuffer BPVBO { get { return FrameCurrent.BPVBO; } }

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
		public SpriteBase (RenderSet render_set, double x, double y):
			this(render_set, x, y, 1.0, 1.0, Texture.DefaultTexture)
		{		
		}
		// Main constructor:
		public SpriteBase (RenderSet render_set, double x, double y, Texture texture):
			this(render_set, x, y, 1.0, 1.0, texture)
		{
		}
		public SpriteBase (RenderSet render_set, double x, double y, double scalex, double scaley, Texture texture):
			base(render_set)
		{
			// Size will scale _Texture width and height
			_Color = Color.White;
			_Scale.X = scalex;
			_Scale.Y = scaley;
			_Position.X = x;
			_Position.Y = y;
			_TileX = 1.0;
			_TileY = 1.0;
			_AnimationFrameIndex = 0;
			_FrameTimer = new Stopwatch();
			_AnimationDefault = _AnimationCurrent = new SpriteAnimation(texture, 0);
		}
		// TODO: rotation stuff here
		public override double RenderSizeX () { return _Scale.X * Texture.Width; }
		public override double RenderSizeY () { return _Scale.Y * Texture.Height; }
		public override void Render (double time)
		{
			GL.PushMatrix();
			{
				GL.Translate (_Position);
                //GL.Translate(Math.Floor (Position.X), Math.Floor (Position.Y), Math.Floor (Position.Z));
				GL.Rotate(_Theta, 0.0, 0.0, 1.0);
                GL.Scale(_Scale);
                Draw();
            }
			GL.PopMatrix();
		}
        protected virtual void Draw()
        {
			// Handle invalidation here:
			if(_TileX != FrameCurrent.TileX)
			{
				FrameCurrent.TileX = _TileX;
				FrameCurrent.Build(); // Invalidation
			}
            GL.Color4(_Color);
            Texture.Bind(); // Bind to (current) sprite texture
            VBO.Render(); // Render the vertex buffer object
            if (Configuration.DrawBlueprints && BPVBO != null)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind
				BPVBO.Render(); // Render blueprint objects
            }
        }
        public override void Build()
        {
			// SpriteFrame handles Build();
		}
		public virtual void Update (double time)
		{
			if (_AnimationCurrent != null) {
				if (_AnimationFrameIndex < _AnimationCurrent.FrameCount) {
					if (_FrameTimer.Elapsed.TotalMilliseconds > FrameCurrent.FrameTime) {
						_AnimationFrameIndex++;
						_FrameTimer.Restart();
						if (_AnimationFrameIndex >= _AnimationCurrent.FrameCount) {
							if(_AnimationCurrent.Looping)
							{
								_AnimationFrameIndex = 0;
							}
							else
							{
								_FrameTimer.Stop ();
								_AnimationFrameIndex = _AnimationCurrent.FrameCount - 1;
								PlayAnimation(_AnimationNext);
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// Starts a sprite animation if it is not already playing
		/// </summary>
		protected void PlayAnimation(SpriteAnimation animation)
		{
			if(animation != _AnimationCurrent)
				StartAnimation(animation);
		}
		protected void StartAnimation(SpriteAnimation animation)
		{
			if(animation == null)
				animation = _AnimationNext;
			if (animation != null) {
				_AnimationFrameIndex = 0;
				_AnimationCurrent = animation;
				_FrameTimer.Restart();
			}
		}
		#endregion
	}
}

