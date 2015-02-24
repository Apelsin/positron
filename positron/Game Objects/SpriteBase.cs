using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Positron
{
    // TODO: Implement clonable interface maybe
    [DataContract]
    public class SpriteBase : GameObject, IDisposable
    {
        internal override void OnSerializing(StreamingContext context)
        {
            base.OnSerializing(context);
            AnimationCurrentId = AnimationCurrent.ElementId;
        }
        #region SpriteAnimation
        [DataContract]
        public class Animation : ContractElement, IDisposable
        {
            protected static List<Animation> Animations = new List<Animation>();
            [DataMember]
            public string Name { get; set; }
            [DataMember]
            public Frame[] Frames { get; set; }
            public int FrameCount { get { return Frames == null ? 0 : Frames.Length; } }
            [DataMember]
            public bool Looping { get; set; }
            [DataMember]
            public bool PingPong { get; set; }
            // TODO: this is awful. Fix it.
            public Animation(string name, Texture texture, bool looping, bool ping_pong, int? frame_time_n = null, params int[] region_incices)
            {
                int frame_time = frame_time_n ?? Frame.DefaultDuration;

                Looping = looping;
                PingPong = ping_pong;

                // TODO: make this better (supply color and frame time)
                if(region_incices == null || region_incices.Length < 1)
                {
                    Frames = new Frame[1];
                    Frames[0] = new Frame(texture, 0, Color.White, frame_time);
                }
                else
                {
                    Frames = new Frame[region_incices.Length];
                    for(int i = 0; i < region_incices.Length; i++)
                        Frames[i] = new Frame(texture, region_incices[i], Color.White, frame_time);
                }
                Animations.Add(this);
            }
            public Animation(string name, Texture texture, bool looping = true, bool ping_pong = false):
                this(name, texture, looping, ping_pong, null)
            {
                
            }

            public static void Store(PositronGame game)
            {
                var configurator = new Configurator<Animation>(new Type[] { typeof(Frame) });
                foreach (Animation animation in Animations)
                {
                    string path = System.IO.Path.Combine(
                        game.Configuration.AnimationPathFull,
                        animation.ElementId + ".spa");
                    using(var stream = new System.IO.FileStream(path, System.IO.FileMode.Create))
                    {
                        configurator.Store(stream, animation);
                    }
                }
            }

            public virtual void Dispose ()
            {
                if (Frames != null)
                {
                    for (int i = 0; i < Frames.Length; i++)
                    {
                        Frames [i].Dispose ();
                        Frames [i] = null;
                    }
                    Frames = null;
                }
            }
        }
        #endregion
        #region SpriteFrame
        [DataContract]
        public class Frame : ContractElementBase, IDisposable
        {
            internal override void OnSerializing(StreamingContext context)
            {
                base.OnSerializing(context);
                TexturePath = Texture.FilePath;
            }
            [DataMember]
            public Color Color { get; set; }
            public Texture Texture { get; set; }
            [DataMember]
            internal string TexturePath;
            [DataMember]
            public int Duration { get; set; }
            [DataMember]
            public int TextureRegionIndex { get; set; }
            public static readonly int DefaultDuration;
            public float SizeX {
                get { return Texture.Regions == null || Texture.Regions.Length == 0 ?
                    Texture.Width : Texture.Regions [TextureRegionIndex].SizeX; }
            }
            public float SizeY {
                get { return Texture.Regions == null || Texture.Regions.Length == 0 ?
                    Texture.Height : Texture.Regions[TextureRegionIndex].SizeY; }
            }
            public VertexBuffer VBO { get; internal set; }
            public Frame (Texture texture, int idx):
                this(texture, idx, Color.White)
            {
            }
            public Frame (Texture texture, int idx, Color color):
                this(texture, idx, color, DefaultDuration)
            {
            }
            public Frame (Texture texture, int idx, Color color, int duration)
            {
                Texture = texture;
                TextureRegionIndex = idx;
                Color = color;
                Duration = duration;
                Build();
            }
            /// <summary>
            /// Builds the vertex buffer object for this sprite frame
            /// </summary>
            public void Build()
            {
                float w, h, w_half, h_half, x0, y0, x1, y1, xx, yy, corner_x, corner_y;
                if (Texture.Regions != null && Texture.Regions.Length > 0)
                {
                    Texture.Region region = Texture.Regions[TextureRegionIndex];
                    x0 = region.Low.X;
                    y0 = region.Low.Y;
                    x1 = region.High.X;
                    y1 = region.High.Y;
                    w = x1 - x0;
                    h = y1 - y0;
                    xx = x0 + x1;
                    yy = y0 + y1;
                    x0 = (xx - w) * 0.5f;
                    y0 = (yy - h) * 0.5f;
                    x1 = (xx + w) * 0.5f;
                    y1 = (yy + h) * 0.5f;
                    x0 /= Texture.Width;
                    x1 /= Texture.Width;
                    y0 /= Texture.Height;
                    y1 /= Texture.Height;
                    corner_x = region.AnchorX;
                    corner_y = region.AnchorY;
                }
                else
                {
                    w = Texture.Width;
                    h = Texture.Height;
                    x0 = 0.0f;
                    y0 = 0.0f;
                    x1 = w;
                    y1 = h;
                    corner_x = 0;
                    corner_y = 0;
                }
                w_half = w * 0.5f;
                h_half = h * 0.5f;
                var A = new VertexLite(corner_x - w_half, corner_y - h_half, 1.0f, x0, -y0);
                var B = new VertexLite(corner_x + w_half, corner_y - h_half, 1.0f, x1, -y0);
                var C = new VertexLite(corner_x + w_half, corner_y + h_half, 1.0f, x1, -y1);
                var D = new VertexLite(corner_x - w_half, corner_y + h_half, 1.0f, x0, -y1);
                VBO = new VertexBuffer(A, B, C, D);
            }
            public virtual void Dispose ()
            {
                if (VBO != null) {
                    VBO.Dispose ();
                    VBO = null;
                }
                Texture = null;
            }
        }
        #endregion
        #region State
        #region Instance Fields

        protected Stopwatch FrameTimer;
        protected int AnimationFrameIndex;
        protected bool FirstUpdate = true;

        /// <summary>
        /// The blueprint vertex buffer object
        /// </summary>
        //protected VertexBuffer BPVBO;
        #endregion
        #region Instance Properties
        public Animation AnimationDefault { get; internal set; }
        [DataMember]
        internal string AnimationCurrentId;
        public Animation AnimationCurrent { get; internal set; }
        public Lazy<Animation> AnimationNext { get; internal set; }
        public Frame FrameCurrent {
            get { return AnimationCurrent == null ? AnimationDefault.Frames[0] : AnimationCurrent.Frames[AnimationFrameIndex]; }
        }
        
        public int FrameIndex {
            get { return AnimationFrameIndex; }
            set { 
                if(FrameTimer != null)
                    FrameTimer.Restart();
                AnimationFrameIndex = value;
            }
        }
        public float SizeX {
            get { return mTransform.ScaleLocalX * FrameCurrent.SizeX; }
        }
        public float SizeY
        {
            get { return mTransform.ScaleLocalY * FrameCurrent.SizeY; }
        }

        //public VertexBuffer BPVBO { get { return FrameCurrent.BPVBO; } }

        #endregion
        #endregion
        #region Behavior
        public SpriteBase(Xform parent):
            this(parent, 0.0f, 0.0f, 1.0f, 1.0f, Texture.DefaultTexture)
        {
        }
        public SpriteBase(Xform parent, Texture texture) :
            this(parent, 0.0f, 0.0f, 1.0f, 1.0f, texture)
        {
        }
        public SpriteBase(Xform parent, float x, float y) :
            this(parent, x, y, 1.0f, 1.0f, Texture.DefaultTexture)
        {        
        }
        // Main constructor:
        public SpriteBase(Xform parent, float x, float y, Texture texture) :
            this(parent, x, y, 1.0f, 1.0f, texture)
        {
        }
        public SpriteBase(Xform parent, float x, float y, float scalex, float scaley, Texture texture) :
            base(parent)
        {
            mTransform.ScaleLocalX = scalex;
            mTransform.ScaleLocalY = scaley;
            AnimationFrameIndex = 0;
            FrameTimer = new Stopwatch();
            AnimationDefault = AnimationCurrent = new Animation("Default", texture);
        }
        
        public override void Draw()
        {
            GL.Color4(FrameCurrent.Color);
            FrameCurrent.Texture.Bind(); // Bind to (current) sprite texture
            FrameCurrent.VBO.Render(); // Render the vertex buffer object
        }
        public void Build()
        {
            // SpriteFrame handles Build() for frames
        }
        public override void Update ()
        {
            base.Update();
            if (FirstUpdate) {
                FirstUpdate = false;
                if(AnimationCurrent != null)
                {
                    // TODO: is this good for anything?

                    //if(_AnimationCurrent.Sound != null)
                    //    _AnimationCurrent.Sound.Play();
                }
            }
            if (AnimationCurrent != null) {
                if (AnimationFrameIndex < AnimationCurrent.FrameCount) {
                    if (FrameTimer.Elapsed.TotalMilliseconds > FrameCurrent.Duration) {
                        AnimationFrameIndex++;
                        FrameTimer.Restart();
                        if (AnimationFrameIndex >= AnimationCurrent.FrameCount) {
                            if(AnimationCurrent.Looping)
                            {
                                AnimationFrameIndex = 0;
                                FirstUpdate = true;
                            }
                            else
                            {
                                FrameTimer.Stop ();
                                AnimationFrameIndex = AnimationCurrent.FrameCount - 1;
                                if(AnimationNext != null)
                                    PlayAnimation(AnimationNext.Value);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Starts a sprite animation if it is not already playing
        /// </summary>
        public void PlayAnimation(Animation animation)
        {
            if(animation != AnimationCurrent)
                 StartAnimation(animation);
        }
        /// <summary>
        /// Plays a new static animation (oxymoron) with a single frame
        /// using the current animation with the specified region given
        /// the region label
        /// </summary>
        //public SpriteBase SetRegion (string region_label)
        //{
        //    PlayAnimation(new Animation("region:" + region_label, FrameCurrent.Texture));
        //    return this;
        //}
        public void StartAnimation (Animation animation)
        {
            if (animation == null) {
                if (AnimationNext != null)
                    animation = AnimationNext.Value;
            }
            if (animation != null) {
                FirstUpdate = true;
                AnimationFrameIndex = 0;
                AnimationCurrent = animation;
                FrameTimer.Restart();
            }
        }
        public virtual void Dispose()
        {
            // TODO
        }
        #endregion
    }
}

