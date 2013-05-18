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

namespace positron
{
	#region EventArgs
	public class RenderSetChangeEventArgs : EventArgs
	{
		public RenderSet From { get; set; }
		public RenderSet To { get; set; }
		public RenderSetChangeEventArgs(RenderSet from, RenderSet to)
		{
			From = from;
			To = to;
		}
	}
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
		protected Vector2d _ViewSize;
		protected Vector3d _ViewOffset;
		protected Vector3d _ViewPosition;
		protected Drawable _FollowTarget;
        protected HUDQuad FrameTimeMeter;
        protected HUDQuad UpdateTimeMeter;
        protected HUDQuad RenderTimeMeter;
        protected HUDQuad RenderDrawingMeter;
		protected Door _DoorToNextScene;
		protected Door _DoorToPreviousScene;
		protected Object _RenderLock = new Object();
		/// <summary>
		/// All of the render sets
		/// </summary>
		//public RenderSet All;
		/// <summary>
		/// The background set is the farthest away from the viewport and is drawn first
		/// </summary>
		public RenderSet Background;
		/// <summary>
		/// The rear set is drawn atop the background and should contain distant objects
		/// </summary>
		public RenderSet Rear;
		/// <summary>
		/// The stage set contains the focal elements of the scene (gameplay critical)
		/// </summary>
		public RenderSet Stage;
		/// <summary>
		/// Tests render set
		/// </summary>
		public RenderSet Tests;
		/// <summary>
		/// The front set is in front of the stage to draw forground objects
		/// </summary>
		public RenderSet Front;
		/// <summary>
		/// WorldBlueprint draws indicatory items on top of the world
		/// and is mainly used for debugging.
		/// </summary>
		public RenderSet WorldBlueprint;
		/// <summary>
		/// The HUD set is the foremost render set and will draw the game's GUI
		/// </summary>
		public RenderSet HUD;
		/// <summary>
		/// I lied when I said HUD is the foremost render set.
		/// HUDBlueprint draws indicatory items on top of everything
		/// and is mainly used for debugging.
		/// </summary>
		public RenderSet HUDBlueprint;
		/// <summary>
		/// Furthermore, there is also this renderset used for drawing
		/// non-blueprint debug visuals
		/// </summary>
		public RenderSet HUDDebug;
		/// <summary>
		/// Time steps are delayed in adaptive time step mode
		/// </summary>
		protected float[] AdaptiveTimeSteps = new float[12];
		/// <summary>
		/// Index for adaptive time step mode
		/// </summary>
		protected int ATSIndex = 0;
		#endregion
		#region Member Accessors
		public string Name { get { return _Name; } }
		public Vector2d ViewSize { get { return _ViewSize; } }
		public double ViewWidth {
			get { return _ViewSize.X; }
			protected set { _ViewSize.X = value; }
		}
		public double ViewHeight {
			get { return _ViewSize.Y; }
			protected set { _ViewSize.Y = value; }
		}
		public Vector3d ViewOffset {
			get { return _ViewOffset; }
			set { _ViewOffset = value; }
		}
		public Vector3d ViewPosition {
			get { return _ViewPosition; }
			//set { _ViewPosition = value; }
		}
        public PositronGame Game { get { return _Game; } }
		public World World { get { return Game.WorldMain; } }
		public Object RenderLock { get { return _RenderLock; } }
		public Drawable FollowTarget { get { return _FollowTarget; } }
		public Door DoorToNextScene { get { return _DoorToNextScene; } }
		public Door DoorToPreviousScene { get { return _DoorToPreviousScene; } }
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

			Background = new RenderSet(this);
			Rear = new RenderSet(this);
			Stage = new RenderSet(this);
			Tests = new RenderSet(this);
			Front = new RenderSet(this);
			WorldBlueprint = new RenderSet(this);
			HUD = new RenderSet(this);
			HUDBlueprint = new RenderSet(this);
			HUDDebug = new RenderSet(this);

