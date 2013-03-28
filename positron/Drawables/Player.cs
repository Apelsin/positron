using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

using OpenTK.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace positron
{
	public class Player : SpriteObject, IInputAccepter
	{
		protected int MoveXIdx = 0;
		protected int MoveYIdx = 0;
		protected float _MovementDamping = 3.0f;
		protected Stopwatch JumpTimer = new Stopwatch();
		protected ManualResetEvent JumpRayMRE = new ManualResetEvent(false);
		protected float[] JumpRayXDirections = new float[] { 0.0f, 0.1f, -0.1f, 0.2f, -0.2f };
		protected bool GoneDown = true;
		public float MovementDamping {
			get { return _MovementDamping; }
			set { _MovementDamping = value; }
		}
		#region Behavior
		public Player(RenderSet render_set, Texture texture, params Texture[] textures):
			this(render_set, 0.0, 0.0, 1.0, 1.0, texture, textures)
		{
		}
		public Player (RenderSet render_set, double x, double y, Texture texture, params Texture[] textures):
			this(render_set, x, y, 1.0, 1.0, texture, textures)
		{		
		}
		// Main constructor:
		public Player (RenderSet render_set, double x, double y, double scalex, double scaley, Texture texture, params Texture[] textures):
			base(render_set, x, y, scalex, scaley, texture, textures)
		{
		}
		protected override void InitPhysics ()
		{
			float pixel = (float)(1.0 / Configuration.MeterInPixels);
			float w = (float)(_Size.X * Texture.Width / Configuration.MeterInPixels);
			float h = (float)(_Size.Y * Texture.Height / Configuration.MeterInPixels);
			var body_offset = new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f );
			var msv2 = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, msv2);
			// Attach the main part of the body
			FixtureFactory.AttachRectangle(w, h, 100.0f, body_offset, _SpriteBody);
			_SpriteBody.BodyType = BodyType.Dynamic;
			_SpriteBody.FixedRotation = true;
			//_SpriteFixture = FarseerPhysics.Factories.FixtureFactory.AttachEdge
			_SpriteBody.Mass = 100.0f;
			Body.OnCollision += HandleOnCollision;
			Body.OnSeparation += HandleOnSeparation;
			JumpTimer.Start();
		}
		protected bool HandleOnCollision (Fixture fixture_a, Fixture fixture_b, Contact contact)
		{
			return true;
		}
		protected void HandleOnSeparation (Fixture fixture_a, Fixture fixture_b)
		{
		}
		public bool KeyDown (object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Space) {
				Jump();
			}
			return true;
		}
		public bool KeyUp(object sender, KeyboardKeyEventArgs e)
		{
			return true;
		}
		public KeysUpdateEventArgs KeysUpdate(object sender, KeysUpdateEventArgs e)
		{
			float fx = (e.KeysPressedWhen.Contains(Key.A) ? -1.0f : 0.0f) + (e.KeysPressedWhen.Contains(Key.D) ? 1.0f : 0.0f);
			//float fy = (e.KeysPressed.Contains(Key.S) ? -1.0f : 0.0f) + (e.KeysPressed.Contains(Key.W) ? 1.0f : 0.0f);
			fx *= 5f;
			//vy *= 10f;
			//this.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(vx * _SpriteBody.Mass, vy * _SpriteBody.Mass));
			var v = new Microsoft.Xna.Framework.Vector2(fx * 0.5f, Body.LinearVelocity.Y);
			Body.LinearVelocity = Microsoft.Xna.Framework.Vector2.Lerp(Body.LinearVelocity, v, _MovementDamping * (float)e.Time);
			Body.ApplyForce(v * Body.Mass);
			//Console.WriteLine("On Platform == {0}", OnPlatform);

			// Time tolerance for input controls!
			//OrderedDictionary keytime = new OrderedDictionary();
			foreach(DictionaryEntry de in e.KeysPressedWhen)
			{
				if((Key)de.Key == Key.Space)
				{
					if(Helper.KeyPressedInTime((DateTime)de.Value, DateTime.Now))
					{
						Jump ();
					}
//					else
//					{
//						keytime.Add(de.Key, de.Value);
//					}
				}
			}
			//e.KeysPressedWhen = keytime;
			return e;
		}
		public void BodyPlatformRayCast (RayCastCallback callback)
		{
			// TODO: make the foot reach distance (in pixels) a member variable
			float x_start = (float)(_Size.X * (0.5 * Texture.Width) / Configuration.MeterInPixels);
			float y_start = (float)(_Size.Y * (0.1 * Texture.Height) / Configuration.MeterInPixels);
			float y_end = (float)(-3.0 / Configuration.MeterInPixels);

			for (int i = 0; i < JumpRayXDirections.Length; i++) {
				float dx = (float)(_Size.X * Texture.Width * JumpRayXDirections [i] / Configuration.MeterInPixels);
				var start = new Microsoft.Xna.Framework.Vector2 (x_start + dx, y_start);
				var end = new Microsoft.Xna.Framework.Vector2 (start.X + dx, y_end);
				_RenderSet.Scene.RayCast (callback, Body.Position + start, Body.Position + end);
			}
		}
		public void Jump()
		{
			/// <summary>
			/// Called for each fixture found in the query. You control how the ray cast
			/// proceeds by returning a float:
			/// <returns>-1 to filter, 0 to terminate, fraction to clip the ray for closest hit, 1 to continue</returns>
			/// </summary>

			RayCastCallback callback = (fixture, point, normal, fraction) => {
				// TODO: Have these values be not hard-coded
				if(JumpTimer.Elapsed.TotalMilliseconds > 50 && GoneDown)
				{
					JumpTimer.Restart();
					float jump_imp_y = 3.0f;// * Body.Mass;
					//var jump_imp = new Microsoft.Xna.Framework.Vector2(0.0f, jump_imp_y);
					Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, jump_imp_y);
					GoneDown = false;
				}
				return 0;
			};
			BodyPlatformRayCast(callback);
		}
		public override void Update(double time)
		{
			GoneDown = GoneDown || Body.LinearVelocity.Y < 0.0f;
			((BooleanIndicator)Program.Game.TestIndicators[0]).State = GoneDown;
		}
		#endregion
	}
}

