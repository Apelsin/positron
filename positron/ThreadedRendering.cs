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
        public float Time { get; set; }
        public KeysUpdateEventArgs(OrderedDictionary keys_and_times, float time)
        {
            KeysPressedWhen = keys_and_times;
            Time = time;
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
        /// Width of canvas in pixels
        /// </summary>
        int _CanvasWidth { get { return Configuration.CanvasWidth; } }
        /// <summary>
        /// Height of canvas in pixels
        /// </summary>
        int _CanvasHeight { get { return Configuration.CanvasHeight; } }
        /// <summary>
        /// Width of the viewport in pixels
        /// </summary>
        int ViewportWidth;
        /// <summary>
        /// Height of the viewport in pixels
        /// </summary>
        int ViewportHeight;
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
        Thread _RenderingThread;
        /// <summary>
        /// Main updating thread
        /// </summary>
        Thread _UpdatingThread;

        /// <summary>
        /// The timer for render time
        /// </summary>
        Stopwatch RenderWatch = new Stopwatch();
        /// <summary>
        /// The timer for update time
        /// </summary>
        Stopwatch UpdateWatch = new Stopwatch();
        /// <summary>
        /// The timer for each frame (i.e. render and update)
        /// </summary>
        Stopwatch FrameWatch = new Stopwatch();

        Stopwatch RenderDrawingWatch = new Stopwatch();

        Stopwatch TestWatch = new Stopwatch();

        float FrameLimitTime = 1.0f / Configuration.FrameRateCap;

        float _LastFrameTime, _LastUpdateTime, _LastRenderTime;
        float _LastRenderDrawingTime;

        #endregion
        #region Accessors
        public PositronGame Game { get { return _Game; } set { _Game = value; } }
        /// <summary>
        /// Width of canvas in pixels
        /// </summary>
        public int CanvasWidth {
            get { return _CanvasWidth; }
        }
        /// <summary>
        /// Height of canvas in pixels
        /// </summary>
        public int CanvasHeight {
            get { return _CanvasHeight; }
        }

        public float LastFrameTime { get { return _LastFrameTime; } }
        public float LastUpdateTime { get { return _LastUpdateTime; } }
        public float LastRenderTime { get { return _LastRenderTime; } }

        public float LastRenderDrawingTime { get { return _LastRenderDrawingTime; } }

        public Thread RenderingThread { get { return _RenderingThread; } }
        public Thread UpdatingThread { get { return _UpdatingThread; } }

        #endregion
        #endregion
        #region Event
        public event KeysUpdateEventHandler KeysUpdate;
        protected virtual void OnKeysUpdate (float time)
        {
            lock(_Game.UpdateLock)
            {
                KeysUpdateEventArgs args = new KeysUpdateEventArgs (KeysPressed, time);
                if (KeysUpdate != null)
                {
                    KeysUpdate(this, args);
                }
                IInputAccepter[] accepters = _Game.InputAccepterGroup;
                for(int i = 0; i < accepters.Length; i++)
                    accepters[i].KeysUpdate(this, args); // Trickle down arguments
            }
        }
        #endregion
        
        public ThreadedRendering ()
            : base()
        {
            //lock (_Game.UpdateLock)
            {
                this.Width = _CanvasWidth;
                this.Height = _CanvasHeight;
                this.WindowState = OpenTK.WindowState.Fullscreen; // Hard-coded quik-fix
            }
            Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
            {
                KeyEventArgs key_args = new KeyEventArgs();
                key_args.Key = e.Key;
                if (key_args.Key == Configuration.KeyReset)
                {
                    if(KeysPressed.Contains (Configuration.KeyResetModifier))
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
                else if (key_args.Key == Configuration.KeyToggleDrawBlueprints)
                {
                    Configuration.DrawBlueprints ^= true;
                }
                else if (key_args.Key == Configuration.KeyToggleShowDebugVisuals)
                {
                    Configuration.ShowDebugVisuals ^= true;
                }
                if(_Game != null)
                {
                    lock(_Game.UpdateLock)
                    {
                        IInputAccepter[] accepters = _Game.InputAccepterGroup;
                        bool key_press = true;
                        for(int i = 0; i < accepters.Length; i++)
                            key_press = key_press && accepters[i].KeyDown(this, key_args);
                        if (key_press)
                        {
                            if(KeysPressed.Contains(key_args.Key))
                                KeysPressed.Remove(key_args.Key);
                            else
                                KeysPressed.Add(key_args.Key, DateTime.Now);
                        }
                    }
                }
                
            };
            Keyboard.KeyUp += delegate(object sender, KeyboardKeyEventArgs e)
            {
                KeyEventArgs key_args = new KeyEventArgs();
                key_args.Key = e.Key;
                if(_Game == null)
                    return;
                lock(_Game.UpdateLock)
                {
                    if (key_args.Key == Configuration.KeyToggleFullScreen)
                    {
                        if(_Game != null)
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
                    if(_Game != null)
                    {
                        for(int i = 0; i < accepters.Length; i++)
                            key_press = key_press && accepters[i].KeyUp(this, key_args);
                        if (key_press && KeysPressed.Contains(key_args.Key))
                            KeysPressed.Remove(key_args.Key);
                    }
                }
            };
            Resize += delegate(object sender, EventArgs e)
            {
                // Note that we cannot call any OpenGL methods directly. What we can do is set
                // a flag and respond to it from the rendering thread.
                var _lock = _Game == null ? e : _Game.UpdateLock;
                lock (_lock)
                {
                    Width = Math.Max (Width, _CanvasWidth);
                    Height = Math.Max (Height, _CanvasHeight);
                    ViewportWidth = Width;
                    ViewportHeight = Height;
                    int multi = ViewportWidth / _CanvasWidth;
                    multi = Math.Min(multi, ViewportHeight / _CanvasHeight);
                    FBOScaleX = multi * _CanvasWidth;
                    FBOScaleY = multi * _CanvasHeight;
                }
            };
        }
        
        #region OnLoad
        
        /// <summary>
        /// Setup OpenGL and load resources here.
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad (EventArgs e)
        {
            base.OnLoad (e);
            
            if (!GL.GetString (StringName.Extensions).Contains ("GL_EXT_framebuffer_object")) {
                throw new NotSupportedException (
                    "GL_EXT_framebuffer_object extension is required. Please update your drivers.");
            }

            FBOSafety (); // Make sure things won't explode!

            // Texture init
            GL.Enable(EnableCap.Texture2D);

            // VBO init
            GL.EnableClientState(ArrayCap.VertexArray);
            //GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);

            // Blending init
            GL.Enable (EnableCap.Blend);
            GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.One);

            // Depth buffer init
            GL.Enable (EnableCap.DepthTest);
            GL.ClearDepth (1.0);
            GL.DepthFunc (DepthFunction.Lequal);

            // Culling init
            //GL.Enable (EnableCap.CullFace);

            // Create Color Tex
            GL.GenTextures (1, out CanvasTexture);
            GL.BindTexture (TextureTarget.Texture2D, CanvasTexture);
            GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _CanvasWidth, _CanvasHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            // GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );
            
            // Create a FBO and attach the textures
            GL.GenFramebuffers (1, out CanvasFBO);
            GL.BindFramebuffer (FramebufferTarget.Framebuffer, CanvasFBO);
            GL.FramebufferTexture2D (FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, CanvasTexture, 0);

            GL.BindTexture (TextureTarget.Texture2D, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Context.SwapInterval = 1;

            Context.MakeCurrent (null); // Release the OpenGL context so it can be used on the new thread.
            //lock (_Game.UpdateLock)
            {
                _RenderingThread = new Thread (RenderLoop);
                _UpdatingThread = new Thread (UpdateLoop);
                //_RenderingThread.IsBackground = true;
                //_UpdatingThread.IsBackground = true;
                _RenderingThread.Start ();
                _UpdatingThread.Start ();
            }
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
            _RenderingThread.Join();
            _UpdatingThread.Join ();

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
        protected void FBOSafety ()
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

        #region UpdateLoop
        void UpdateLoop ()
        {
        }
        #endregion
        #region RenderLoop
        void RenderLoop ()
        {
            MakeCurrent (); // The context now belongs to this thread. No other thread may use it!

            while (!Exiting) {
                using (_Game = new PositronGame (this)) {
                    //Game setup

                    // Since we don't use OpenTK's timing mechanism, we need to keep time ourselves;
                    UpdateWatch.Start ();
                    RenderWatch.Start ();
                    FrameWatch.Start ();
                    TestWatch.Start ();
                    RenderDrawingWatch.Start ();

                    while (!Reset && !Exiting) {
                        // Store this in a local variable because accessors have overhead!
                        // Bear with me...this will get a bit tricky to explain pefrectly...
                    
                        // Sleep the thread for the most milliseconds less than the frame limit time
                        float time_until = (float)FrameLimitTime - Configuration.ThreadSleepTimeStep * 0.001f;
                        while (FrameWatch.Elapsed.TotalSeconds < time_until)
                            Thread.Sleep (Configuration.ThreadSleepTimeStep); // Does this help?
                        // Hard-loop the remainder
                        while (FrameWatch.Elapsed.TotalSeconds < FrameLimitTime);

                        _LastFrameTime = (float)FrameWatch.Elapsed.TotalSeconds;
                        //Console.WriteLine(_LastFrameTime);
                        FrameWatch.Restart ();
                        UpdateWatch.Restart ();
                        lock (_Game.UpdateLock)
                            Update (_LastFrameTime);
                        _LastUpdateTime = (float)UpdateWatch.Elapsed.TotalSeconds;
                        RenderWatch.Restart ();
                        lock (_Game.UpdateLock)
                            RenderView (_LastFrameTime);
                        // TODO: Figure out why this does wild shit if FPS > 60
                        SwapBuffers ();
                        _LastRenderTime = (float)UpdateWatch.Elapsed.TotalSeconds;
                    }
                    Sound.KillTheNoise (); // I don't know where a better place for this could be...
                    Reset = false;
                }
            }
            Context.MakeCurrent(null);
        }
        
        #endregion
        
        #region Update

        void Update (float time)
        {
            OnKeysUpdate (time);
            _Game.Update (time);
        }
        
        #endregion
        
        #region Render
        
        /// <summary>
        /// This is our main rendering function, which executes on the rendering thread.
        /// </summary>
        public void RenderView(float time)
        {
            #region Render Frame Buffer

            
            //GL.PushAttrib(AttribMask.ViewportBit);
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, CanvasFBO); // Bind canvas FBO
            {
                // FBO viewport
                GL.Viewport(0, 0, _CanvasWidth, _CanvasHeight);
                GL.ClearColor (Color.Black);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                // TODO: pre-calculate these
                float p = 1.0f;
                float distance = -0.5f * _CanvasHeight / (float)Math.Tan (0.5f * p); // -0.5 * h * cot(0.5 * p)
                float ratio = (float)_CanvasWidth / (float)_CanvasHeight;
                Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView((float)p, (float)ratio, 0.01f, 9999.0f);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref perspective);
                GL.Translate ((float)_CanvasWidth / -2.0f, (float)_CanvasHeight / -2.0f, distance);
                RenderDrawingWatch.Restart();
                _Game.Draw(time);
                _LastRenderDrawingTime = (float)RenderDrawingWatch.Elapsed.TotalSeconds;
                GL.Color4(1.0, 1.0, 1.0, 1.0);

            }
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // unbind FBO
            //GL.PopAttrib();
            


            #endregion

            #region Render Main Viewport
            GL.BindTexture(TextureTarget.Texture2D, CanvasTexture); // Bind the canvas
            //GL.PushAttrib(AttribMask.ViewportBit);
            {
                // FBO viewport
                GL.Viewport(0, 0, ViewportWidth, ViewportHeight);
                GL.ClearColor (Color.Black);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); // Do clear
                Matrix4 perspective = Matrix4.CreateOrthographic(ViewportWidth, ViewportHeight, -128, 128);
                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadMatrix(ref perspective);
                GL.PushMatrix();
                {
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
                }
                GL.PopMatrix ();
            }
            //GL.PopAttrib();
            GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind
            
            //GL.Flush();

            #endregion
        }

    }
        #endregion
}