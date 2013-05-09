using System;
using System.Diagnostics;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace positron
{
    public enum SwitchState
    {
        Open = 0,
        Latched = 1,
        Closed = 2,
    }
	public class PressureSwitch : SpriteObject, IActuator, IStateShare<SwitchState>
	{
		
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
        public Stopwatch LatchTimer { get { return _LatchTimer; } }

		protected SpriteAnimation TurnOn;
		protected SpriteAnimation TurnOff;

		public PressureSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, double latch_time = 2.0):
			this(render_set, x, y, state_changed, new SharedState<SwitchState>(SwitchState.Open), new SharedState<double>(latch_time), new Stopwatch(), latch_time)
		{
		}
		public PressureSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, PressureSwitch sync_switch):
			this(render_set, x, y, state_changed, sync_switch._State, sync_switch._LatchExpiration, sync_switch._LatchTimer, sync_switch.LatchTime)
		{
		}
		public PressureSwitch (RenderSet render_set, double x, double y, ActionEventHandler state_changed, PressureSwitch sync_switch, double latch_time):
			this(render_set, x, y, state_changed, sync_switch._State, sync_switch._LatchExpiration, sync_switch._LatchTimer, latch_time)
		{
		}
        protected PressureSwitch(RenderSet render_set, double x, double y,
                               ActionEventHandler state_changed,
                               SharedState<SwitchState> state,
                               SharedState<double> latch_expiration,
                               Stopwatch latch_timer, double latch_time) :
            this(render_set, x, y, state_changed, state, latch_expiration, latch_timer, latch_time, Texture.Get("sprite_floor_switch"))
        {
        }
		protected PressureSwitch (RenderSet render_set, double x, double y,
		                       ActionEventHandler state_changed,
		                       SharedState<SwitchState> state,
		                       SharedState<double> latch_expiration,
		                       Stopwatch latch_timer, double latch_time,
                               Texture texture):
			base(render_set, x, y, texture)
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
			_SpriteBody = BodyFactory.CreateBody (_RenderSet.Scene.World, msv2);
			FixtureFactory.AttachRectangle (w, h, 100.0f, new Microsoft.Xna.Framework.Vector2(0.0f, -half_w_h.Y), _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.5f;
			_SpriteBody.OnCollision += HandleOnCollision;
			_SpriteBody.OnSeparation += HandleOnSeparation;

			// HACK: Only enable bodies for which the object is in the current scene
            Body.Enabled = this.RenderSet.Scene == _RenderSet.Scene.Game.CurrentScene;

			InitBlueprints ();
		}

		protected virtual bool HandleOnCollision (Fixture fixtureA, Fixture fixtureB, Contact contact)
		{
			lock (Body) {
				object sender = fixtureB.Body.UserData;
				if (sender is Player) {

					// TODO: implement hit-direction checking CORRECTLY
					// Previous attempts have been awful and thus removed

					if(!_LastAffected)
					{
						_LastAffected = true;
						OnAction (sender, SwitchState.Closed);
					}
					//Console.WriteLine("Collided with switch");
				}
			}
			return true;
		}

        protected virtual void HandleOnSeparation(Fixture fixtureA, Fixture fixtureB)
		{
			lock (Body) {
				object sender = fixtureB.Body.UserData;
				if (sender is Player) {
					if (_LastAffected)
					{
						_LastAffected = false;
						OnAction (fixtureB.Body.UserData, SwitchState.Latched);
					}
					//Console.WriteLine("Separated from switch");
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

