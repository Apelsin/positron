using System;
using System.Diagnostics;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace positron
{
	public class FloorSwitch : SpriteObject, IActuator
	{
		public enum SwitchState
		{
			Open = 0,
			Latched = 1,
			Closed = 2,
		}
		public event ActionEventHandler Action;
		protected SharedState<SwitchState> _State;
		protected SharedState<double> _LatchExpiration;
		protected bool _LastAffected = false;
		protected double _LatchTime;
		protected Vector2 HalfWH;

		//protected object _LastSender;
		protected Stopwatch _LatchTimer;
		public double LatchTime { get { return _LatchTime; } set { _LatchTime = value; } }
		public SharedState<double> LatchExpiration { get { return _LatchExpiration; } }
		public SharedState<SwitchState> State { get { return _State; } }

		protected SpriteAnimation TurnOn;
		protected SpriteAnimation TurnOff;

		public FloorSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, double latch_time = 2.0):
			this(render_set, x, y, state_changed, new SharedState<SwitchState>(SwitchState.Open), new SharedState<double>(latch_time), new Stopwatch(), latch_time)
		{
		}
		public FloorSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, FloorSwitch sync_switch):
			this(render_set, x, y, state_changed, sync_switch._State, sync_switch._LatchExpiration, sync_switch._LatchTimer, sync_switch.LatchTime)
		{
		}
		public FloorSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, FloorSwitch sync_switch, double latch_time):
			this(render_set, x, y, state_changed, sync_switch._State, sync_switch._LatchExpiration, sync_switch._LatchTimer, latch_time)
		{
		}
		protected FloorSwitch (RenderSet render_set, double x, double y,
		                       ActionEventHandler state_changed,
		                       SharedState<SwitchState> state,
		                       SharedState<double> latch_expiration,
		                       Stopwatch latch_timer, double latch_time):
			base(render_set, x, y, Texture.Get("sprite_floor_switch"))
		{
			TurnOn = new SpriteAnimation(Texture, 50, new int [] {0, 1, 2, 3});
			TurnOff = new SpriteAnimation(Texture, 50, new int [] {3, 2, 1, 0});
			_LatchTimer = latch_timer;
			_LatchTime = latch_time;
			_LatchExpiration = latch_expiration;
			_State = state;
			_State.SharedStateChanged += (sender, e) => {
				switch(e.CurrentState){
				case SwitchState.Open:
					PlayAnimation(TurnOff);
					break;
				case SwitchState.Closed:
					PlayAnimation(TurnOn);
					break;
				}
				state_changed(sender, new ActionEventArgs(e.CurrentState, this));
			};
		}
		protected override void InitPhysics ()
		{
			float w, h;
			var size = Texture.Regions [0].Size;
			w = (float)(_Scale.X * size.X / Configuration.MeterInPixels);
			h = (float)(_Scale.Y * size.Y / Configuration.MeterInPixels);
			h *= 0.5f; // Floor switch
			HalfWH = new Vector2 (w * 0.5f, h * 0.5f); // Please kill me
			var half_w_h = new Microsoft.Xna.Framework.Vector2(HalfWH.X, HalfWH.Y);
			var msv2 = new Microsoft.Xna.Framework.Vector2 (
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody (_RenderSet.Scene.World, msv2 + half_w_h);
			FixtureFactory.AttachRectangle (w, h, 100.0f, new Microsoft.Xna.Framework.Vector2(0.0f, -half_w_h.Y), _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.5f;
			_SpriteBody.OnCollision += HandleOnCollision;
			_SpriteBody.OnSeparation += HandleOnSeparation;

			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = this.RenderSet.Scene == Program.MainGame.CurrentScene;

			ConnectBody ();
		}

		protected bool HandleOnCollision (Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			object sender = fixtureB.Body.UserData;
			if(sender == Program.MainGame.Player1)
			{
				// Using the contact manifold normal has proven to be
				// a huge trouble due to unexplained bugginess when
				// enabling/disabling the body in the world
				// TL;DR ray cast FTW

				Vector2 p0, p1, c0, c1;
				float pixel = 1.0f / (float)Configuration.MeterInPixels;
				float pixel2 = 5.0f * pixel;
				// Top edge
				p0 = HalfWH;
				p0.X -= pixel;
				p0.Y += pixel; // Upward offset
				p1 = p0;
				p1.X *= -1.0f;
				c0 = new Vector2(0.0f, HalfWH.Y - pixel2);
				c1 = new Vector2(0.0f, HalfWH.Y + pixel2);

				// Do some math (probably overkill)
				var q = Quaternion.FromAxisAngle(Vector3.UnitZ, (float)Theta);
				p0 = Vector2.Transform(p0, q);
				p1 = Vector2.Transform(p1, q);
				c0 = Vector2.Transform(c0, q);
				c1 = Vector2.Transform(c1, q);

				var p0_xna = new Microsoft.Xna.Framework.Vector2(p0.X + Body.Position.X, p0.Y + Body.Position.Y);
				var p1_xna = new Microsoft.Xna.Framework.Vector2(p1.X + Body.Position.X, p1.Y + Body.Position.Y);
				var c0_xna = new Microsoft.Xna.Framework.Vector2(c0.X + Body.Position.X, c0.Y + Body.Position.Y);
				var c1_xna = new Microsoft.Xna.Framework.Vector2(c1.X + Body.Position.X, c1.Y + Body.Position.Y);

				RayCastCallback callback = (fixture, point, normal, fraction) => {
					if(!_LastAffected)
					{
						OnAction(sender, SwitchState.Closed);
						_LastAffected = true;
					}
					return 0;
				};
				//Console.WriteLine ("p0_xna: {0}", p0_xna);
				//Console.WriteLine ("p1_xna: {0}", p1_xna);
				_RenderSet.Scene.RayCast(callback, p0_xna, p1_xna);
				_RenderSet.Scene.RayCast(callback, p1_xna, p0_xna);
				_RenderSet.Scene.RayCast(callback, c0_xna, c1_xna);
		
			}
			return true;
		}

		protected void HandleOnSeparation (Fixture fixtureA, Fixture fixtureB)
		{
			if(fixtureB.Body.UserData == Program.MainGame.Player1)
			{
				if(_LastAffected)
				{
					_LastAffected = false;
					OnAction(fixtureB.Body.UserData, SwitchState.Latched);
				}
			}
		}
		protected void OnAction(SwitchState state)
		{
			OnAction(this, state);
		}
		public void OnAction(object sender, SwitchState state)
		{
			OnAction(sender, new ActionEventArgs(state));
		}
		public void OnAction (object sender, ActionEventArgs e)
		{
			e.Self = this;
			SwitchState state = (SwitchState)e.Info;
			if(state == SwitchState.Closed)
			{
				if(_State != SwitchState.Closed)
				{
					_State.OnChange (sender, (SwitchState)e.Info);
				}
			}
			else if(_State == SwitchState.Closed)
			{
				//_LastSender = sender;
				_State.OnChange(sender, SwitchState.Latched);
				_LatchExpiration.OnChange(this,  Math.Max (_LatchExpiration, _LatchTimer.Elapsed.TotalSeconds + _LatchTime));
				_LatchTimer.Start();
			}
			if (Action != null) {
				Action (sender, e);
			}
		}
		public override void Update (double time)
		{
			base.Update (time);
			if (_State == SwitchState.Latched) {
				if (_LatchTimer.Elapsed.TotalSeconds > LatchExpiration) {
					var e = new ActionEventArgs (SwitchState.Open, this);
					if (Action != null)
						Action (this, e);
					_State.OnChange (this, (SwitchState)e.Info);
					_LatchTimer.Stop ();
					_LatchTimer.Reset ();
					_LatchExpiration.OnChange(this, 0.0);
				}
			}
		}
	}
}

