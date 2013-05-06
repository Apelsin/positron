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
            protected Sound _Sound;

			public SpriteFrame[] Frames { get { return _Frames; } }
			public int FrameCount { get { return _Frames == null ? 0 : _Frames.Length; } }
			public bool Looping { get { return _Looping; } set { _Looping = value; } }
			public bool PingPong { get { return _PingPong; } set { _PingPong = value; } }
            public Sound Sound { get { return _Sound; } set { _Sound = value; } }


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
			public SpriteAnimation(Texture texture, int frame_time, bool looping, bool ping_pong, params string[] region_labels) :
				this(texture, frame_time, looping, ping_pong, texture.Regions.Labeled(region_labels))
			{
			}
			public SpriteAnimation(Texture texture, int frame_time, bool looping, params string[] region_labels) :
				this(texture, frame_time, looping, false, region_labels)
			{
			}
//			public SpriteAnimation(Texture texture, int frame_time, params string[] region_labels) :
//				this(texture, frame_time, false, region_labels)
//			{
//			}
			public SpriteAnimation(Texture texture, bool looping, bool ping_pong, params string[] region_labels) :
				this(texture, _FrameTimeDefault, looping, ping_pong, region_labels)
			{
			}
			public SpriteAnimation(Texture texture, bool looping, params string[] region_labels) :
				this(texture, looping, false, region_labels)
			{
			}
			public SpriteAnimation(Texture texture, params string[] region_labels) :
				this(texture, false, region_labels)
			{
			}

			public SpriteAnimation(Texture texture, bool looping, bool ping_pong, params int[] region_incices) :
				this(texture, _FrameTimeDefault, looping, ping_pong, region_incices)
			{
			}
			public SpriteAnimation(Texture texture, bool looping, params int[] region_incices):
				this(texture, looping, false, region_incices)
			{
			}
			/// <summary>
			/// Initializes a new instance of the <see cref="positron.SpriteBase+SpriteAnimation"/> class.
			/// If region_indices is empty, frame_time instead defines first frame and default frame time is used,
			/// otherwise frame time is the first integer parameter followed by texture region indices for frames.
			/// </summary>
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
			//protected VertexBuffer _BPVBO;
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
				get { return _Texture.Regions == null || _Texture.Regions.Length == 0 ?
                    _Texture.Width : _Texture.Regions [_TextureRegionIndex].SizeX; }
			}
			public double SizeY {
                get { return _Texture.Regions == null || _Texture.Regions.Length == 0 ?
                    _Texture.Height : _Texture.Regions[_TextureRegionIndex].SizeY; }
			}
			public VertexBuffer VBO { get { return _VBO; } }
			//public VertexBuffer BPVBO { get { return _BPVBO; } }
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
				double w, h, w_half, h_half, x0, y0, x1, y1, xx, yy, corner_x, corner_y;
				if (_Texture.Regions != null && _Texture.Regions.Length > 0)
				{
					TextureRegion region = _Texture.Regions[_TextureRegionIndex];
					x0 = region.Low.X;
					y0 = region.Low.Y;
					x1 = region.High.X;
					y1 = region.High.Y;
					w = x1 - x0;
					h = y1 - y0;
					xx = x0 + x1;
					yy = y0 + y1;
					x0 = (xx - w * _TileX) * 0.5;
					y0 = (yy - h * _TileY) * 0.5;
					x1 = (xx + w * _TileX) * 0.5;
					y1 = (yy + h * _TileY) * 0.5;
					x0 /= _Texture.Width;
					x1 /= _Texture.Width;
					y0 /= _Texture.Height;
					y1 /= _Texture.Height;
					corner_x = region.OriginOffsetX;
					corner_y = region.OriginOffsetY;
				}
				else
				{
					w = _Texture.Width;
					h = _Texture.Height;
					x0 = 0.0;
					y0 = 0.0;
					x1 = _TileX;
					y1 = _TileY;
					corner_x = 0;
					corner_y = 0;
				}
				w_half = w * 0.5;
				h_half = h * 0.5;
				var A = new Vertex(corner_x - w_half, corner_y - h_half, 1.0, x0, -y0);
				var B = new Vertex(corner_x + w_half, corner_y - h_half, 1.0, x1, -y0);
				var C = new Vertex(corner_x + w_half, corner_y + h_half, 1.0, x1, -y1);
				var D = new Vertex(corner_x - w_half, corner_y + h_half, 1.0, x0, -y1);
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

		//protected Dictionary<string, SpriteAnimation> _Animations;
		protected Stopwatch _FrameTimer;
		protected int _AnimationFrameIndex;

		protected SpriteAnimation _AnimationDefault;
		protected SpriteAnimation _AnimationCurrent;
		protected Lazy<SpriteAnimation> _AnimationNext;

		protected SpriteFrame _FrameStatic;

        protected bool _FirstUpdate = false;

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
		public Lazy<SpriteAnimation> AnimationNext {
			get { return _AnimationNext; }
		}
		public SpriteFrame FrameCurrent {
			get { return _AnimationCurrent == null ? _FrameStatic : _AnimationCurrent.Frames[_AnimationFrameIndex]; }
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
			set { _TileX = value; }
		}
		public double TileY {
			get { return _TileY; }
			set { _TileY = value; }
		}
		public override double SizeX {
			get { return _Scale.X * FrameCurrent.SizeX; }
		}
        public override double SizeY
        {
			get { return _Scale.Y * FrameCurrent.SizeY; }
		}

		public VertexBuffer VBO { get { return FrameCurrent.VBO; } }
		//public VertexBuffer BPVBO { get { return FrameCurrent.BPVBO; } }

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
			_TileX = 1.0;
			_TileY = 1.0;
			_AnimationFrameIndex = 0;
			_FrameTimer = new Stopwatch();
			_AnimationDefault = _AnimationCurrent = new SpriteAnimation(texture, 0);
			_FrameStatic = _AnimationDefault.Frames[0];
            
			// Position for world objects is handled differently
			if(!(this is IWorldObject))
				Corner = new Vector3d(x, y, 0.0);
		}
		public SpriteBase CenterShift ()
		{
			PositionX -= FrameCurrent.SizeX * 0.5;
			PositionY -= FrameCurrent.SizeY * 0.5;
			return this;
		}
		public override void Render (double time)
		{
			GL.PushMatrix();
			{
				GL.Translate (_Position + CalculateMovementParallax());
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
            if (Configuration.DrawBlueprints /*&& BPVBO != null*/)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind
				//BPVBO.Render(); // Render blueprint objects
				if(_Blueprints != null)
					foreach(IRenderable r in _Blueprints)
						r.Render(0.0);
            }
        }
        public override void Build()
        {
			// SpriteFrame handles Build() for frames
		}
		public virtual void Update (double time)
        {
            if (_FirstUpdate) {
                _FirstUpdate = false;
                if(_AnimationCurrent != null)
                {
                    if(_AnimationCurrent.Sound != null)
                        _AnimationCurrent.Sound.Play();
                }
            }
			if (_AnimationCurrent != null) {
				if (_AnimationFrameIndex < _AnimationCurrent.FrameCount) {
					if (_FrameTimer.Elapsed.TotalMilliseconds > FrameCurrent.FrameTime) {
						_AnimationFrameIndex++;
						_FrameTimer.Restart();
						if (_AnimationFrameIndex >= _AnimationCurrent.FrameCount) {
							if(_AnimationCurrent.Looping)
							{
								_AnimationFrameIndex = 0;
                                _FirstUpdate = true;
							}
							else
							{
								_FrameTimer.Stop ();
								_AnimationFrameIndex = _AnimationCurrent.FrameCount - 1;
								if(_AnimationNext != null)
									PlayAnimation(_AnimationNext.Value);
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
        public void LoopSound__HACK(object sound_key)
        {
            Sound sound = Sound.Get (sound_key);
            var sound_anim = new SpriteAnimation(Texture, (int)(1000 * sound.Duration),  true, false, Texture.DefaultRegionIndex);
            PlayAnimation(sound_anim);
        }
        /// <summary>
        /// Plays a new static animation (oxymoron) with a single frame
        /// using the current animation with the specified region given
        /// the region label
        /// </summary>
        public SpriteBase SetRegion (string region_label)
        {
            PlayAnimation(new SpriteAnimation(Texture, region_label));
            return this;
        }
		protected void StartAnimation (SpriteAnimation animation)
		{
			if (animation == null) {
				if (_AnimationNext != null)
					animation = _AnimationNext.Value;
			}
			if (animation != null) {
                _FirstUpdate = true;
				_AnimationFrameIndex = 0;
				_AnimationCurrent = animation;
				_FrameTimer.Restart();
			}
		}
		public override void Dispose()
		{
			//_RenderSet.Remove(this);
			VBO.Dispose();
			base.Dispose();
		}
		#endregion
	}
}

