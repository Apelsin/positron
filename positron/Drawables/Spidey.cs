using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace positron
{
	public class Spidey : SpriteObject
	{
		public Spidey (RenderSet render_set, double x, double y):
			base(render_set, x, y, Texture.Get ("sprite_spidey_0"), Texture.Get ("sprite_spidey_1"))
		{
			this.Body.OnCollision += HandleOnCollision;
		}

		bool HandleOnCollision (Fixture fixtureA, Fixture fixtureB, Contact contact)
		{

			// NOTE: Placeholder/test
			if (fixtureB.Body == Program.Game.Player1.Body) {
				if(!Program.Game.TestDialog.Shown)
					Program.Game.TestDialog.Begin();
			}
			return true;
		}
	}
}

