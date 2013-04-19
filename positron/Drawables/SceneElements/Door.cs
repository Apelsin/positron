using System;

namespace positron
{
	public class Door : SpriteObject, IActuator
	{
		public event ActionEventHandler Action;
		protected Scene _NextScene;
		protected Door _Destination;
		public Scene NextScene {
			get { return _NextScene; }
			set { _NextScene = value; }
		}
		public Door Destination {
			get { return _Destination; }
			set { _Destination = value; }
		}
		public Door (RenderSet render_set, double x, double y, Door destination):
			this(render_set, x, y, destination._RenderSet.Scene)
		{
			_Destination = destination;
		}
		public Door (RenderSet render_set, Door destination):
			this(render_set, destination.PositionX, destination.PositionY, destination)
		{
		}
		public Door (RenderSet render_set, double x, double y, Scene next_scene):
			base(render_set, x, y, Texture.Get("sprite_doorway"))
		{
			_SpriteBody.CollisionCategories = FarseerPhysics.Dynamics.Category.None;
			_NextScene = next_scene;
		}
		public void OnAction (object sender, ActionEventArgs e)
		{
			e.Self = this;
			if (sender is Player) {
				Player player = (Player)sender;
				if(_Destination != null)
				{
					// TODO: make this work
					//player.Position = _Destination._Position + player.Position - Position;
				}
				Program.MainGame.ChangeScene(_Destination == null ? _NextScene == null ?  null : _NextScene : _Destination.RenderSet.Scene);
			}
			if(Action != null)
				Action(sender, e);
		}
	}
}

