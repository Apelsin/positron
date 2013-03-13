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

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace positron
{
	#region Event-related
	public class KeysUpdateEventArgs : EventArgs
	{
		public KeysUpdateEventArgs(OrderedDictionary keys_and_times)
		{
			KeysPressedWhen = keys_and_times;
		}
		public OrderedDictionary KeysPressedWhen { get; set; }
	}
	public delegate void KeysUpdateEventHandler(object sender, KeysUpdateEventArgs e);
	#endregion
	public class ThreadedRendering : GameWindow
	{
		#region State
		#region Member
		public int PlayerKeyUpdates = 0;
		public int ThreadedUpdateCount = 0;
		/// <summary>
		/// GL handle for canvas FBO texture
		/// </summary>
		uint CanvasTexture;
		/// <summary>
		/// GL handle for canvas FBO
		/// </summary>
		uint CanvasFBO;
		/// <summary>
		/// Signifies if the vieport needs to be re-instantiated
		/// </summary>
		bool VieportChanged = true;
		/// <summary>
		/// Width of canvas in pixels
		/// </summary>
		int _CanvasWidth = 512;
		/// <summary>
		/// Height of canvas in pixels
		/// </summary>
		int _CanvasHeight = 320;
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
		/// Signifies that the window position changed
		/// </summary>
		bool PositionChanged = true;
		/// <summary>
		/// The main window's X-position in pixels
		/// </summary>
		int PositionX;
		/// <summary>
		/// The main window's Y-position in pixels
		/// </summary>
		int PositionY;
		/// <summary>
		/// The main window's X-position rate of change
		/// </summary>
		float PositiondX;
		/// <summary>
		/// The main window's Y-position rate of change
		/// </summary>
		float PositiondY;
		/// <summary>
		/// Ordered dictionary containing the current keyboard keys being pressed in time order
		///w </summary>
		OrderedDictionary KeysPressed = new OrderedDictionary();
		/// <summary>
		/// Signifies that the program is exiting
		/// </summary>
		bool Exiting = false;
		/// <summary>
		/// Main OpenGL-calling thread for rendering
		/// </summary>
		Thread RenderingThread;
		/// <summary>
		/// Main updating thread
		/// </summary>
		Thread UpdatingThread;

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

		/// <summary>
		/// Lock to synchronize rendering and updating
		/// </summary>
		object UpdateLock = new object();

		/// <summary>
		/// Lock to synchronize user input controls
		/// </summary>
		object UserInputLock = new object();

		double FrameLimitTime = 1.0 / Configuration.FrameRateCap;

		/// <summary>
		/// Some random thing
		/// </summary>
		Random Randy = new Random(4352453);

		#endregion
		#region Accessors
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
		#endregion
		#endregion
		#region Event
		public event KeysUpdateEventHandler KeysUpdate;
		protected virtual void OnKeysUpdate ()
		{
			if (KeysUpdate != null) {
				//TODO: Not sure if this respects UpdateLock (verify)
				lock(UserInputLock)
				{
					KeysUpdate (this, new KeysUpdateEventArgs (KeysPressed));
				}
			}
		}
		#endregion
		
		public ThreadedRendering ()
			: base()
		{

			lock (UpdateLock)
			{
				this.Width = _CanvasWidth;
				this.Height = _CanvasHeight;
			}
			Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
			{
				if (e.Key == Key.Escape)
					this.Exit();
				else if(e.Key == Key.Space)
				{
				}
				lock(UserInputLock)
					KeysPressed.Add (e.Key, DateTime.Now);
			};
			Keyboard.KeyUp += delegate(object sender, KeyboardKeyEventArgs e)
			{
				/*if (e.Key == Key.F)
				{
					lock(UpdateLock)
					{
						if (this.WindowState == WindowState.Fullscreen)
							this.WindowState = WindowState.Normal;
						else
							this.WindowState = WindowState.Fullscreen;
						OnResize(null);
					}
				}*/
				lock(UserInputLock)
					KeysPressed.Remove(e.Key);
			};
			Resize += delegate(object sender, EventArgs e)
			{
				// Note that we cannot call any OpenGL methods directly. What we can do is set
				// a flag and respond to it from the rendering thread.
				lock (UpdateLock)
				{
					Width = Math.Max (Width, _CanvasWidth);
					Height = Math.Max (Height, _CanvasHeight);
					VieportChanged = true;
					ViewportWidth = Width;
					ViewportHeight = Height;
					int multi = ViewportWidth / _CanvasWidth;
					multi = Math.Min(multi, ViewportHeight / _CanvasHeight);
					FBOScaleX = multi * (float)(_CanvasWidth*_CanvasWidth) / (float)ViewportWidth;
					FBOScaleY = multi * (float)(_CanvasHeight*_CanvasHeight) / (float)ViewportHeight;
				}
			};
			Move += delegate(object sender, EventArgs e)
			{
				// Note that we cannot call any OpenGL methods directly. What we can do is set
				// a flag and respond to it from the rendering thread.
				lock (UpdateLock)
				{
					PositionChanged = true;
					PositiondX = (PositionX - X) / (float)Width;
					PositiondY = (PositionY - Y) / (float)Height;
					PositionX = X;
					PositionY = Y;
				}
			};	
			// Make sure initial position are correct, otherwise there will be a great initial velocity
			PositionX = X;
			PositionY = Y;
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
				Exit();
			}

			// Depth buffer init
			GL.Enable(EnableCap.DepthTest);
			GL.ClearDepth(1.0);
			GL.DepthFunc(DepthFunction.Lequal);

			// Blending init
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.DstAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			// Culling init
			GL.Enable(EnableCap.CullFace);

			// Create Color Tex
			GL.GenTextures(1, out CanvasTexture);
			GL.BindTexture(TextureTarget.Texture2D, CanvasTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, _CanvasWidth, _CanvasHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			// GL.Ext.GenerateMipmap( GenerateMipmapTarget.Texture2D );
			
			// Create a FBO and attach the textures
			GL.Ext.GenFramebuffers(1, out CanvasFBO);
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, CanvasFBO);
			GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, CanvasTexture, 0);

			FBOSafety(); // Make sure things won't explode!

			Context.MakeCurrent(null); // Release the OpenGL context so it can be used on the new thread.

			RenderingThread = new Thread(RenderLoop);
			UpdatingThread = new Thread(UpdateLoop);
			RenderingThread.IsBackground = true;
			UpdatingThread.IsBackground = true;
			RenderingThread.Start();
			UpdatingThread.Start();
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
			RenderingThread.Join();
			UpdatingThread.Join();

			// Delete textures from graphics memory space
			Texture.Teardown();

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
			Console.WriteLine("max. ColorBuffers: " + queryinfo[0] + " max. AuxBuffers: " + queryinfo[1] + " max. DrawBuffers: " + queryinfo[2] +
			                  "\nStereo: " + queryinfo[3] + " Samples: " + queryinfo[4] + " DoubleBuffer: " + queryinfo[5]);
			
			Console.WriteLine("Last GL Error: " + GL.GetError());
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
			// Updating is handled before render *for now*
			/*UpdateWatch.Start ();
			while (!Exiting) {
				while (UpdateWatch.Elapsed.TotalSeconds < FrameLimitTime);
				lock(UpdateLock)
				{
					double time = UpdateWatch.Elapsed.TotalSeconds;
					UpdateWatch.Reset ();
					UpdateWatch.Start ();
					Update (time);
				}
			}*/
		}
		#endregion
		#region RenderLoop
		void RenderLoop()
		{
			MakeCurrent(); // The context now belongs to this thread. No other thread may use it!

			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.PointSmooth);
			GL.Enable(EnableCap.Texture2D); // enable Texture Mapping

			// Since we don't use OpenTK's timing mechanism, we need to keep time ourselves;
			UpdateWatch.Start();
			RenderWatch.Start();
			FrameWatch.Start();

			// Store this in a local variable because accessors have overhead!
			int caffeine = Configuration.ThreadSleepTolerance;
			while (!Exiting) {
				// Sleep the thread for the most milliseconds less than the frame limit time
				double frame_time = FrameWatch.Elapsed.TotalSeconds;
				// At least one dmillisecond will be in a loop (because Thread.Sleep is really shitty)
				int millisleep = Math.Max(0, (int)(1000 * (FrameLimitTime - frame_time)) - caffeine);
				Thread.Sleep (millisleep);
				while (FrameWatch.Elapsed.TotalSeconds < FrameLimitTime);
				frame_time = UpdateWatch.Elapsed.TotalSeconds;
				FrameWatch.Restart();

				double update_time = UpdateWatch.Elapsed.TotalSeconds;
				UpdateWatch.Restart();
				Update (update_time);
				double render_time = RenderWatch.Elapsed.TotalSeconds;
				RenderWatch.Restart();
				RenderView (render_time);
				SwapBuffers();
			}
			Context.MakeCurrent(null);
		}
		
		#endregion
		
		#region Update

		void Update (double time)
		{
			// Objects subscribe to this event
			// TODO: create managed KeyUp and KeyDown for MainWindow
			OnKeysUpdate ();
			// Really bad singleton implementation
			Program.Game.Update (time);
		}
		
		#endregion
		
		#region Render
		
		/// <summary>
		/// This is our main rendering function, which executes on the rendering thread.
		/// </summary>
		public void RenderView(double time)
		{
			#region Render Frame Buffer

			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, CanvasFBO); // Bind canvas FBO
			GL.PushAttrib(AttribMask.ViewportBit);
			{
				// FBO viewport
				GL.Viewport(0, 0, _CanvasWidth, _CanvasHeight);
				GL.ClearColor (Color.White); // Clear as white
				GL.Clear(ClearBufferMask.ColorBufferBit); // Do clear
				Matrix4 perspective = Matrix4.CreateOrthographic(CanvasWidth, CanvasHeight, -1, 1);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref perspective);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadIdentity();
				GL.PushMatrix();
				{
					GL.Translate(-_CanvasWidth * 0.5, -_CanvasHeight * 0.5, 0f);
					// Draw background gradient:
					/*
					GL.Begin (BeginMode.Quads);
					GL.Color4 (Color.OrangeRed);
					GL.Vertex2(0, 0);
					GL.Vertex2(_CanvasWidth, 0);
					GL.Color4 (Color.MediumBlue);
					GL.Vertex2(_CanvasWidth, _CanvasHeight);
					GL.Vertex2(0, _CanvasHeight);
					GL.End();
					*/

					// Draw the meat and potatos
					//GL.Enable( EnableCap.Lighting );
					//GL.Enable( EnableCap.Light0 );
					//GL.Enable( EnableCap.ColorMaterial );
					// "Singleton" object
					Program.Game.Draw(time);
					GL.Color4(Color.White);
					//GL.Disable( EnableCap.ColorMaterial );
					//GL.Disable( EnableCap.Light0 );
					//GL.Disable( EnableCap.Lighting );
				}
				GL.PopMatrix();

			}
			GL.PopAttrib();
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // unbind FBO


			#endregion

			#region Render Main Viewport

			lock (UpdateLock)
			{
				if (VieportChanged)
				{
					GL.Viewport(0, 0, ViewportWidth, ViewportHeight);
					//GL.ClearAccum(0.0f, 0.0f, 0.0f, 1.0f);
					//GL.Clear(ClearBufferMask.AccumBufferBit);
					VieportChanged = false;
				}
			}

			GL.BindTexture(TextureTarget.Texture2D, CanvasTexture); // Bind the canvas
			GL.ClearColor (Color.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();
			{
				GL.Translate(-FBOScaleX * 0.5, -FBOScaleY * 0.5, 0f);
				// Draw the Color Texture
				GL.Begin(BeginMode.Quads);
				{
					GL.TexCoord2(0.0f, 0.0f);
					GL.Vertex2(0.0f, 0.0f);
					GL.TexCoord2(1.0f, 0.0f);
					GL.Vertex2(FBOScaleX, 0.0f);
					GL.TexCoord2(1.0f, 1.0f);
					GL.Vertex2(FBOScaleX, FBOScaleY);
					GL.TexCoord2(0.0f, 1.0f);
					GL.Vertex2(0.0f, FBOScaleY);
				}
				GL.End();
			}
			GL.PopMatrix();
			GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind

			
			GL.Flush();

			#endregion
		}

	}
		#endregion
}