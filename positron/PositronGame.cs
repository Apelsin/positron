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
	public class PositronGame: IUpdateSync
    {
        #region Event-Related
        protected Queue<UpdateEventHandler> _UpdateEventQueue = new Queue<UpdateEventHandler>();
        public Queue<UpdateEventHandler> UpdateEventQueue { get { return _UpdateEventQueue; } }
        #endregion
        #region Member Variables

        #region Test stuff
        Stopwatch TestWatch = new Stopwatch();
		Random rand = new Random();
		//public List<BooleanIndicator> TestIndicators = new List<BooleanIndicator>();
		public Dialog TestDialog;
		public Player Player1;
		public TileMap BackgroundTiles;
		int IncrementTest = 0;
		#endregion
		protected Scene _CurrentScene;
		protected World _MainWorld;
		public float TimeStepCoefficient = 1.0f;
		protected OrderedDictionary InputAccepterGroups;
		protected readonly Object _InputAccepterGroupsLock = new Object();
		protected int InputAccepterGroupIdx = 0;

		#endregion
		#region Static Variables
		#endregion
		#region Member Accessors
		public Scene CurrentScene { get { return _CurrentScene; } }
		public World MainWorld { get { return _MainWorld; } }
		// TODO: ensure thread safety here:
		public IInputAccepter[] InputAccepterGroup {
			get {  return (IInputAccepter[])InputAccepterGroups[InputAccepterGroupIdx]; }
		}
		public Object InputAccepterGroupsLock {
			get { return _InputAccepterGroupsLock; }
		}
		#endregion
		#region Static Accessors
		#endregion

		public PositronGame ()
		{
		}
		public static void InitialSetup ()
		{
			// Load textures into graphics memory space
			Texture.InitialSetup();
		}
		public void Setup ()
		{
			// TODO: world objects need to be pending initialization before the world is controlled by the scene
			FarseerPhysics.Settings.VelocityIterations = 1;

			Scene.Instantiate(); // Instantiate one of each of the scenes defined in this entire assembly
			_CurrentScene = (Scene)Scene.Scenes["SceneOne"];
			Player1 = new Player (_CurrentScene.Stage, Texture.Get ("sprite_player"));
			Scene.InitializeAll();
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
			TestWatch.Start();
		}
		public void ChangeScene (Scene next_scene)
		{
			if (_CurrentScene == null) {
				_CurrentScene = next_scene;
			}
			else {
				Scene.ChangeScene (ref _CurrentScene, next_scene);
			}
		}
		public void Update (double time)
		{
			//BackgroundTiles.RandomMap();
			time = Math.Round(time, 4);
			_CurrentScene.Update (time * TimeStepCoefficient);
            lock (_UpdateEventQueue)
            {
                while (_UpdateEventQueue.Count > 0)
                    _UpdateEventQueue.Dequeue()(this, new UpdateEventArgs(time));
            }
			foreach(RenderSet render_set in _CurrentScene.UpdateRenderSetsInOrder())
			{
				foreach (object o in render_set) {
					if (o is SpriteObject)
						((SpriteObject)o).Update (time * TimeStepCoefficient);
				}
			}
		}
		public void Draw(double time)
		{
			_CurrentScene.Render (time);
		}
	}
}

