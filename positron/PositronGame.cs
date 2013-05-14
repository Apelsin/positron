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
	public class PositronGame: IUpdateSync, IDisposable, IGLContextLateUpdate
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
        protected ThreadedRendering _Window;
		protected Hashtable _Scenes = new Hashtable();
		protected Scene _CurrentScene;
		protected World _WorldMain;

		public float TimeStepCoefficient = 1.0f;
		protected OrderedDictionary InputAccepterGroups;

        /// <summary>
        /// Lock to synchronize rendering and updating
        /// </summary>
        public readonly object UpdateLock = new object();

		#endregion
		#region Static Variables
		#endregion
		#region Member Accessors
        public ThreadedRendering Window { get { return _Window; } }
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
		#endregion
		#region Static Accessors
		#endregion

		public PositronGame (ThreadedRendering window)
		{
            _Window = window;
            // TODO: world objects need to be pending initialization before the world is controlled by the scene
            FarseerPhysics.Settings.VelocityIterations = 1;
            InputAccepterGroups = new OrderedDictionary();
            SetupScenes();
            _CurrentScene = (Scene)_Scenes["SceneFirstMenu"];
		}
		public static void InitialSetup ()
		{
			// Load textures into graphics memory space
			Texture.InitialSetup();
            Sound.InitialSetup();
			DialogSpeaker.InitialSetup();
		}
		public void SetInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
            lock (InputAccepterGroups) {
				if(!InputAccepterGroups.Contains(name))
					InputAccepterGroups.Insert (0, name, input_accepters);
			}
		}
		public void SetLastInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
            lock (InputAccepterGroups) {
				if(!InputAccepterGroups.Contains(name))
				{
					InputAccepterGroups.Add (name, input_accepters);
				}
			}
		}
		public void MixAddInputAccepters (string name, params IInputAccepter[] input_accepters)
		{
            lock (InputAccepterGroups) {
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
            lock (InputAccepterGroups) {
				InputAccepterGroups.Remove (name);
			}
		}
//		public void SetupTests ()
//		{
//			TestWatch.Start();
//		}
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
        /// <summary>
        /// Instantiates and initializes one instnace
        /// of every subclass of Scene in this assembly
        /// </summary>
        public void SetupScenes (params Type[] type_filters)
        {
            if (type_filters.Length == 0)
            type_filters = new Type[] { typeof(Scene) };
            // Brave new world:
            _WorldMain = new World (new Microsoft.Xna.Framework.Vector2 (0.0f, (float)Configuration.ForceDueToGravity));
            // This is EVIL:
            IEnumerable<Type> model_enum = typeof(Scene).FindAllEndClasses ();
            //          foreach (Type m in model_enum) {
            //              Console.WriteLine("Picked {0}", m.Name);
            //          }
            Scene next_scene = _CurrentScene;
            bool redirect = next_scene != null;
            List<object> remove_keys = new List<object> ();
            foreach (object key in _Scenes.Keys) {
                Scene scene = (Scene)_Scenes [key];
                for (int i = 0; i < type_filters.Length; i++) {
                    if (type_filters [i].DescendantOf (scene.GetType ())) {
                        remove_keys.Add (key);
                    }
                }
            }
            foreach (object key in remove_keys) {
                Scene scene = (Scene)_Scenes [key];
                _Scenes.Remove (key);
                if (next_scene == scene && redirect)
                    next_scene = null;
                scene.Dispose ();
            }
            //            foreach (object key in game.Scenes.Keys) {
            //                Scene scene = (Scene)game.Scenes [key];
            //                if (next_scene == null && redirect) {
            //                    next_scene = scene;
            //                    break;
            //                }
            //            }
            List<Scene> new_scenes = new List<Scene> ();
            foreach (Type m in model_enum) {
                for (int i = 0; i < type_filters.Length; i++) {
                    if (type_filters [i].DescendantOf (m)) {
                        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                        ConstructorInfo ctor = m.GetConstructor (flags, null, new Type[] { typeof(PositronGame) }, null);
                        object instanace = ctor.Invoke(new object[] { this });
                        Scene scene = (Scene)instanace;
                        _Scenes.Add (scene.Name, scene);
                        new_scenes.Add (scene);
                    }
                }
            }
            if (next_scene == null) {
                next_scene = (Scene)_Scenes["SceneFirstMenu"];
            }
            foreach (Scene scene in new_scenes)
                scene.InstantiateConnections ();
            foreach (Scene scene in new_scenes)
                scene.InitializeScene ();
            CurrentScene = next_scene; // Change scenes as necessary
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
			lock (UpdateLock) {												// Don't even think about not locking this threaded monstrosity
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
			lock (UpdateLock) {
                Demolish();
				//foreach(Scene scene in Scenes.Values)
				//	scene.Dispose();
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
			lock (UpdateLock) {
				foreach(Scene scene in Scenes.Values)
				{
					scene.Dispose();
				}
				//Dispose ();
			}
		}
	}
}

