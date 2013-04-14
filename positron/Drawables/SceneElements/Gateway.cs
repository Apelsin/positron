using System;

namespace positron
{
	public class Gateway : SpriteObject, IInteractiveObject
	{
		public event ActionEventHandler Action;
		protected SharedState<bool> _State;

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
			base(render_set, x, y, Texture.Get("sprite_gateway"))
		{
			Open = new SpriteAnimation(Texture, 50, new int [] {0, 1, 2, 3});
			Close = new SpriteAnimation(Texture, 50, new int [] {3, 2, 1, 0});
			_State = sync_state;
			_State.SharedStateChanged += (sender, e) => 
			{
				PlayAnimation (e.CurrentState ? Open : Close);
				Body.Enabled = (RenderSet.Scene == Program.MainGame.CurrentScene) && !e.CurrentState;
			};
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

