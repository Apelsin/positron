using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection; // Please kill me.

using OpenTK;
using OpenTK.Graphics.OpenGL;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace Positron
{
	#region EventArgs
	public class SceneChangeEventArgs : EventArgs
	{
		public Scene From { get; set; }
		public Scene To { get; set; }
		public SceneChangeEventArgs(Scene from, Scene to)
		{
			From = from;
			To = to;
		}
	}
	#endregion
    #region Delegates
    public delegate void SceneEntryEventHandler(object sender, SceneChangeEventArgs e);
	public delegate void SceneExitEventHandler(object sender, SceneChangeEventArgs e);
    #endregion
    public class Scene : IDisposable
	{
		#region Events
        /// <summary>
        /// Event raised when this scene enters
        /// </summary>
		public event SceneEntryEventHandler SceneEntry;
        /// <summary>
        /// Event raised when this scene exist
        /// </summary>
		public event SceneExitEventHandler SceneExit;
		#endregion
		#region State
		#region Member Variables
		protected string _Name;
        protected PositronGame _Game;
        protected SceneRoot _Root;
        protected HUDQuad FrameTimeMeter;
        protected HUDQuad UpdateTimeMeter;
        protected HUDQuad RenderTimeMeter;
        protected HUDQuad RenderDrawingMeter;
        
		protected float[] AdaptiveTimeSteps = new float[12];
		/// <summary>
		/// Index for adaptive time step mode
		/// </summary>
		protected int ATSIndex = 0;
		#endregion
		#region Member Accessors
        /// <summary>
        /// Name of the current scene
        /// </summary>
		public string Name { get { return _Name; } }
        /// <summary>
        /// Game object associated with this scene; the game this scene belongs to
        /// </summary>
        public PositronGame Game { get { return _Game; } }
        /// <summary>
        /// The root Xform of the scene that contains the hierarchy of GameObjects
        /// </summary>
        public SceneRoot Root { get { return _Root; } }
        /// <summary>
        /// Physical world associated with the scene
        /// </summary>
		public World World { get { return Game.WorldMain; } }
        /// <summary>
        /// Camera used by the scene
        /// </summary>
        public Camera Camera { get { return Root.mCamera; } set { Root.mCamera = value; } }
		#endregion
		#endregion
		#region Behavior
		#region Member
        /// <summary>
        /// Creates a new scene object that can be used by a PositronGame object
        /// </summary>
        /// <param name="game">Associated PositronGame object</param>
        /// <param name="name">Name for the scene</param>
        protected Scene (PositronGame game, string name)
		{
            _Game = game;
            _Name = name;
			_Root = new SceneRoot(this);
		}
        /// <summary>
        /// Creates a new scene object that can be used by a PositronGame object
        /// </summary>
        /// <param name="game">Associated PositronGame object</param>
		protected Scene (PositronGame game):
			this(game, "Scene")
		{
		}
        /// <summary>
        /// Notify subscribers of the SceneEntry event that the scene has entered
        /// </summary>
		public void OnSceneEntry (object sender, SceneChangeEventArgs e)
		{
			if(SceneEntry != null)
				SceneEntry(sender, e);
		}
        /// <summary>
        /// Notify subscribers of the SceneExit event that the scene has exited
        /// </summary>
		public void OnSceneExit (object sender, SceneChangeEventArgs e)
		{
			if(SceneExit != null)
				SceneExit(sender, e);
		}
        /// <summary>
        /// Update the physical simulation by performing a time step
        /// </summary>
        public virtual void UpdateWorld()
        {
            if (Configuration.AdaptiveTimeStep)
            {
                AdaptiveTimeSteps [ATSIndex] = Math.Min (Game.DeltaTime, Configuration.MaxWorldTimeStep);
                ATSIndex = (ATSIndex + 1) % AdaptiveTimeSteps.Length;
                float t = AdaptiveTimeSteps [ATSIndex];
                World.Step (t);
            } else if(World != null)
                World.Step(Math.Min(Game.DeltaTime, Configuration.MaxWorldTimeStep));
        }
        /// <summary>
        /// Perform the main update for this scene
        /// </summary>
		public virtual void Update ()
        {
            UpdateWorld();
            foreach (Xform xform in _Root.Children)
                xform.mGameObject.Update();
		}
        /// <summary>
        /// Render the current scene
        /// </summary>
        public virtual void Render()
        {
            GL.LoadMatrix(ref _Root._Local);
            foreach (Xform xform in _Root.Children)
                xform.mGameObject.Render();
        }
        /// <summary>
        /// Perform the post-render update for this scene. Physical calculations happen here.
        /// </summary>
        public virtual void LateUpdate()
        {
            foreach (Xform xform in _Root.Children)
                xform.mGameObject.LateUpdate();
        }
        /// <summary>
        /// Perform a ray cast test in the current <see cref="World"/>.
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
		public void RayCast (Physics.RayCastCallback callback, Microsoft.Xna.Framework.Vector2 point1, Microsoft.Xna.Framework.Vector2 point2)
		{
			World.RayCast (callback.Invoke, point1, point2);
            /*
			if (Configuration.DrawBlueprints) {
                lock (_Game.UpdateLock)
				{
					new BlueprintLine (
					    new Vector3 (point1.X * Configuration.MeterInPixels, point1.Y * Configuration.MeterInPixels, 0.0f),
					    new Vector3 (point2.X * Configuration.MeterInPixels, point2.Y * Configuration.MeterInPixels, 0.0f));
				}
			}
            */
		}
        /// <summary>
        /// Hit-test a point in <see cref="World"/>-space on all fixtures
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
		public List<Fixture> TestPointAll (Microsoft.Xna.Framework.Vector2 point)
		{
			List<Fixture> hit_fixture = World.TestPointAll (point);
            /*
			if (Configuration.DrawBlueprints) {
                lock (_Game.UpdateLock)
				{
					float cross = 2;
					new BlueprintLine (
						new Vector3 (point.X * Configuration.MeterInPixels - cross, point.Y * Configuration.MeterInPixels - cross, 0.0f),
						new Vector3 (point.X * Configuration.MeterInPixels + cross, point.Y * Configuration.MeterInPixels + cross, 0.0f));
                    new BlueprintLine(
                        new Vector3(point.X * Configuration.MeterInPixels - cross, point.Y * Configuration.MeterInPixels + cross, 0.0f),
                        new Vector3(point.X * Configuration.MeterInPixels + cross, point.Y * Configuration.MeterInPixels - cross, 0.0f));
				}
			}
            */
			return hit_fixture;
		}
		public virtual void Dispose ()
        {
            SceneEntry = null;
            SceneExit = null;
            _Root.Dispose();
		}
		#endregion
		#endregion
	}
}

