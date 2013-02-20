using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace positron
{
	public class ThreadedRendering : GameWindow
	{
		#region State
		#region Member
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
		int CanvasWidth = 512;
		/// <summary>
		/// Height of canvas in pixels
		/// </summary>
		int CanvasHeight = 320;
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
		/// Signifies that the program is exiting
		/// </summary>
		bool Exiting = false;
		/// <summary>
		/// Main OpenGL-calling thread for rendering
		/// </summary>
		Thread RenderingThread;
		/// <summary>
		/// Lock to synchronize rendering and updating
		/// </summary>
		object UpdateLock = new object();
		/// <summary>
		/// The acceleration due to gravity at sea level on Earth in m/s
		/// </summary>
		const float GravityAccel = -9.81f;
		/// <summary>
		/// Some random thing
		/// </summary>
		Random Randy = new Random(4352453);
		#endregion
		#endregion
		struct Particle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public Color4 Color;
		}
		List<Particle> Particles = new List<Particle>();
		Random rand = new Random();
		
		public ThreadedRendering ()
			: base()
		{
			lock (UpdateLock)
			{
				this.Width = CanvasWidth;
				this.Height = CanvasHeight;
			}
			Keyboard.KeyDown += delegate(object sender, KeyboardKeyEventArgs e)
			{
				if (e.Key == Key.Escape)
					this.Exit();
			};

			/*
			Keyboard.KeyUp += delegate(object sender, KeyboardKeyEventArgs e)
			{
				if (e.Key == Key.F)
				{
					lock(UpdateLock)
					{
						if (this.WindowState == WindowState.Fullscreen)
							this.WindowState = WindowState.Normal;
						else
							this.WindowState = WindowState.Fullscreen;
						OnResize(null);
					}
				}
			};
			*/
			
			Resize += delegate(object sender, EventArgs e)
			{
				// Note that we cannot call any OpenGL methods directly. What we can do is set
				// a flag and respond to it from the rendering thread.
				lock (UpdateLock)
				{
					Width = Math.Max (Width, CanvasWidth);
					Height = Math.Max (Height, CanvasHeight);
					VieportChanged = true;
					ViewportWidth = Width;
					ViewportHeight = Height;
					int multi = ViewportWidth / CanvasWidth;
					multi = Math.Min(multi, ViewportHeight / CanvasHeight);
					FBOScaleX = multi * (float)CanvasWidth / (float)ViewportWidth;
					FBOScaleY = multi * (float)CanvasHeight / (float)ViewportHeight;
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
			
			// Make sure initial position are correct, otherwise we'll give a huge
			// initial velocity to the balls.
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

			GL.Enable(EnableCap.DepthTest);
			GL.ClearDepth(1.0);
			GL.DepthFunc(DepthFunction.Lequal);
			
			GL.Enable(EnableCap.CullFace);
			
			// Create Color Tex
			GL.GenTextures(1, out CanvasTexture);
			GL.BindTexture(TextureTarget.Texture2D, CanvasTexture);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, CanvasWidth, CanvasHeight, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
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
			RenderingThread.IsBackground = true;
			RenderingThread.Start();
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
		
		#region RenderLoop
		
		void RenderLoop()
		{
			MakeCurrent(); // The context now belongs to this thread. No other thread may use it!
			
			//VSync = VSyncMode.On;
			
			for (int i = 0; i < (1<<8); i++)
			{
				Particle p = new Particle();
				p.Position = new Vector2((float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble() * 2 - 1);
				p.Color.R = (float)rand.NextDouble();
				p.Color.G = (float)rand.NextDouble();
				p.Color.B = (float)rand.NextDouble();
				Particles.Add(p);
			}
			
			// Since we don't use OpenTK's timing mechanism, we need to keep time ourselves;
			Stopwatch render_watch = new Stopwatch();
			Stopwatch update_watch = new Stopwatch();
			update_watch.Start();
			render_watch.Start();

			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.PointSmooth);
			GL.Enable(EnableCap.Texture2D); // enable Texture Mapping
			GL.PointSize(10);
			
			while (!Exiting)
			{
				Update(update_watch.Elapsed.TotalSeconds);
				update_watch.Reset();
				update_watch.Start();
				
				Render(render_watch.Elapsed.TotalSeconds);
				render_watch.Reset(); // Stopwatch may be inaccurate over larger intervals.
				render_watch.Start(); // Plus, timekeeping is easier if we always start counting from 0.
				
				SwapBuffers();
			}
			
			Context.MakeCurrent(null);
		}
		
		#endregion
		
		#region Update
		
		void Update(double time)
		{
			lock (UpdateLock)
			{
				// When the user moves the window we make the particles react to
				// this movement. The reaction is semi-random and not physically
				// correct. It looks quite good, however.
				if (PositionChanged)
				{
					for (int i = 0; i < Particles.Count; i++)
					{
						Particle p = Particles[i];
						p.Velocity += new Vector2(
							16 * (PositiondX + 0.05f * (float)(rand.NextDouble() - 0.5)),
							32 * (PositiondY + 0.05f * (float)(rand.NextDouble() - 0.5)));
						Particles[i] = p;
					}
					
					PositionChanged = false;
				}
			}
			
			// For simplicity, we use simple Euler integration to simulate particle movement.
			// This is not accurate, especially under varying timesteps (as is the case here).
			// A better solution would have been time-corrected Verlet integration, as
			// described here:
			// http://www.gamedev.net/reference/programming/features/verlet/

			for (int i = 0; i < Particles.Count; i++)
			{
				Particle p = Particles[i];
				
				p.Velocity.X = Math.Abs(p.Position.X) >= 1 ?-p.Velocity.X * 0.92f : p.Velocity.X * 0.97f;
				p.Velocity.Y = Math.Abs(p.Position.Y) >= 1 ? -p.Velocity.Y * 0.92f : p.Velocity.Y * 0.97f;
				if (p.Position.Y > -0.99)
				{
					p.Velocity.Y += (float)(GravityAccel * time);
				}
				else
				{
					if (Math.Abs(p.Velocity.Y) < 0.02)
					{
						p.Velocity.Y = 0;
						p.Position.Y = -1;
					}
					else
					{
						p.Velocity.Y *= 0.9f;
					}
				}
				p.Velocity.Y = Randy.Next() % 100 == 0 ? (float)((0x40 + (Randy.Next() & 0xFF)) * time) : p.Velocity.Y;
				p.Position += p.Velocity * (float)time;
				if (p.Position.Y <= -1)
					p.Position.Y = -1;
				Particles[i] = p;
			}
		}
		
		#endregion
		
		#region Render
		
		/// <summary>
		/// This is our main rendering function, which executes on the rendering thread.
		/// </summary>
		public void Render(double time)
		{
			lock (UpdateLock) // Lock for this stuff
			{
				if (VieportChanged)
				{
					// FBO viewport
					GL.Viewport(0, 0, ViewportWidth, ViewportHeight);
					VieportChanged = false;
				}
			}

			#region Render Frame Buffer
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, CanvasFBO);
			GL.PushAttrib(AttribMask.ViewportBit);
			{	
				// FBO viewport
				GL.Viewport(0, 0, CanvasWidth, CanvasHeight);
				GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.OneMinusSrcAlpha);
				GL.ClearColor (Color.White);
				GL.Clear(ClearBufferMask.ColorBufferBit);
				Matrix4 perspective =
					Matrix4.CreateOrthographic(2, 2, -1, 1);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref perspective);
				
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadIdentity();

				GL.Begin (BeginMode.Quads);
				GL.Color4 (Color.OrangeRed);
				GL.Vertex2(1.0f, 1.0f);
				GL.Vertex2(-1.0f, 1.0f);
				GL.Color4 (Color.MediumBlue);
				GL.Vertex2(-1.0f, -1.0f);
				GL.Vertex2(1.0f, -1.0f);
				GL.End();

				// draw some complex object into the FBO's textures
				//GL.Enable( EnableCap.Lighting );
				//GL.Enable( EnableCap.Light0 );
				//GL.Enable( EnableCap.ColorMaterial );

				GL.Begin(BeginMode.Points);
	            foreach (Particle p in Particles)
	            {
	                GL.Color4(p.Color);
	                GL.Vertex2(p.Position);
	            }
				GL.Color4(Color.White);
	            GL.End();
				//GL.Disable( EnableCap.ColorMaterial );
				//GL.Disable( EnableCap.Light0 );
				//GL.Disable( EnableCap.Lighting );

			}
			GL.PopAttrib();
			GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0); // disable rendering into the FBO

			#endregion


			GL.BindTexture(TextureTarget.Texture2D, CanvasTexture);
			GL.ClearColor (Color.Black);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.PushMatrix();
			{
				// Draw the Color Texture
				GL.Translate(0f, 0f, 0f);

				GL.Begin(BeginMode.Quads);
				{
					GL.TexCoord2(0.0f, 1f);
					GL.Vertex2(-FBOScaleX, FBOScaleY);
					GL.TexCoord2(0.0f, 0.0f);
					GL.Vertex2(-FBOScaleX, -FBOScaleY);
					GL.TexCoord2(1.0f, 0.0f);
					GL.Vertex2(FBOScaleX, -FBOScaleY);
					GL.TexCoord2(1.0f, 1.0f);
					GL.Vertex2(FBOScaleX, FBOScaleY);
				}
				GL.End();

			}
			GL.PopMatrix();
			GL.BindTexture(TextureTarget.Texture2D, 0); // bind default texture
		}

	}
		#endregion
}