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
    public delegate void SceneEntryEventHandler(object sender, SceneChangeEventArgs e);
	public delegate void SceneExitEventHandler(object sender, SceneChangeEventArgs e);
	public class Scene : IDisposable
	{
		#region Events
		public event SceneEntryEventHandler SceneEntry;
		public event SceneExitEventHandler SceneExit;
		#endregion
		#region State
		#region Member Variables
		protected string _Name;
        protected PositronGame _Game;
		protected Vector2 _ViewSize;
		protected Vector3 _ViewOffset;
		protected Vector3 _ViewPosition;
        protected HUDQuad FrameTimeMeter;
        protected HUDQuad UpdateTimeMeter;
        protected HUDQuad RenderTimeMeter;
        protected HUDQuad RenderDrawingMeter;
        protected SceneRoot _Root;
        public SceneRoot Root { get { return _Root; } }
		protected float[] AdaptiveTimeSteps = new float[12];
		/// <summary>
		/// Index for adaptive time step mode
		/// </summary>
		protected int ATSIndex = 0;
		#endregion
		#region Member Accessors
		public string Name { get { return _Name; } }

		public Vector2 ViewSize { get { return _ViewSize; } }
		public float ViewWidth {
			get { return _ViewSize.X; }
			protected set { _ViewSize.X = value; }
		}
		public float ViewHeight {
			get { return _ViewSize.Y; }
			protected set { _ViewSize.Y = value; }
		}
		public Vector3 ViewOffset {
			get { return _ViewOffset; }
			set { _ViewOffset = value; }
		}
		public Vector3 ViewPosition {
			get { return _ViewPosition; }
			//set { _ViewPosition = value; }
		}
        public PositronGame Game { get { return _Game; } }
		public World World { get { return Game.WorldMain; } }
		#endregion
		#endregion
		#region Behavior
		#region Member
        protected Scene (PositronGame game, string name)
		{
            _Game = game;
            _Name = name;

			// TODO: This is awful. Fix it.
			// TODO: Actually fix this.
			// TODO: Seriously, make this -not suck-
            ViewWidth = _Game.Window.CanvasWidth;
			ViewHeight = _Game.Window.CanvasHeight;

			_Root = new SceneRoot(this);

			SceneEntry += (sender, e) => 
			{
			};
		}
		protected Scene (PositronGame game):
			this(game, "Scene")
		{
			_Name = GetType ().Name;
		}
		public virtual void InstantiateConnections ()
		{
		}
		public virtual void InitializeScene ()
		{
		}
		public void OnSceneEntry (object sender, SceneChangeEventArgs e)
		{
			if(SceneEntry != null)
				SceneEntry(sender, e);
		}
		public void OnSceneExit (object sender, SceneChangeEventArgs e)
		{
			if(SceneExit != null)
				SceneExit(sender, e);
		}
        /*
        protected void SetupHUD()
        {
            var p = new Vector3(5.0f, 5.0f, 0.0f);
            var s = new Vector3(5.0f, 12f, 0.0f);
			FrameTimeMeter = new HUDQuad(HUDDebug, p, s);
            FrameTimeMeter.Color = Color.DarkSlateBlue;
			UpdateTimeMeter = new HUDQuad(HUDDebug, p, s);
            UpdateTimeMeter.Color = Color.DarkCyan;
			RenderTimeMeter = new HUDQuad(HUDDebug, p, s);
            RenderTimeMeter.Color = Color.DarkRed;
			RenderDrawingMeter = new HUDQuad(HUDDebug, p, s);
            RenderDrawingMeter.Color = Color.Red;
        }
        protected void UpdateHUDStats()
        {
            float w_x, w = 4000.0f;
            w_x = w * _Game.Window.LastFrameTime;
            FrameTimeMeter.B.X = FrameTimeMeter.A.X + w_x;
            FrameTimeMeter.C.X = FrameTimeMeter.D.X + w_x;
            w_x = w * _Game.Window.LastUpdateTime;
            UpdateTimeMeter.B.X = FrameTimeMeter.A.X + w_x;
            UpdateTimeMeter.C.X = FrameTimeMeter.D.X + w_x;
            w_x = w * _Game.Window.LastRenderTime;
            RenderTimeMeter.A.X = UpdateTimeMeter.B.X;
            RenderTimeMeter.D.X = UpdateTimeMeter.C.X;
            RenderTimeMeter.B.X = RenderTimeMeter.A.X + w_x;
            RenderTimeMeter.C.X = RenderTimeMeter.D.X + w_x;
            w_x = w * _Game.Window.LastRenderDrawingTime;
            RenderDrawingMeter.A.Y = RenderTimeMeter.A.Y + 3;
            RenderDrawingMeter.B.Y = RenderTimeMeter.B.Y + 3;
            RenderDrawingMeter.C.Y = RenderTimeMeter.C.Y - 3;
            RenderDrawingMeter.D.Y = RenderTimeMeter.D.Y - 3;
            RenderDrawingMeter.A.X = RenderTimeMeter.A.X;
            RenderDrawingMeter.D.X = RenderTimeMeter.D.X;
            RenderDrawingMeter.B.X = RenderDrawingMeter.A.X + w_x;
            RenderDrawingMeter.C.X = RenderDrawingMeter.D.X + w_x;
        }
        */

        public virtual void UpdateWorld()
        {
            if (Configuration.AdaptiveTimeStep)
            {
                AdaptiveTimeSteps [ATSIndex] = Math.Min (Game.DeltaTime, Configuration.MaxWorldTimeStep);
                ATSIndex = (ATSIndex + 1) % AdaptiveTimeSteps.Length;
                float t = AdaptiveTimeSteps [ATSIndex];
                World.Step (t);
            } else
                World.Step(Math.Min(Game.DeltaTime, Configuration.MaxWorldTimeStep));
        }
		public virtual void Update ()
        {
            UpdateWorld();
            foreach (Xform xform in _Root.Children)
                xform.mGameObject.Update();
		}
        public virtual void Render()
        {
            GL.PushMatrix();
            GL.LoadMatrix(ref _Root._Local);
            foreach (Xform xform in _Root.Children)
                xform.mGameObject.Render();
            GL.PopMatrix();
        }
        public virtual void LateUpdate()
        {
            foreach (Xform xform in _Root.Children)
                xform.mGameObject.LateUpdate();
        }
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

