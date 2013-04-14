using System;
using System.Diagnostics;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace positron
{
	public class FloorSwitch : SpriteObject, IInteractiveObject
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
			var half_w_h = new Microsoft.Xna.Framework.Vector2 (w * 0.5f, h * 0.5f);
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
				// Start at 6 o'clock
				double tolerance = 0.3;
				double sin = -Math.Cos (Theta);
				double cos = Math.Sin (Theta);
				_LastAffected = Math.Abs(contact.Manifold.LocalNormal.Y - sin) < tolerance &&
						Math.Abs(contact.Manifold.LocalNormal.X - cos) < tolerance;
				if(_LastAffected)
					OnAction(sender, SwitchState.Closed);
			}
			return true;
		}
		protected void HandleOnSeparation (Fixture fixtureA, Fixture fixtureB)
		{
			if(fixtureB.Body.UserData == Program.MainGame.Player1)
			{
				if(_LastAffected)
				{
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

