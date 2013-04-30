using System;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace positron
{
	public class ExtenderPlatform : SpriteObject, IActuator, IStateShare<bool>
	{
		public event ActionEventHandler Action;
		protected SharedState<bool> _State;
		public SharedState<bool> State { get { return _State; } }

		protected SpriteAnimation Extend;
		protected SpriteAnimation Collapse;

		public ExtenderPlatform (RenderSet render_set, double x, double y, bool initial_state = false):
			this(render_set, x, y, new SharedState<bool>(initial_state))
		{
		}
		public ExtenderPlatform (RenderSet render_set, double x, double y, IStateShare<bool> sync_share):
			this(render_set, x, y, sync_share == null ? new SharedState<bool>(false) : sync_share.State)
		{
		}
		protected ExtenderPlatform (RenderSet render_set, double x, double y, SharedState<bool> sync_state):
			base(render_set, x, y, Texture.Get("sprite_extender_platform"))
		{
			Extend = new SpriteAnimation(Texture, 50, false, false, new[] { "collapsed", "t1", "t2", "extended" });
			Collapse = new SpriteAnimation(Texture, 50, false, false, new[] { "extended", "t2", "t1", "collapsed" });
			_State = sync_state;
			RenderSetEntry += (sender, e) => {
				PlayAnimation(_AnimationDefault = new SpriteAnimation(false, false, _State ? Collapse.Frames[0] : Extend.Frames[0]));
			};
			_State.SharedStateChanged += (sender, e) => 
			{
				PlayAnimation (e.CurrentState ? Extend : Collapse);
				Body.Enabled = (RenderSet.Scene == Program.MainGame.CurrentScene) && e.CurrentState;
			};
		}
		protected override void EnteredRenderSet (object sender, RenderSetChangeEventArgs e)
		{
			Body.Enabled = (RenderSet.Scene == e.To.Scene) && _State;
		}
		protected override void InitPhysics()
		{
			float w, h;
			var size = Texture.DefaultRegion.Size;
			w = (float)(_Scale.X * size.X / Configuration.MeterInPixels);
			h = (float)(_Scale.Y * size.Y / Configuration.MeterInPixels);
			var half_w_h = new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f);
			var msv2 = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, msv2, this);
			FixtureFactory.AttachRectangle(w, h * 0.1f, 100.0f, new Microsoft.Xna.Framework.Vector2(0.0f, h * 0.9f * 0.5f), _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.2f;
			
			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = this.RenderSet.Scene == Program.MainGame.CurrentScene;
			
			InitBlueprints();
		}
		public void OnAction (object sender, ActionEventArgs e)
		{
			bool state = (bool)e.Info;
			if (state && !_State) {
				_State.OnChange(sender, true);
			}else if(!state && _State) {
				_State.OnChange(sender, false);
			}
			if(Action != null)
				Action(sender, e);
		}
	}
}

