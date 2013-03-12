using System;
using System.Collections;
using System.Collections.Generic;
using OpenTK.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace positron
{
	public class Player : SpriteObject, IInputAccepter
	{
		public bool OnPlatform { get; set; }
		#region Behavior
		public Player(Scene scene):
			this(0.0, 0.0, 1.0, 1.0, Texture.DefaultTexture, Program.Game.CurrentScene)
		{
		}
		public Player(Texture texture):
			this(0.0, 0.0, 1.0, 1.0, texture, Program.Game.CurrentScene)
		{
		}
		public Player (double x, double y):
			this(x, y, 1.0, 1.0, Texture.DefaultTexture, Program.Game.CurrentScene)
		{		
		}
		public Player (double x, double y, Texture texture):
			this(x, y, 1.0, 1.0, texture, Program.Game.CurrentScene)
		{		
		}
		// Main constructor:
		public Player (double x, double y, double scalex, double scaley, Texture texture, Scene scene):
			base(x, y, scalex, scaley, texture, scene)
		{
			OnPlatform = false;
			_Scene = scene;
			Program.MainWindow.Keyboard.KeyUp += KeyUp;
			Program.MainWindow.Keyboard.KeyDown += KeyDown;
			Program.MainWindow.KeysUpdate += KeysUpdate;
		}
		protected override void InitPhysics ()
		{
			base.InitPhysics();
			_SpriteBody.BodyType = BodyType.Dynamic;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Mass = 100.0f;
			Body.OnCollision += HandleOnCollision;
			Body.OnSeparation += HandleOnSeparation;
		}

		protected bool HandleOnCollision (Fixture fixure_a, Fixture fixture_b, Contact contact)
		{
			return true;
		}
		protected void HandleOnSeparation (Fixture fixture_a, Fixture fixture_b)
		{
		}
		public void KeyDown (object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Space) {
				Jump();
			}
		}
		public void KeyUp(object sender, KeyboardKeyEventArgs e)
		{
		}
		public void KeysUpdate(object sender, KeysUpdateEventArgs e)
		{
			float vx = (e.KeysPressedWhen.Contains(Key.A) ? -1.0f : 0.0f) + (e.KeysPressedWhen.Contains(Key.D) ? 1.0f : 0.0f);
			//float vy = (e.KeysPressed.Contains(Key.S) ? -1.0f : 0.0f) + (e.KeysPressed.Contains(Key.W) ? 1.0f : 0.0f);
			vx *= 5f;
			//vy *= 10f;
			//this.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(vx * _SpriteBody.Mass, vy * _SpriteBody.Mass));
			Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(vx, Body.LinearVelocity.Y);
			//Console.WriteLine("On Platform == {0}", OnPlatform);

			// Time tolerance for input controls!
			foreach(DictionaryEntry de in e.KeysPressedWhen)
			{
				if((Key)de.Key == Key.Space)
				{
					if(Helper.KeyPressedInTime((DateTime)de.Value, DateTime.Now))
						Jump ();
				}
			}
		}
		public void Jump()
		{
			if(OnPlatform)
			{
				OnPlatform = false;
				float jump_imp_y = 5.0f * Body.Mass;
				var jump_imp = new Microsoft.Xna.Framework.Vector2(0.0f, jump_imp_y);
				Body.ApplyLinearImpulse(jump_imp);
			}
		}
		#endregion
	}
}

