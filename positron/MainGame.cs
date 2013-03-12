using System;
using System.Collections;
using System.Collections.Generic;
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
	public class MainGame
	{
		#region Member Variables
		/// <summary>
		/// A watch used for testing purposes
		/// </summary>
		Stopwatch TestWatch = new Stopwatch();
		/// <summary>
		/// A random number generator!
		/// </summary>
		Random rand = new Random();
		List<BooleanIndicator> TestIndicators = new List<BooleanIndicator>();
		#endregion
		#region Static Variables
		private Scene _CurrentScene;
		#endregion
		#region Member Accessors
		public Scene CurrentScene { get { return _CurrentScene; } }
		#endregion
		#region Static Accessors
		#endregion

		public MainGame ()
		{
			_CurrentScene = Scene.Create("herp derp");
		}
		public static void InitialSetup ()
		{
			// Load textures into graphics memory space
			Texture.InitialSetup();
		}
		public void SetupTests ()
		{
			_CurrentScene.Tests.Add(new Player(Texture.Get("sprite_player")));
			int w_i = (int)Program.MainWindow.CanvasWidth;
			int h_i = (int)Program.MainWindow.CanvasHeight;
			for (int i = 0; i < (1<<7); i++)
			{
				SpriteObject sprite = new SpriteObject(rand.Next (10, w_i - 10), rand.Next (10, h_i - 10), _CurrentScene);
				//if(i % 20 != 0)
					sprite.Body.BodyType = BodyType.Dynamic;
				sprite.Color = Color.FromArgb ((int)(rand.Next () | 0xFF000000));
				_CurrentScene.Tests.Add (sprite);
			}
			Texture four_square = Texture.Get("sprite_four_square");
			for (int i = 0; i < (1<<7); i++) {
				SpriteObject sprite = new SpriteObject(rand.Next (10, w_i - 10), rand.Next (10, h_i - 10), four_square, _CurrentScene);
				//if(i % 20 != 0)
					sprite.Body.BodyType = BodyType.Dynamic;
				sprite.Color = Color.FromArgb ((int)(rand.Next () | 0xFF000000));
				_CurrentScene.Tests.Add (sprite);
			}
			Vector3d b = new Vector3d(w_i, 0.0, 0.0);
			Vector3d c = new Vector3d(0.0, h_i, 0.0);
			Vector3d d = new Vector3d(w_i, h_i, 0.0);
			Vector3d e = new Vector3d(w_i, h_i * 0.5, 0.0);
			_CurrentScene.Tests.Add(new Line(Vector3d.Zero, b, 4.0f, _CurrentScene));
			_CurrentScene.Tests.Add(new Line(b, c, 4.0f, _CurrentScene));
			_CurrentScene.Tests.Add(new Line(d, -b, 4.0f, _CurrentScene));
			_CurrentScene.Tests.Add(new Line(c, -c, 4.0f, _CurrentScene));
			//_CurrentScene.Tests.Add(new Line(Vector3d.Zero, e, 4.0f, _CurrentScene));
			int bi_x = 2, bi_y = h_i - 18;
			for(int i = 0; i < 24; i++)
			{
				var bi = new BooleanIndicator(bi_x += 18, bi_y);
				TestIndicators.Add(bi);
				_CurrentScene.Tests.Add(bi);
			}
			TestWatch.Start();
		}
		public void Update (double time)
		{
			_CurrentScene.Update (time);
			for(int i = 0; i < TestIndicators.Count; i++)
			{
				int t = ((int)TestWatch.Elapsed.TotalMilliseconds % (TestIndicators.Count * 100)) / 100;
				TestIndicators[i].State = t == i;
			}
		}
		public void Draw(double time=0)
		{
			_CurrentScene.Render (time);
		}
	}
}

