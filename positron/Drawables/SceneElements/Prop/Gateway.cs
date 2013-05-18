using System;

namespace positron
{
	public class Gateway : SpriteObject, IActuator, IStateShare<bool>
	{
		public event ActionEventHandler Action;
		protected SharedState<bool> _State;
		public SharedState<bool> State { get { return _State; } }
		protected SpriteAnimation Open;
		protected SpriteAnimation Close;

		public Gateway (RenderSet render_set, double x, double y, bool initial_state = false):
			this(render_set, x, y, new SharedState<bool>(initial_state))
		{
		}
		public Gateway (RenderSet render_set, double x, double y, Gateway sync_gateway):
			this(render_set, x, y, sync_gateway._State)
		{
		}
		protected Gateway (RenderSet render_set, double x, double y, SharedState<bool> sync_state):
			this(render_set, x, y, sync_state, Texture.Get("sprite_gateway"))
		{
			Open = new SpriteAnimation(Texture, 50, new int [] {0, 1, 2, 3});
			Close = new SpriteAnimation(Texture, 50, new int [] {3, 2, 1, 0});
		}
		protected Gateway (RenderSet render_set, double x, double y, SharedState<bool> sync_state, Texture texture):
			base(render_set, x, y, texture)
		{
			_State = sync_state;
			_State.SharedStateChanged += (sender, e) => 
			{
				PlayAnimation (e.CurrentState ? Open : Close);
                Body.Enabled = (_RenderSet.Scene == _RenderSet.Scene.Game.CurrentScene) && !e.CurrentState;
			};
		}
		protected override void EnteredRenderSet (object sender, RenderSetChangeEventArgs e)
		{
            if(Body != null)
			    Body.Enabled = (Set.Scene == e.To.Scene) && !_State;
			PlayAnimation (_State ? Open : Close);
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
        public override void Dispose()
        {
            Action = null;
            _State = null;
            base.Dispose();
        }
	}
}

