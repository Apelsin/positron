using System;

namespace positron
{
	public class SmallGateway : Gateway
	{
		public SmallGateway (RenderSet render_set, double x, double y, bool initial_state = false):
			this(render_set, x, y, new SharedState<bool>(initial_state))
		{
		}
		public SmallGateway (RenderSet render_set, double x, double y, Gateway sync_gateway):
			this(render_set, x, y, sync_gateway.State)
		{
		}
		protected SmallGateway (RenderSet render_set, double x, double y, SharedState<bool> sync_state):
			base(render_set, x, y, sync_state, Texture.Get("sprite_mini_gate"))
		{
			Open = new SpriteAnimation(Texture, 50, false, "closed", "opening_1", "opening_2", "open");
			Close = new SpriteAnimation(Texture, 50, false, "open", "opening_2", "opening_1", "closed");
		}
	}
}

