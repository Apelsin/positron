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
	public class MainGame
	{
		#region Member Variables

		#region Test stuff
		Stopwatch TestWatch = new Stopwatch();
		Random rand = new Random();
		public List<BooleanIndicator> TestIndicators = new List<BooleanIndicator>();
		public Dialog TestDialog;
		public Player Player1;
		public TileMap BackgroundTiles;
		int IncrementTest = 0;
		#endregion

		public float TimeStepCoefficient = 1.0f;
		protected OrderedDictionary InputAccepterGroups;
		protected Object _InputAccepterGroupsLock = new Object();
		protected int InputAccepterGroupIdx = 0;
		// TODO: ensure thread safety here:
		public IInputAccepter[] InputAccepterGroup {
			get {  return (IInputAccepter[])InputAccepterGroups[InputAccepterGroupIdx]; }
		}
		public Object InputAccepterGroupsLock { get { return _InputAccepterGroupsLock; } }
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
		public void Setup ()
		{
			Player1 = new Player (_CurrentScene.Stage, Texture.Get ("sprite_player"));
			InputAccepterGroups = new OrderedDictionary();
			InputAccepterGroups.Add("Player1", new IInputAccepter[]{ Player1 });
		}
		public void SetInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
			lock (_InputAccepterGroupsLock) {
				InputAccepterGroups.Add (name, input_accepters);
				InputAccepterGroupIdx = InputAccepterGroups.Count - 1;
			}
		}
		public void MixAddInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
			lock (_InputAccepterGroupsLock) {
				IInputAccepter[] mixed = new IInputAccepter[InputAccepterGroup.Length + input_accepters.Length];
				int idx = 0;
				for(int i = 0; i < InputAccepterGroup.Length; i++)
					mixed[idx++] = InputAccepterGroup[i];
				for(int i = 0; i < input_accepters.Length; i++)
					mixed[idx++] = input_accepters[i];
				InputAccepterGroups.Add (name, mixed);
				InputAccepterGroupIdx = InputAccepterGroups.Count - 1;
			}
		}
		public void RemoveInputAccepter (string name)
		{
			lock (_InputAccepterGroupsLock) {
				InputAccepterGroups.Remove (name);
				InputAccepterGroupIdx = MathUtil.Clamp (InputAccepterGroupIdx, InputAccepterGroups.Count - 1, 0);
			}
		}
		public void SetupTests ()
		{
			int w_i = (int)Program.MainWindow.CanvasWidth;
			int h_i = (int)Program.MainWindow.CanvasHeight;
			Vector3d q = new Vector3d (0.0, 0.25 * h_i, 0.0);
			BackgroundTiles = new TileMap(_CurrentScene.Background, 64, 18, Texture.Get("sprite_tile_bg_atlas"));
			BackgroundTiles.RandomMap();
            BackgroundTiles.Build();
			Player1.Position += q;
			_CurrentScene.Follow(Player1);
			Texture default_sprite = Texture.Get ("sprite_small_disc");
			for (int i = 0; i < (1<<6); i++) {
				SpriteObject sprite = new SpriteObject (_CurrentScene.Tests, rand.Next (10, w_i - 10), rand.Next (10, h_i / 2 - 10), 1.0, 1.0, default_sprite);
				sprite.Position += q;
				//if(i % 20 != 0)
				sprite.Body.BodyType = BodyType.Dynamic;
				sprite.Color = Color.FromArgb ((int)(rand.Next () | 0xFF000000));
			}
			Texture four_square = Texture.Get ("sprite_four_square");
			for (int i = 0; i < (1<<6); i++) {
				SpriteObject sprite = new SpriteObject (_CurrentScene.Tests, rand.Next (10, w_i - 10), rand.Next (10, h_i / 2 - 10), 2.0, 2.0, four_square);
				sprite.Position += q;
				//if(i % 20 != 0)
				sprite.Body.BodyType = BodyType.Dynamic;
				//sprite.Color = Color.FromArgb ((int)(rand.Next () | 0xFF000000));
			}
//			Vector3d b = new Vector3d (w_i, 0.0, 0.0);
//			Vector3d c = new Vector3d (0.0, 0.75 * h_i, 0.0);
//			Vector3d d = new Vector3d (w_i, 0.75 * h_i, 0.0);
//			Vector3d e = new Vector3d (w_i, h_i * 0.5, 0.0);
//			_CurrentScene.Tests.Add (new Line (q, b, 4.0f, _CurrentScene));
//			_CurrentScene.Tests.Add (new Line (q + b, c, 4.0f, _CurrentScene));
//			_CurrentScene.Tests.Add (new Line (q + d, -b, 4.0f, _CurrentScene));
//			_CurrentScene.Tests.Add (new Line (q + c, -c, 4.0f, _CurrentScene));
//			_CurrentScene.Tests.Add(new Line(Vector3d.Zero, e, 4.0f, _CurrentScene));
			int bi_x = 2, bi_y = h_i - 18;
			for (int i = 0; i < 24; i++) {
				var bi = new BooleanIndicator (_CurrentScene.HUD, bi_x += 18, bi_y);
				TestIndicators.Add (bi);
			}
			//for (int i = 0; i < 3; i++) {
			//	var spidey = new Spidey(_CurrentScene.Tests, 10 * i + 30, h_i / 4);
			//	spidey.Position += q;
			//	spidey.Animate = true;
			//	spidey.Body.BodyType = BodyType.Dynamic;
			//}
			for (int i = 0; i < 100; i++)
			{
				double x = 20 * i;
				double y = rand.NextDouble() * 30;
				var ground = new SpriteObject(_CurrentScene.Tests, x, y, Texture.Get("sprite_tile_bg_atlas"));
			}
			TestDialog = new Dialog(_CurrentScene.HUD, "Test", null);
			TestDialog.RenderSet = _CurrentScene.HUD;
			TestDialog.PauseTime = 1.0f;
			TestWatch.Start();
		}
		public void Update (double time)
		{
			//BackgroundTiles.RandomMap();
			_CurrentScene.Update (time * TimeStepCoefficient);
//			int millis = (int)TestWatch.Elapsed.TotalMilliseconds;
//			for (int i = 0; i < TestIndicators.Count; i++) {
//				int t = (millis % (TestIndicators.Count * 100)) / 100;
//				TestIndicators [i].State = (t % 3) == (i % 3);
//			}
			//TestDialog.SizeX = 0.25 * (IncrementTest++ % (_CurrentScene.ViewWidth * 4));

			foreach (object o in _CurrentScene.Tests) {
				if(o is SpriteObject)
					((SpriteObject)o).Update(time * TimeStepCoefficient);
			}
			foreach (object o in _CurrentScene.Stage) {
				if(o is SpriteObject)
					((SpriteObject)o).Update(time * TimeStepCoefficient);
			}
            foreach (object o in _CurrentScene.HUD)
            {
                if (o is SpriteObject)
                    ((SpriteObject)o).Update(time * TimeStepCoefficient);
            }
		}
		public void Draw(double time)
		{
			_CurrentScene.Render (time);
		}
	}
}

