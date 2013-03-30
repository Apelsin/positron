using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using FarseerPhysics.Dynamics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace positron
{
	public class Scene
	{
		#region State
		#region Member Variables
		protected string Name;
		protected Vector2d _ViewSize;
		protected Vector3d _ViewPosition;
		protected World _World;
		protected Drawable _FollowTarget;
        protected HUDQuad FrameTimeMeter;
        protected HUDQuad UpdateTimeMeter;
        protected HUDQuad RenderTimeMeter;
        protected HUDQuad RenderDrawingMeter;
		protected Object _RenderLock = new Object();
		/// <summary>
		/// All of the render sets
		/// </summary>
		public RenderSet All;
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
		/// Time steps are delayed in adaptive time step mode
		/// </summary>
		protected float[] AdaptiveTimeSteps = new float[12];
		/// <summary>
		/// Index for adaptive time step mode
		/// </summary>
		protected int ATSIndex = 0;
		protected Vector3d ScenePan;
		#endregion
		#region Static Variables
		protected static Hashtable Scenes = new Hashtable();
		#endregion
		#region Member Accessors
		public Vector2d ViewSize { get { return _ViewSize; } }
		public double ViewWidth {
			get { return _ViewSize.X; }
			private set { _ViewSize.X = value; }
		}
		public double ViewHeight {
			get { return _ViewSize.Y; }
			private set { _ViewSize.Y = value; }
		}
		public Vector3d ViewPosition {
			get { return _ViewPosition; }
			set { _ViewPosition = value; }
		}
		public World World { get { return _World; } }
		public Object RenderLock { get { return _RenderLock; } }
		#endregion
		#region Static Accessors
		#endregion
		#endregion
		#region Behavior
		#region Member
		protected Scene (string name)
		{
			// TODO: This is awful. Fix it.
			// TODO: Actually fix this.
			// TODO: Seriously, make this -not suck-
			ViewWidth = Program.MainWindow.CanvasWidth;
			ViewHeight = Program.MainWindow.CanvasHeight;

			Background = new RenderSet(this);
			Rear = new RenderSet(this);
			Stage = new RenderSet(this);
			Tests = new RenderSet(this);
			Front = new RenderSet(this);
			WorldBlueprint = new RenderSet(this);
			HUD = new RenderSet(this);
			HUDBlueprint = new RenderSet(this);

			// This should contain everything AllRednerSetsInOrder would contain
			// This is an optimization over using an enumerable
			All = new RenderSet(this, Background, Rear, Stage, Tests, Front, HUD, HUDBlueprint);

			// This is a little irritating but I'll just have to deal with it for now:
			Microsoft.Xna.Framework.Vector2 gravity =
				new Microsoft.Xna.Framework.Vector2(0.0f, (float)Configuration.ForceDueToGravity);

			_World = new World(gravity);
			//_World.EnableSubStepping = true;

            SetupHUD();
		}
        private void SetupHUD()
        {
            var p = new Vector3d(5.0, 5.0, 0.0);
            var s = new Vector3d(5.0, 12, 0.0);
            FrameTimeMeter = new HUDQuad(p, s, HUDBlueprint, 0);
            FrameTimeMeter.Color = Color.DarkSlateBlue;
            UpdateTimeMeter = new HUDQuad(p, s, HUDBlueprint, 0);
            UpdateTimeMeter.Color = Color.DarkCyan;
            RenderTimeMeter = new HUDQuad(p, s, HUDBlueprint, 0);
            RenderTimeMeter.Color = Color.DarkRed;
            RenderDrawingMeter = new HUDQuad(p, s, HUDBlueprint, 0);
            RenderDrawingMeter.Color = Color.Red;
        }
        private void UpdateHUDStats()
        {
            double w_x, w = 4000.0;
            w_x = w * Program.MainWindow.LastFrameTime;
            FrameTimeMeter.B.X = FrameTimeMeter.A.X + w_x;
            FrameTimeMeter.C.X = FrameTimeMeter.D.X + w_x;
            w_x = w * Program.MainWindow.LastUpdateTime;
            UpdateTimeMeter.B.X = FrameTimeMeter.A.X + w_x;
            UpdateTimeMeter.C.X = FrameTimeMeter.D.X + w_x;
            w_x = w * Program.MainWindow.LastRenderTime;
            RenderTimeMeter.A.X = UpdateTimeMeter.B.X;
            RenderTimeMeter.D.X = UpdateTimeMeter.C.X;
            RenderTimeMeter.B.X = RenderTimeMeter.A.X + w_x;
            RenderTimeMeter.C.X = RenderTimeMeter.D.X + w_x;
            w_x = w * Program.MainWindow.LastRenderDrawingTime;
            RenderDrawingMeter.A.Y = RenderTimeMeter.A.Y + 3;
            RenderDrawingMeter.B.Y = RenderTimeMeter.B.Y + 3;
            RenderDrawingMeter.C.Y = RenderTimeMeter.C.Y - 3;
            RenderDrawingMeter.D.Y = RenderTimeMeter.D.Y - 3;
            RenderDrawingMeter.A.X = RenderTimeMeter.A.X;
            RenderDrawingMeter.D.X = RenderTimeMeter.D.X;
            RenderDrawingMeter.B.X = RenderDrawingMeter.A.X + w_x;
            RenderDrawingMeter.C.X = RenderDrawingMeter.D.X + w_x;
        }
//		public bool CheckIn(object o, )
//		{
//		}
		public virtual void Update (double time)
		{
			// Update the world!
			// TODO: Make this good
			if (Configuration.AdaptiveTimeStep) {
				AdaptiveTimeSteps[ATSIndex] = Math.Min ((float)time, Configuration.MaxWorldTimeStep);
				ATSIndex = (ATSIndex + 1) % AdaptiveTimeSteps.Length;
				float t = AdaptiveTimeSteps[ATSIndex];
				_World.Step(t);
			}
			else
				_World.Step(Math.Min ((float)time, Configuration.MaxWorldTimeStep));
            UpdateHUDStats();
		}
		public void Render (double time)
		{
			lock (_RenderLock)
			{
				GL.PushMatrix ();
				{
                    if (_FollowTarget != null)
                    {
                        Vector3d pan = new Vector3d(0.5 * ViewWidth, 0.375 * ViewHeight, 0.0) - _FollowTarget.Position;
                        float a = (float)time + 1.0f / Math.Max(1.0f, (float)(pan - ScenePan).LengthFast);
                        ScenePan = Vector3d.Lerp(ScenePan, pan, a);
                        GL.Translate(Math.Round(ScenePan.X), Math.Round(ScenePan.Y), Math.Round(ScenePan.Z));
                        //GL.Translate(ScenePan);
                    }
					Background.Render (time);
					Rear.Render (time);
					Stage.Render (time);
					Tests.Render (time);
					Front.Render (time);
					WorldBlueprint.Render (time);
				}
				GL.PopMatrix ();
				HUD.Render (time);
				HUDBlueprint.Render (time);
			}
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
		}
		/// <summary>
		/// Includes preserved <see cref="IWorldObject"/>World Object</summary>s from previous scene
		/// </summary>
		protected void PreservationTransfer (Scene previous_scene)
		{
			// TODO: Make this better
			/*
			// Get the enumerable for the render sets; these need to be aligned
			IEnumerable<RenderSet> previous_sets = previous_scene.AllRenderSetsInOrder();
			IEnumerable<RenderSet> these_sets = AllRenderSetsInOrder();
			IEnumerator<RenderSet> this_set_enum = these_sets.GetEnumerator();
			foreach (RenderSet r in previous_sets)				// For each render set
			{
				foreach(RenderSet r__r in r)					// For each render set in render set
				{
					if(r__r is IWorldObject)					// If renderable is world object
					{
						IWorldObject w = (IWorldObject)r__r;	// Cast to world object
						if(w.Preserve) 							// If world object is preserved
						{
							this_set_enum.Current.Add(r__r);	// Add in this renderable/world object
							SetChangeEventArgs scea = new SetChangeEventArgs();
							scea.From = r__r;
							scea.To = r__r;
							w.SetChange(null, scea);
						}
					}
				}
				if(!this_set_enum.MoveNext())					// Advance enumerator; if passed end...
					break;										// ...get the hell out of Dodge
			}
			*/
		}
		public void Follow (Drawable follow_target)
		{
			_FollowTarget = follow_target;
			ScenePan = _FollowTarget.Position;
		}
		public void RayCast (RayCastCallback callback, Microsoft.Xna.Framework.Vector2 point1, Microsoft.Xna.Framework.Vector2 point2)
		{
			_World.RayCast (callback, point1, point2);
			if (Configuration.DrawBlueprints) {
				lock(RenderLock)
				{
					WorldBlueprint.Add (
						new BlueprintLine (
						new Vector3d (point1.X * Configuration.MeterInPixels, point1.Y * Configuration.MeterInPixels, 0.0),
						new Vector3d (point2.X * Configuration.MeterInPixels, point2.Y * Configuration.MeterInPixels, 0.0),
							WorldBlueprint));
				}
			}
		}
		#endregion
		#region Static
		public static Scene Create (string name)
		{
			Scene scene = new Scene(name);
			Scenes.Add(name, scene);
			return scene;
		}
		#endregion
		#endregion
	}
	public class SetChangeEventArgs : EventArgs
	{
		public RenderSet From { get; set; }
		public RenderSet To { get; set; }
	}
}

