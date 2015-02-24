using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Positron.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace Positron
{
    #region Event-related
    public class KeysUpdateEventArgs : EventArgs
    {
        public OrderedDictionary KeysPressedWhen;
        public KeysUpdateEventArgs(OrderedDictionary keys_and_times)
        {
            KeysPressedWhen = keys_and_times;
        }
    }
    public delegate void KeysUpdateEventHandler(object sender, KeysUpdateEventArgs e);
    #endregion
    public class ThreadedRendering : GameWindow
    {
        #region State
        #region Member
        protected PositronGame _Game;
        /// <summary>
        /// GL handle for canvas FBO texture
        /// </summary>
        uint CanvasTexture;
        /// <summary>
        /// GL handle for canvas FBO
        /// </summary>
        uint CanvasFBO;
        /// <summary>
        /// Width of the viewport in pixels
        /// </summary>
        int ViewportWidth;
        /// <summary>
        /// Height of the viewport in pixels
        /// </summary>
        int ViewportHeight;
        int TargetWidth;
        int TargetHeight;
        /// <summary>
        /// Width scale of FBO textured quad
        /// </summary>
        float FBOScaleX;
        /// <summary>
        /// Height scale of FBO textured quad
        /// </summary>
        float FBOScaleY;

        /// <summary>
        /// Ordered dictionary containing the current keyboard keys being pressed in time order
        /// </summary>
        public OrderedDictionary KeysPressed = new OrderedDictionary();
        /// <summary>
        /// Signifies that the main game needs to be destroyed and then reinstantiated / reinitialized
        /// </summary>
        bool Reset = false;
        /// <summary>
        /// Signifies that the program is exiting
        /// </summary>
        bool Exiting = false;
        /// <summary>
        /// Main OpenGL-calling thread for rendering
        /// </summary>
        Thread RenderThread;

        /// <summary>
        /// The timer for render time
        /// </summary>
        Stopwatch RenderWatch = new Stopwatch();
        /// <summary>
        /// The timer for update time
        /// </summary>
        Stopwatch UpdateWatch = new Stopwatch();
        /// <summary>
        /// The timer for late update time
        /// </summary>
        Stopwatch LateUpdateWatch = new Stopwatch();
        /// <summary>
        /// The timer for each frame (i.e. render and updates)
        /// </summary>
        Stopwatch FrameWatch = new Stopwatch();

        Stopwatch RenderDrawingWatch = new Stopwatch();

        Stopwatch TestWatch = new Stopwatch();

        float _LastFrameTime,
            _LastUpdateTime,
            _LastRenderTime,
            _LastLateUpdateTime,
            _LastRenderDrawingTime;

        /// <summary>
        /// Lock to synchronize update
        /// </summary>
        public readonly object UpdateLock = new object();

        /// <summary>
        /// Lock to synchronize render
        /// </summary>
        public readonly object RenderLock = new object();

        /// <summary>
        /// Lock to synchronize post-render update
        /// </summary>
        public readonly object LateUpdateLock = new object();

        #endregion
        #region Accessors
        /// <summary>
        /// The current <see cref="PositronGame"/> for this window
        /// </summary>
        public PositronGame Game { get { return _Game; }
            set {
                _Game = value;
                Width = CanvasWidth;
                Height = CanvasHeight;
            }
        }
        /// <summary>
        /// Alias for Game.Configuration
        /// </summary>
        protected GameConfiguration Configuration { get { return _Game.Configuration; } }
        /// <summary>
        /// Width of canvas in pixels
        /// </summary>
        public int CanvasWidth { get { return (int)Configuration.CanvasWidth; } }
        /// <summary>
        /// Height of canvas in pixels
        /// </summary>
        public int CanvasHeight { get { return (int)Configuration.CanvasHeight; } }
        float FrameLimitTime { get { return 1.0f / (float)Configuration.FrameRateCap; } }
        public float LastFrameTime { get { return _LastFrameTime; } }
        public float LastUpdateTime { get { return _LastUpdateTime; } }
        public float LastRenderTime { get { return _LastRenderTime; } }
        public float LastLateUpdateTime { get { return _LastLateUpdateTime; } }
        public float LastRenderDrawingTime { get { return _LastRenderDrawingTime; } }

        #endregion
        #endregion
        #region Event
        public event KeysUpdateEventHandler KeysUpdate;
        protected virtual void OnKeysUpdate()
        {
            KeysUpdateEventArgs args = new KeysUpdateEventArgs(KeysPressed);
            if (KeysUpdate != null)
            {
                KeysUpdate(this, args);
            }
            IInputAccepter[] accepters = _Game.InputAccepterGroup;
            for (int i = 0; i < accepters.Length; i++)
                accepters[i].KeysUpdate(this, args); // Trickle down arguments
        }
        #endregion

        public ThreadedRendering()
            : base()
        {
            Keyboard.KeyDown += HandleKeyDown;
            Keyboard.KeyUp += HandleKeyUp;
            Resize += HandleResize;
        }

        #region Event Handlers
        void HandleResize(object sender, EventArgs e)
        {
            // Note that we cannot call any OpenGL methods directly. What we can do is set
            // a flag and respond to it from the rendering thread.
            lock (RenderLock)
            {
                int canvas_width = CanvasWidth;
                int canvas_height = CanvasHeight;
                TargetWidth = Math.Max(Width, canvas_width);
                TargetHeight = Math.Max(Height, canvas_height);
                // TODO: figure out window minimum dimensions (Win 8.1 is picky)
                ViewportWidth = TargetWidth;
                ViewportHeight = TargetHeight;
                int multi = ViewportWidth / canvas_width;
                multi = Math.Min(multi, ViewportHeight / canvas_height);
                FBOScaleX = multi * canvas_width;
                FBOScaleY = multi * canvas_height;
            }
        }

        protected bool KeyMatch(Key key, string name)
        {
            Key named_key;
            return Configuration.KeyMap.TryGetValue(name, out named_key) ? key == named_key : false;
        }
        protected bool KeyIsPressed(string name)
        {
            Key named_key;
            return Configuration.KeyMap.TryGetValue(name, out named_key) ? KeysPressed.Contains(named_key) : false;
        }
        void HandleKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            KeyEventArgs key_args = new KeyEventArgs();
            key_args.Key = e.Key;
            if (_Game == null)
                return;
            if (KeyMatch(key_args.Key, "ToggleFullScreen"))
            {
                if (_Game != null)
                {
                    if (this.WindowState == WindowState.Fullscreen)
                        this.WindowState = WindowState.Normal;
                    else
                        this.WindowState = WindowState.Fullscreen;
                    OnResize(null);
                }
            }
            IInputAccepter[] accepters = _Game.InputAccepterGroup;
            bool key_press = true;
            if (_Game != null)
            {
                for (int i = 0; i < accepters.Length; i++)
                    key_press = key_press && accepters[i].KeyUp(this, key_args);
                if (key_press && KeysPressed.Contains(key_args.Key))
                    KeysPressed.Remove(key_args.Key);
            }
        }
        #endregion

        void HandleKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            KeyEventArgs key_args = new KeyEventArgs();
            key_args.Key = e.Key;
            if (KeyMatch(key_args.Key, "Reset"))
            {
                if (KeyIsPressed("ResetModifier"))
                {
                    this.Exit();
                }
                else
                {
                    Reset = true;
                }
            }
            // Hard-coded classic Alt-F4 because we're cool
            else if (key_args.Key == Key.F4)
            {
                if (KeysPressed.Contains(Key.AltLeft) || KeysPressed.Contains(Key.AltRight))
                {
                    this.Exit();
                }
            }
            else if (KeyIsPressed("ToggleDrawBlueprints"))
            {
                Configuration.DrawBlueprints ^= true;
            }
            else if (KeyIsPressed("ToggleShowDebugVisuals"))
            {
                Configuration.ShowDebugVisuals ^= true;
            }
            if (_Game != null)
            {
                IInputAccepter[] accepters = _Game.InputAccepterGroup;
                bool key_press = true;
                for (int i = 0; i < accepters.Length; i++)
                    key_press = key_press && accepters[i].KeyDown(this, key_args);
                if (key_press)
                {
                    if (KeysPressed.Contains(key_args.Key))
                        KeysPressed.Remove(key_args.Key);
                    else
                        KeysPressed.Add(key_args.Key, DateTime.Now);
                }
            }
        }

        #region OnLoad

        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!GL.GetString(StringName.Extensions).Contains("GL_EXT_framebuffer_object"))
            {
                throw new NotSupportedException(
                    "GL_EXT_framebuffer_object extension is required. Please update your drivers.");
            }

            FBOSafety(); // Make sure things won't explode!

            // Texture init
            GL.Enable(EnableCap.Texture2D);

            // VBO init
            GL.EnableClientState(ArrayCap.VertexArray);
            //GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            // Blending init
            GL.Enable(EnableCap.Blend);
            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One);

            // Depth buffer init
            GL.Enable(EnableCap.DepthTest);
            GL.ClearDepth(1.0);
            GL.DepthFunc(DepthFunction.Lequal);

            // Culling init
            //GL.Enable (EnableCap.CullFace);

            // Create Color Tex
            GL.GenTextures(1, out CanvasTexture);
            GL.BindTexture(TextureTarget.Texture2D, CanvasTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, CanvasWidth, CanvasHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );

            // Create a FBO and attach the textures
            GL.GenFramebuffers(1, out CanvasFBO);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, CanvasFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, CanvasTexture, 0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Context.SwapInterval = 1;

            Context.MakeCurrent(null); // Release the OpenGL context so it can be used on the new thread.

            RenderThread = new Thread(RenderLoop);
            RenderThread.Start();
        }

        #endregion

        #region OnUnload

        /// <summary>
        /// Release resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnUnload(EventArgs e)
        {
            Exiting = true; // Set a flag that the rendering thread should stop running.
            RenderThread.Join();

            MakeCurrent();

            // Delete textures from graphics memory space
            Texture.Teardown();
            Sound.Teardown();

            // Clean up what we allocated before exiting
            if (CanvasTexture != 0)
                GL.DeleteTextures(1, ref CanvasTexture);

            if (CanvasFBO != 0)
                GL.Ext.DeleteFramebuffers(1, ref CanvasFBO);
            base.OnUnload(e);
        }

        #endregion

        #region FBO Error check
        protected void FBOSafety()
        {
            switch (GL.Ext.CheckFramebufferStatus(FramebufferTarget.FramebufferExt))
            {
                case FramebufferErrorCode.FramebufferCompleteExt:
                    {
                        Console.WriteLine("FBO: The framebuffer is complete and valid for rendering.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteAttachmentExt:
                    {
                        Console.WriteLine("FBO: One or more attachment points are not framebuffer attachment complete. This could mean there’s no texture attached or the format isn’t renderable. For color textures this means the base format must be RGB or RGBA and for depth textures it must be a DEPTH_COMPONENT format. Other causes of this error are that the width or height is zero or the z-offset is out of range in case of render to volume.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachmentExt:
                    {
                        Console.WriteLine("FBO: There are no attachments.");
                        break;
                    }
                /* case  FramebufferErrorCode.GL_FRAMEBUFFER_INCOMPLETE_DUPLICATE_ATTACHMENT_EXT: 
                     {
                         Console.WriteLine("FBO: An object has been attached to more than one attachment point.");
                         break;
                     }*/
                case FramebufferErrorCode.FramebufferIncompleteDimensionsExt:
                    {
                        Console.WriteLine("FBO: Attachments are of different size. All attachments must have the same width and height.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteFormatsExt:
                    {
                        Console.WriteLine("FBO: The color attachments have different format. All color attachments must have the same format.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteDrawBufferExt:
                    {
                        Console.WriteLine("FBO: An attachment point referenced by GL.DrawBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferIncompleteReadBufferExt:
                    {
                        Console.WriteLine("FBO: The attachment point referenced by GL.ReadBuffers() doesn’t have an attachment.");
                        break;
                    }
                case FramebufferErrorCode.FramebufferUnsupportedExt:
                    {
                        Console.WriteLine("FBO: This particular FBO configuration is not supported by the implementation.");
                        break;
                    }
                default:
                    {
                        Console.WriteLine("FBO: Status unknown. (yes, this is really bad.)");
                        break;
                    }
            }

            // using FBO might have changed states, e.g. the FBO might not support stereoscopic views or double buffering
            int[] queryinfo = new int[6];
            GL.GetInteger(GetPName.MaxColorAttachmentsExt, out queryinfo[0]);
            GL.GetInteger(GetPName.AuxBuffers, out queryinfo[1]);
            GL.GetInteger(GetPName.MaxDrawBuffers, out queryinfo[2]);
            GL.GetInteger(GetPName.Stereo, out queryinfo[3]);
            GL.GetInteger(GetPName.Samples, out queryinfo[4]);
            GL.GetInteger(GetPName.Doublebuffer, out queryinfo[5]);
            //Console.WriteLine("max. ColorBuffers: " + queryinfo[0] + " max. AuxBuffers: " + queryinfo[1] + " max. DrawBuffers: " + queryinfo[2] +
            //                  "\nStereo: " + queryinfo[3] + " Samples: " + queryinfo[4] + " DoubleBuffer: " + queryinfo[5]);

            //Console.WriteLine("Last GL Error: " + GL.GetError());
        }


        #endregion

        #region OnUpdateFrame

        /// <summary>
        /// Add your game logic here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            // Nothing to do!
        }

        #endregion

        #region OnRenderFrame

        /// <summary>
        /// Ignored. All rendering is performed on our own rendering function.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Nothing to do. Release the CPU to other threads.
            Thread.Sleep(1);
        }

        #endregion

        #region RenderLoop
        void RenderLoop()
        {
            MakeCurrent(); // The context now belongs to this thread. No other thread may use it!

            while (!Exiting)
            {
                // Since we don't use OpenTK's timing mechanism, we need to keep time ourselves;
                UpdateWatch.Start();
                RenderWatch.Start();
                LateUpdateWatch.Start();
                FrameWatch.Start();
                TestWatch.Start();
                RenderDrawingWatch.Start();

                while(!Reset && !Exiting)
                {
                    FrameWatch.Restart();

                    // Store this in a local variable because accessors have overhead!
                    // Bear with me...this will get a bit tricky to explain precisely...

                    // Sleep the thread for the most milliseconds less than the frame limit time
                    float frame_limit_time = FrameLimitTime;
                    float time_until = (float)frame_limit_time - (float)Configuration.ThreadSleepTimeStep * 0.001f;
                    while (FrameWatch.Elapsed.TotalSeconds < time_until)
                        Thread.Sleep((int)Configuration.ThreadSleepTimeStep);
                    // Hard-loop the remainder
                    while (FrameWatch.Elapsed.TotalSeconds < frame_limit_time) ;

                    UpdateWatch.Restart();
                    lock (UpdateLock)
                        Update();
                    _LastUpdateTime = (float)UpdateWatch.Elapsed.TotalSeconds;

                    RenderWatch.Restart();
                    lock (RenderLock)
                        RenderView();
                    _LastRenderTime = (float)RenderWatch.Elapsed.TotalSeconds;

                    LateUpdateWatch.Restart();
                    lock (LateUpdateLock)
                        LateUpdate();
                    _LastLateUpdateTime = (float)LateUpdateWatch.Elapsed.TotalSeconds;
                    // TODO: Make sure FPS above 60 perform correctly
                    SwapBuffers();
                    _LastFrameTime = (float)UpdateWatch.Elapsed.TotalSeconds;
                }
                //Sound.KillTheNoise (); // I don't know where a better place for this could be...
                Reset = false;
            }
            Context.MakeCurrent(null);
        }

        #endregion

        #region Update

        void Update()
        {
            OnKeysUpdate();
            _Game.Update();
        }

        void LateUpdate()
        {
            _Game.LateUpdate();
        }
        #endregion

        #region Render

        /// <summary>
        /// This is our main rendering function, which executes on the rendering thread.
        /// </summary>
        public void RenderView()
        {
            #region Render Frame Buffer

            int canvas_width = CanvasWidth;
            int canvas_height = CanvasHeight;

            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, CanvasFBO); // Bind canvas FBO
            GL.Viewport(0, 0, canvas_width, canvas_height);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // Camera
            Matrix4 perspective_camera = Matrix4.CreatePerspectiveFieldOfView(
               _Game.CurrentCamera.FieldOfView,
               (float)canvas_width / (float)canvas_height,
               0.0001f,
               9999.0f);

            // TODO: figure out how to do this step in ModelView matrix mode instead
            Matrix4 cam_inv = Game.CurrentCamera.mTransform._Global.Inverted();

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective_camera);
            GL.MultMatrix(ref cam_inv);
            // Render
            GL.MatrixMode(MatrixMode.Modelview);
            RenderDrawingWatch.Restart();
            _Game.Render();
            _LastRenderDrawingTime = (float)RenderDrawingWatch.Elapsed.TotalSeconds;
            GL.LoadIdentity();
            GL.Color4(1.0, 1.0, 1.0, 1.0);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // unbind FBO
            #endregion

            #region Render Main Viewport
            GL.BindTexture(TextureTarget.Texture2D, CanvasTexture); // Bind the canvas
            GL.Viewport(0, 0, ViewportWidth, ViewportHeight);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Do clear
            Matrix4 projection = Matrix4.CreateOrthographic(ViewportWidth, ViewportHeight, -128, 128);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Translate(FBOScaleX / -2.0, FBOScaleY / -2.0, 0);
            GL.Begin(PrimitiveType.Quads);
            GL.TexCoord2(0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.TexCoord2(1.0f, 0.0f);
            GL.Vertex3(FBOScaleX, 0.0f, 0.0);
            GL.TexCoord2(1.0f, 1.0);
            GL.Vertex3(FBOScaleX, FBOScaleY, 0.0f);
            GL.TexCoord2(0.0f, 1.0f);
            GL.Vertex3(0.0f, FBOScaleY, 0.0f);
            GL.End();
            GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind

            //GL.Flush();

            #endregion
        }

    }
        #endregion
}