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

namespace Positron
{
    public class PositronGame: IDisposable
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
        protected float _DeltaTime;
        public float DeltaTime { get { return _DeltaTime; } }
        protected ThreadedRendering _Window;
        protected Hashtable _Scenes = new Hashtable();
        protected Scene _CurrentScene;
        protected World _WorldMain;
        public float TimeStepCoefficient = 1.0f;
        protected OrderedDictionary InputAccepterGroups;

        #endregion
        #region Static Variables
        #endregion
        #region Member Accessors
        public ThreadedRendering Window { get { return _Window; } }
        public Hashtable Scenes { get { return _Scenes; } }
        public Scene CurrentScene {
            get { return _CurrentScene; }
            set { LoadScene ((Scene)value); }
        }
        public World WorldMain { get { return _WorldMain; } set { _WorldMain = value; } }
        public Camera CurrentCamera { get { return CurrentScene.Camera; } }
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
            //SetupScenes();
            //_CurrentScene = (Scene)_Scenes["SceneFirstMenu"];
        }
        public static void InitialSetup ()
        {
            // Load textures into graphics memory space
            Texture.InitialSetup();
            Sound.InitialSetup();
            //DialogSpeaker.InitialSetup();
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
        protected void ProcessUpdateEventList ()
        {
            lock (_UpdateEventList)
            {
                for(int i = 0; i < _UpdateEventList.Count;)
                {
                    if(_UpdateEventList[i].Value(_UpdateEventList[i].Key, new UpdateEventArgs(i)))
                        _UpdateEventList.RemoveAt(i);
                    else
                        i++;
                }
            }
        }
        public void Update ()
        {
            ProcessUpdateEventList();
            _DeltaTime = TimeStepCoefficient * (float)Math.Round(Window.LastFrameTime, 4);
            _CurrentScene.Update();
            foreach (Xform xform in _CurrentScene.Root.Children)
                xform.mGameObject.Update();
        }
        public void Render()
        {
            foreach (Xform xform in _CurrentScene.Root.Children)
                xform.mGameObject.Render();
        }
        public void LateUpdate()
        {
            foreach (Xform xform in _CurrentScene.Root.Children)
                xform.mGameObject.LateUpdate();
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
            LoadScene(next_scene); // Change scenes as necessary
        }
        protected void LoadScene (Scene next_scene)
        {
            if (_CurrentScene == next_scene || next_scene == null)
                return;
            SceneChangeEventArgs scea = new SceneChangeEventArgs (_CurrentScene, next_scene);

            foreach (Xform xform in next_scene.Root.Children)
            {
                xform.mGameObject.LoadState();
            }
            if (_CurrentScene != null)
            {
                // Persistence only applicable to top-level Xforms
                foreach (Xform xform in _CurrentScene.Root.Children)
                {
                    if (xform.mGameObject.mState.Persist)
                        next_scene.Root.AddChild(xform);
                    else
                        xform.mGameObject.SaveState();
                }
                _CurrentScene.OnSceneExit(this, scea);
            }
            next_scene.OnSceneEntry (this, scea);
            _CurrentScene = next_scene; // Update the scene reference
            GC.Collect();
        }
        public void Dispose ()
        {
            Demolish();
            //foreach(Scene scene in Scenes.Values)
            //    scene.Dispose();
            _Scenes.Clear();
            _Scenes = null;
            InputAccepterGroups.Clear();
            InputAccepterGroups = null;
            _UpdateEventList.Clear();
            _UpdateEventList = null;
            _WorldMain.Clear();
            _WorldMain = null;
        }
        public void Demolish()
        {
            foreach(Scene scene in Scenes.Values)
            {
                scene.Dispose();
            }
        }
    }
}

