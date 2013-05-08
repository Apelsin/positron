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
	public class PositronGame: IUpdateSync, IDisposable
    {
        #region Event-Related
		protected List<KeyValuePair<object, UpdateEventHandler>> _UpdateEventList = new List<KeyValuePair<object, UpdateEventHandler>>();
		public List<KeyValuePair<object, UpdateEventHandler>> UpdateEventList { get { return _UpdateEventList; } }
        #endregion
        #region Member Variables
		public void AddUpdateEventHandler(object sender, UpdateEventHandler handler)
		{
            lock(_UpdateEventList)
			    _UpdateEventList.Add(new KeyValuePair<object, UpdateEventHandler>(sender, handler));
		}
        #region Test stuff
        Stopwatch TestWatch = new Stopwatch();
		Random rand = new Random();
		//public List<BooleanIndicator> TestIndicators = new List<BooleanIndicator>();
		public Player Player1;
		int IncrementTest = 0;
		#endregion
		protected Hashtable _Scenes = new Hashtable();
		protected Scene _CurrentScene;
		protected World _WorldMain;

		public float TimeStepCoefficient = 1.0f;
		protected OrderedDictionary InputAccepterGroups;

        /// <summary>
        /// Lock to synchronize rendering and updating
        /// </summary>
        public readonly object MainUpdateLock = new object();
        
        /// <summary>
        /// Lock to synchronize user input controls
        /// </summary>
        public readonly object MainUserInputLock = new object();

        protected readonly Object _InputAccepterGroupsLock = new Object();
		#endregion
		#region Static Variables
		#endregion
		#region Member Accessors
		public Hashtable Scenes { get { return _Scenes; } }
		public Scene CurrentScene {
			get { return _CurrentScene; }
			set { ChangeScene (value); }
		}
		public World WorldMain { get { return _WorldMain; } set { _WorldMain = value; } }
		// TODO: ensure thread safety here:
		public IInputAccepter[] InputAccepterGroup {
			get { 
				if(InputAccepterGroups != null && InputAccepterGroups.Count > 0)
					return (IInputAccepter[])InputAccepterGroups[0];
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
            Sound.InitialSetup();
			DialogSpeaker.InitialSetup();
		}
		public void Setup ()
		{
			// TODO: world objects need to be pending initialization before the world is controlled by the scene
			FarseerPhysics.Settings.VelocityIterations = 1;
			InputAccepterGroups = new OrderedDictionary();
		}
		public void SetInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
			lock (_InputAccepterGroupsLock) {
				if(!InputAccepterGroups.Contains(name))
					InputAccepterGroups.Insert (0, name, input_accepters);
			}
		}
		public void SetLastInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
			lock (_InputAccepterGroupsLock) {
				if(!InputAccepterGroups.Contains(name))
				{
					InputAccepterGroups.Add (name, input_accepters);
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
				InputAccepterGroups.Insert (0, name, mixed);
			}
		}
		public void RemoveInputAccepters (string name)
		{
			lock (_InputAccepterGroupsLock) {
				InputAccepterGroups.Remove (name);
			}
		}
		public void SetupTests ()
		{
			TestWatch.Start();
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
		public void ChangeScene (Scene next_scene)
		{
			if (_CurrentScene == next_scene || next_scene == null)
				return;
			SceneChangeEventArgs scea = new SceneChangeEventArgs (_CurrentScene, next_scene);
			IEnumerable<RenderSet> next_sets = next_scene.AllRenderSetsInOrder ();
			IEnumerator<RenderSet> next_set_enum = next_sets.GetEnumerator ();
			IEnumerable<RenderSet> current_sets = null;
			IEnumerator<RenderSet> current_set_enum = null;
			RenderSetChangeEventArgs rscea;
			lock (Program.MainUpdateLock) {												// Don't even think about not locking this threaded monstrosity
				if (_CurrentScene != null) {
					// Get the enumerable for the render sets; these need to be in the same order!
					current_sets = _CurrentScene.AllRenderSetsInOrder ();
					current_set_enum = current_sets.GetEnumerator ();					// Enumerate from the beginning of the set enumerable
					foreach (RenderSet render_set in current_sets) {
						if (!next_set_enum.MoveNext ())
							break;
						rscea = new RenderSetChangeEventArgs (render_set, next_set_enum.Current);
						// Process this scene
						for (int i = 0; i < render_set.Count;) {						// For each renderable in render set
							var renderable = render_set [i];
							if (renderable is ISceneElement) {							// If object also implements scene object
								ISceneElement scene_object = (ISceneElement)renderable;	// Cast to scene object
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
					}
					foreach (RenderSet render_set in next_scene.AllRenderSetsInOrder()) {
						if (!current_set_enum.MoveNext ())
							break;
						// Process next scene, PART 1
						foreach (IRenderable renderable in render_set) {
							if (renderable is ISceneElement) {							// If object also implements scene object
								if (renderable is IWorldObject) {
									IWorldObject world_object = (IWorldObject)renderable;
									// HACK: temporarily enable all bodies in this renderset
									// in order to allow deferred updates to affect bodies
									world_object.Body.Enabled = true;
								}
							}
						}
					}
				}

				if(_CurrentScene != null)
					_CurrentScene.OnSceneExit (this, scea);
				next_scene.OnSceneEntry (this, scea);
				// Enumerate from the beginning of the set enumerable
				if(_CurrentScene != null)
					current_set_enum = current_sets.GetEnumerator ();
				// Process next scene, PART 2
				foreach (RenderSet render_set in next_scene.AllRenderSetsInOrder()) {
					if(current_set_enum != null)
					{
						if (!current_set_enum.MoveNext ())
							break;
						rscea = new RenderSetChangeEventArgs (current_set_enum.Current, render_set);
					}
					else
						rscea = new RenderSetChangeEventArgs (null, render_set);
					foreach (IRenderable renderable in render_set) {
						if (renderable is ISceneElement) {
							ISceneElement scene_element = (ISceneElement)renderable;
							// Body.Enabled should be updated with a RenderSetEntry event handler
							scene_element.OnRenderSetEntry (this, rscea);
						}
					}
				}
				var last_scene = _CurrentScene;
				AddUpdateEventHandler(this, (sender, e) => {
					if (last_scene != null && last_scene.FollowTarget != null && last_scene.FollowTarget.Preserve) {
						next_scene.Follow (last_scene.FollowTarget, true);
						//next_scene._ViewPosition = current_scene._ViewPosition;
					}
					return true;
				});
			}
			_CurrentScene = next_scene;	// Update the scene reference
		}
		public void Draw(double time)
		{
			_CurrentScene.Render (time);
		}
		public void Dispose ()
		{
			lock (Program.MainUpdateLock) {
				foreach(Scene scene in Scenes.Values)
					scene.Dispose();
                _Scenes.Clear();
                _Scenes = null;
                InputAccepterGroups.Clear();
                InputAccepterGroups = null;
                _UpdateEventList.Clear();
                _UpdateEventList = null;
                _WorldMain.Clear();
                _WorldMain = null;
			}
		}
		public void Demolish()
		{
			lock (Program.MainUpdateLock) {
				foreach(Scene scene in Scenes.Values)
				{
					foreach(RenderSet render_set in scene.AllRenderSetsInOrder())
					{
						foreach(IRenderable renderable in render_set)
							renderable.Dispose();
						render_set.Dispose();
					}
					scene.Dispose();
				}
				Dispose ();
			}
		}
	}
}

