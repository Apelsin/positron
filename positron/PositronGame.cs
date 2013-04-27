using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Reflection;

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
		protected List<KeyValuePair<object, UpdateEventHandler>> _UpdateEventList = new List<KeyValuePair<object, UpdateEventHandler>>();
		public List<KeyValuePair<object, UpdateEventHandler>> UpdateEventList { get { return _UpdateEventList; } }
        #endregion
        #region Member Variables
		public void AddUpdateEventHandler(object sender, UpdateEventHandler handler)
		{
			_UpdateEventList.Add(new KeyValuePair<object, UpdateEventHandler>(sender, handler));
		}
        #region Test stuff
        Stopwatch TestWatch = new Stopwatch();
		Random rand = new Random();
		//public List<BooleanIndicator> TestIndicators = new List<BooleanIndicator>();
		public Dialog TestDialog;
		public Player Player1;
		public TileMap BackgroundTiles;
		int IncrementTest = 0;
		#endregion
		protected Hashtable _Scenes = new Hashtable();
		protected Scene _CurrentScene;
		protected World _WorldMain;
		public float TimeStepCoefficient = 1.0f;
		protected OrderedDictionary InputAccepterGroups;
		protected readonly Object _InputAccepterGroupsLock = new Object();
		protected int InputAccepterGroupIdx = 0;

		#endregion
		#region Static Variables
		#endregion
		#region Member Accessors
		public Hashtable Scenes { get { return _Scenes; } }
		public Scene CurrentScene {
			get { return _CurrentScene; }
			set { ChangeScene (ref _CurrentScene, value); }
		}
		public World WorldMain { get { return _WorldMain; } set { _WorldMain = value; } }
		// TODO: ensure thread safety here:
		public IInputAccepter[] InputAccepterGroup {
			get { 
				if(InputAccepterGroups != null && InputAccepterGroupIdx < InputAccepterGroups.Count)
					return (IInputAccepter[])InputAccepterGroups[InputAccepterGroupIdx];
				else
					return new IInputAccepter[] { };
			}
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
			InputAccepterGroups = new OrderedDictionary();

			Scene.InstantiateScenes(this); // Instantiate one of each of the scenes defined in this entire assembly
			CurrentScene = (Scene)Program.MainGame.Scenes["SceneFirstMenu"];
			Scene.InitializeScenes(this);
		}
		public void SetInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
			lock (_InputAccepterGroupsLock) {
				if(!InputAccepterGroups.Contains(name))
				{
					InputAccepterGroups.Add (name, input_accepters);
					InputAccepterGroupIdx = InputAccepterGroups.Count - 1;
				}
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
				ChangeScene (ref _CurrentScene, next_scene);
			}
		}
		protected void ProcessUpdateEventList (double time)
		{
			lock (_UpdateEventList)
			{
				for(int i = 0; i < _UpdateEventList.Count;)
				{
					if(_UpdateEventList[i].Value(_UpdateEventList[i].Key, new UpdateEventArgs(time, i)))
						_UpdateEventList.RemoveAt(i);
					else
						i++;
				}
			}
		}
		public void Update (double time)
		{
			//BackgroundTiles.RandomMap();
			time = Math.Round(time, 4);
			_CurrentScene.Update (time * TimeStepCoefficient);
			ProcessUpdateEventList(time);
			foreach(RenderSet render_set in _CurrentScene.UpdateRenderSetsInOrder())
			{
				foreach (object o in render_set) {
					if (o is SpriteBase)
						((SpriteBase)o).Update (time * TimeStepCoefficient);
				}
			}
		}
		public void ChangeScene (ref Scene current_scene, Scene next_scene)
		{
			if (current_scene == next_scene)
				return;
			SceneChangeEventArgs scea = new SceneChangeEventArgs(current_scene, next_scene);
			next_scene.OnSceneEntry(this, scea);
			if (current_scene != null) {
				// TODO: Make this better
				// Get the enumerable for the render sets; these need to be in the same order!
				IEnumerable<RenderSet> current_sets = current_scene.AllRenderSetsInOrder ();
				IEnumerable<RenderSet> next_sets = next_scene.AllRenderSetsInOrder ();
				//IEnumerator<RenderSet> current_set_enum = current_sets.GetEnumerator ();
				IEnumerator<RenderSet> next_set_enum = next_sets.GetEnumerator ();
				RenderSetChangeEventArgs rscea;
				lock (Program.MainUpdateLock) {										// Don't even think about not locking this threaded monstrosity
					ProcessUpdateEventList(0.0);
					foreach (RenderSet render_set in current_sets) {
						if (!next_set_enum.MoveNext ())
							break;
						rscea = new RenderSetChangeEventArgs(render_set, next_set_enum.Current);
						// Process this scene
						for (int i = 0; i < render_set.Count;) {						// For each renderable in render set
							var renderable = render_set [i];
							if (renderable is ISceneObject) {							// If object also implements scene object
								ISceneObject scene_object = (ISceneObject)renderable;	// Cast to scene object
								if (scene_object is IWorldObject) {
									IWorldObject world_object = (IWorldObject)scene_object;
									// Disable the body object is not preserved
									world_object.Body.Enabled &= world_object.Preserve;
								}
								if (scene_object.Preserve) { 							// If scene object is preserved
									next_set_enum.Current.Add (renderable);				// Add in this object
									render_set.RemoveAt (i);							// Remove from previous 
									scene_object.OnRenderSetTransfer (this, rscea);
									continue;
								}
							}
							i++;
						}
						// Process next scene
						foreach (IRenderable renderable in next_set_enum.Current) {
							if (renderable is ISceneObject) {							// If object also implements scene object
								ISceneObject scene_object = (ISceneObject)renderable;	// Cast to scene object
								scene_object.OnRenderSetEntry(this, rscea);
							}
						}
					}
					// Scene exit event must be synchronized with update event list
					// for world-dependent operations
					AddUpdateEventHandler(this, (sender, e) => {
						if(_CurrentScene == scea.To)
							scea.From.OnSceneExit (this, scea); return true;
					});
				}
				if (current_scene.FollowTarget != null && current_scene.FollowTarget.Preserve) {
					next_scene.Follow (current_scene.FollowTarget, true);
					//next_scene._ViewPosition = current_scene._ViewPosition;
				}
				
			}
			current_scene = next_scene;	// Update the scene reference
			ProcessUpdateEventList(0.0);
		}
		public void Draw(double time)
		{
			_CurrentScene.Render (time);
		}
	}
}