			// This should contain everything AllRenderSetsInOrder would contain
			// This is an optimization over using an enumerable
			//All = new RenderSet(this, Background, Rear, Stage, Tests, Front, HUD, HUDBlueprint, HUDDebug);

			SceneEntry += (sender, e) => 
			{
				foreach(RenderSet render_set in AllRenderSetsInOrder())
				{
					render_set.ForEach (element => {
						if(element is IWorldObject)
						{
							IWorldObject w_o = (IWorldObject)element;
							//Console.WriteLine("Rotation for {0} sprite with rotation {1}", w_o, w_o.Theta);
						}
					});
				}
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
			SetupHUD();
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
        protected void SetupHUD()
        {
            var p = new Vector3d(5.0, 5.0, 0.0);
            var s = new Vector3d(5.0, 12, 0.0);
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
            double w_x, w = 4000.0;
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

		public virtual void Update (double time)
        {
            //lock (_RenderLock) {
                if (Configuration.AdaptiveTimeStep) {
                    AdaptiveTimeSteps [ATSIndex] = Math.Min ((float)time, Configuration.MaxWorldTimeStep);
                    ATSIndex = (ATSIndex + 1) % AdaptiveTimeSteps.Length;
                    float t = AdaptiveTimeSteps [ATSIndex];
                    World.Step (t);
                } else
                    World.Step (Math.Min ((float)time, Configuration.MaxWorldTimeStep));
                UpdateHUDStats ();
            //}
		}
		public virtual void Render (double time)
		{
			lock (_RenderLock)
			{
				GL.PushMatrix ();
				{
                    if (_FollowTarget != null)
						CalculatePan((float)time);
                    GL.Translate(Math.Round(_ViewPosition.X), Math.Round(_ViewPosition.Y), Math.Round(_ViewPosition.Z));
					Background.Render (time);
					Rear.Render (time);
					Stage.Render (time);
					Tests.Render (time);
					Front.Render (time);
					if (Configuration.DrawBlueprints)
						WorldBlueprint.Render (time);
				}
				GL.PopMatrix ();
				HUD.Render (time);
				if (Configuration.DrawBlueprints)
					HUDBlueprint.Render (time);
				if(Configuration.ShowDebugVisuals)
					HUDDebug.Render (time);
			}
		}
		protected void CalculatePan (float time)
		{
			double view_size_scaled_x = _ViewSize.X * 0.2;
			double view_size_scaled_y = _ViewSize.Y * 0.2;
			Vector3d pan = new Vector3d (0.5 * ViewWidth, 0.4 * ViewHeight, 0.0) - _FollowTarget.Position;
			if (time > 0.0f) {
				double a = Math.Abs (_ViewPosition.X - pan.X);
				double b = Math.Abs (_ViewPosition.Y - pan.Y);
				a = Helper.SmootherStep (view_size_scaled_x - 128, view_size_scaled_x, a);
				b = Helper.SmootherStep (view_size_scaled_y - 128, view_size_scaled_y, b);
				pan.X = _ViewPosition.X + (pan.X - _ViewPosition.X) * a;
				pan.Y = _ViewPosition.Y + (pan.Y - _ViewPosition.Y) * b;
				float alpha = Math.Min (1.0f, (2000f * (float)time) / (float)Math.Max (50f, (pan - _ViewPosition).Length));
				pan = Vector3d.Lerp (_ViewPosition, pan, alpha);
				//if(typeof(Player) == typeof(Player))
				//	pan -= ((Player)_FollowTarget).DampVeloNormal * 50;
			}
			_ViewPosition = pan;
		}
		public IEnumerable<RenderSet> AllRenderSetsInOrder()
		{
			yield return Background;
			yield return Rear;
			yield return Stage;
			yield return Tests;
			yield return Front;
			yield return WorldBlueprint;
			yield return HUD;
			yield return HUDBlueprint;
			yield return HUDDebug;
		}
		public IEnumerable<RenderSet> UpdateRenderSetsInOrder()
		{
			yield return Background;
			yield return Rear;
			yield return Stage;
			yield return Tests;
			yield return Front;
			yield return HUD;
		}
		public void Follow (Drawable follow_target)
		{
			Follow(follow_target, false);
		}
		public void Follow (Drawable follow_target, bool cut)
		{
			_FollowTarget = follow_target;
			if (cut && _FollowTarget != null)
				CalculatePan (1.0f);
		}
		public void RayCast (RayCastCallback callback, Microsoft.Xna.Framework.Vector2 point1, Microsoft.Xna.Framework.Vector2 point2)
		{
			World.RayCast (callback, point1, point2);
			if (Configuration.DrawBlueprints) {
				lock(_RenderLock)
				{
					new BlueprintLine (
					new Vector3d (point1.X * Configuration.MeterInPixels, point1.Y * Configuration.MeterInPixels, 0.0),
					new Vector3d (point2.X * Configuration.MeterInPixels, point2.Y * Configuration.MeterInPixels, 0.0),
						WorldBlueprint);
				}
			}
		}
		public List<Fixture> TestPointAll (Microsoft.Xna.Framework.Vector2 point)
		{
			List<Fixture> hit_fixture = World.TestPointAll (point);
			if (Configuration.DrawBlueprints) {
				lock(_RenderLock)
				{
					double cross = 2;
					new BlueprintLine (
						new Vector3d (point.X * Configuration.MeterInPixels - cross, point.Y * Configuration.MeterInPixels - cross, 0.0),
						new Vector3d (point.X * Configuration.MeterInPixels + cross, point.Y * Configuration.MeterInPixels + cross, 0.0),
						WorldBlueprint);
					new BlueprintLine (
						new Vector3d (point.X * Configuration.MeterInPixels - cross, point.Y * Configuration.MeterInPixels + cross, 0.0),
						new Vector3d (point.X * Configuration.MeterInPixels + cross, point.Y * Configuration.MeterInPixels - cross, 0.0),
						WorldBlueprint);
				}
			}
			return hit_fixture;
		}
		protected void SetupPlayerOnExit()
		{
			SceneExit += (sender, e) => {
				if(e.To is ISceneGameplay)
				{
                    _Game.AddUpdateEventHandler(this, (sender1, e1) => {
                        Scene scene_context = e.To;
                        double start_x = 0, start_y = 0;
                        if(scene_context.DoorToPreviousScene != null)
                        {
                            start_x = scene_context.DoorToPreviousScene.CornerX;
                            start_y = scene_context.DoorToPreviousScene.CornerY;
                        }
                        //new Spidey(e.To.Stage, start_x, start_y);
                        
                        //
                        // Setup Player 1
                        //
                        
                        Player player_1 = _Game.Player1;
                        if(player_1 != null)
                        {
                            player_1.Derez();
                        }
                        player_1 = _Game.Player1 = new Player(scene_context.Stage, start_x, start_y, Texture.Get("sprite_player"));
                        player_1.CornerY += 32;
                        scene_context.Follow(player_1, true);
                        _Game.SetLastInputAccepters("Player1", new IInputAccepter[] { player_1 });
                        Follow(player_1);
                        
                        var health_meter = new HealthMeter(scene_context.HUD, 64, ViewHeight - 64, player_1);
                        health_meter.Preserve = true;
                        
                        player_1.DerezEvent += (sender3, e3) => {
                            _Game.RemoveInputAccepters("Player1");
                            health_meter.Set.Remove(health_meter);
                        };
                        return true;
                    });
				}
			};
		}
		public virtual void Dispose ()
        {
            SceneEntry = null;
            SceneExit = null;
            foreach(RenderSet render_set in AllRenderSetsInOrder())
            {
                foreach(IRenderable renderable in render_set)
                    renderable.Dispose();
                render_set.Dispose();
            }
		}
		#endregion
		#endregion
	}
}

