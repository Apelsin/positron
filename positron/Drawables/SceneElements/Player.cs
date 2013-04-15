using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;

using OpenTK;
using OpenTK.Input;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace positron
{
	#region Event Args
	public class HealthChangedEventArgs : EventArgs
	{
		public Player Player { get; set; }
		public int HealthWas { get; set; }
		public int HealthNow { get; set; }
		public HealthChangedEventArgs(Player player, int health_was, int health_now)
		{
			Player = player;
			HealthWas = health_was;
			HealthNow = health_now;
		}
	}
	#endregion
	#region Event Handlers
	public delegate void HealthChangedEventHandler(object sender, HealthChangedEventArgs e);
	#endregion
	public class Player : SpriteObject, IInputAccepter
	{
		#region Events
		public event HealthChangedEventHandler HealthChanged;
		#endregion

		protected int _HealthMax = 4;
		protected int _Health;
		protected int MoveXIdx = 0;
		protected int MoveYIdx = 0;
		protected float _MovementDamping = 3.0f;
		protected Vector3d _DampVeloNormal = new Vector3d();
		protected Stopwatch JumpTimer = new Stopwatch();
		protected Stopwatch WalkAnimationTimer = new Stopwatch();

		protected SpriteAnimation AnimationStationary;
		protected SpriteAnimation AnimationWalk;
		protected SpriteAnimation AnimationStationaryFw;
		protected SpriteAnimation AnimationWalkFw;
		protected SpriteAnimation AnimationStationaryBk;
		protected SpriteAnimation AnimationWalkBk;

		protected SpriteAnimation AnimationPreJump;
		protected SpriteAnimation AnimationJumping;
		protected SpriteAnimation AnimationEndJump;

		protected ManualResetEvent JumpRayMRE = new ManualResetEvent(false);
		protected float[] JumpRayXDirections = new float[] { 0.0f, 0.2f, -0.2f, 0.45f, -0.45f };
		protected bool GoneDown = true;
		public float MovementDamping {
			get { return _MovementDamping; }
			set { _MovementDamping = value; }
		}
		public Vector3d DampVeloNormal { get { return _DampVeloNormal; } }
		public int HealthMax {
			get { return _HealthMax; }
		}
		public int Health {
			get { return _Health; }
		}
		#region Behavior
		public Player(RenderSet render_set, Texture texture):
			this(render_set, 0.0, 0.0, 1.0, 1.0, texture)
		{
		}
		public Player (RenderSet render_set, double x, double y, Texture texture):
			this(render_set, x, y, 1.0, 1.0, texture)
		{
		}
		// Main constructor:
		public Player (RenderSet render_set, double x, double y, double scalex, double scaley, Texture texture):
			base(render_set, x, y, scalex, scaley, texture)
		{
			HealthChanged += (object sender, HealthChangedEventArgs e) => { _Health = Math.Max (0, e.HealthNow); };
			OnHealthChanged(this, _HealthMax);

			AnimationStationary = 	new SpriteAnimation(texture, 0);
			AnimationWalk  = 		new SpriteAnimation(texture, true, 1, 2, 3, 4);
			AnimationStationaryFw = new SpriteAnimation(texture, 5);
			AnimationWalkFw = 		new SpriteAnimation(texture, true, 6, 7, 8, 9);
			AnimationStationaryBk = new SpriteAnimation(texture, 10);
			AnimationWalkBk = 		new SpriteAnimation(texture, true, 11, 12, 13, 14);
			
			AnimationPreJump = 		new SpriteAnimation(texture, 15);
			AnimationJumping = 		new SpriteAnimation(texture, 17);
			AnimationEndJump = 		new SpriteAnimation(texture, 19);

			AnimationPreJump.Frames[0].FrameTime = 100;
			//AnimationPreJump.Frames[1].FrameTime = 50;

			AnimationEndJump.Frames[0].FrameTime = 200;

			_FrameTimer.Start ();
		}
		public void OnHealthChanged (object sender, int health)
		{
			HealthChanged(sender, new HealthChangedEventArgs(this, _Health, Math.Max (0, health)));
		}
		protected override void InitPhysics ()
		{

			float pixel = (float)(1.0 / Configuration.MeterInPixels);

			float corner_clip_x = 7.0f * pixel;
			float corner_clip_y = 2.0f * pixel;


			float w, h;
			if (Texture.Regions != null && Texture.Regions.Length > 0)
			{
				var size = Texture.Regions[0].Size;
				w = (float)(_Scale.X * size.X / Configuration.MeterInPixels);
				h = (float)(_Scale.Y * size.Y / Configuration.MeterInPixels);
			}
			else
			{
				w = (float)(_Scale.X * Texture.Width / Configuration.MeterInPixels);
				h = (float)(_Scale.Y * Texture.Height / Configuration.MeterInPixels);
			}
			var half_w_h = new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f);
			var msv2 = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, msv2);

			Microsoft.Xna.Framework.Vector2 a, b, c, d;

			// Attach the main part of the body
			var verts = new Vertices(new Microsoft.Xna.Framework.Vector2[] {
				new Microsoft.Xna.Framework.Vector2(corner_clip_x, 0.0f) - half_w_h,
				new Microsoft.Xna.Framework.Vector2(w - corner_clip_x, 0.0f) - half_w_h,
				b = new Microsoft.Xna.Framework.Vector2(w, corner_clip_y) - half_w_h,
				c = new Microsoft.Xna.Framework.Vector2(w, h - corner_clip_y) - half_w_h,
				new Microsoft.Xna.Framework.Vector2(w - corner_clip_x, h) - half_w_h,
				new Microsoft.Xna.Framework.Vector2(corner_clip_x, h) - half_w_h,
				d = new Microsoft.Xna.Framework.Vector2(0.0f, h - corner_clip_y) - half_w_h,
				a = new Microsoft.Xna.Framework.Vector2(0.0f, corner_clip_y) - half_w_h
			});

			FixtureFactory.AttachPolygon(verts, 100.0f, _SpriteBody);

			//FixtureFactory.AttachRectangle(w, h, 100.0f, new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f), _SpriteBody);

			_SpriteBody.BodyType = BodyType.Dynamic;
			_SpriteBody.FixedRotation = true;
			//_SpriteFixture = FarseerPhysics.Factories.FixtureFactory.AttachEdge
			_SpriteBody.Mass = 100.0f;
			Body.OnCollision += HandleOnCollision;
			Body.OnSeparation += HandleOnSeparation;
			Preserve = true;

			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = this.RenderSet.Scene == Program.MainGame.CurrentScene;

			ConnectBody();

			JumpTimer.Start();
		}
		protected bool HandleOnCollision (Fixture fixture_a, Fixture fixture_b, Contact contact)
		{
			RayCastCallback callback = (fixture, point, normal, fraction) => {
				// TODO: Have these values be not hard-coded
				if(JumpTimer.Elapsed.TotalMilliseconds > 50 && GoneDown)
				{
					if(_AnimationCurrent == AnimationPreJump ||
					   _AnimationCurrent == AnimationJumping)
					{
						PlayAnimation(AnimationEndJump);
						_AnimationNext = AnimationStationary;
					}
					//Console.WriteLine (VelocityY);
				}
				return 0;
			};
			BodyPlatformRayCast(callback);
			return true;
		}
		protected void HandleOnSeparation (Fixture fixture_a, Fixture fixture_b)
		{
		}
		public bool KeyDown (object sender, KeyboardKeyEventArgs e)
		{
			if (e.Key == Key.Space) {
				Jump ();
			} else if (e.Key == Key.E) {
				DoActionHere ();
			} else if (e.Key == Key.F) {
                UpdateEventHandler late;
                late = (u_sender, u_e) =>
                {
                    var bullet = new BasicBullet(this._RenderSet.Scene, this.PositionX + (SizeX) * TileX, this.PositionY, 500 * TileX, 0);
                };
                Program.MainGame.UpdateEventQueue.Enqueue(late);
			}
			return true;
		}
		protected void DoActionHere ()
		{
			var fixtures = _RenderSet.Scene.World.TestPointAll(Body.WorldCenter);
			foreach(Fixture fixture in fixtures)
			{
				if (fixture != null) {
					var world_object = fixture.Body.GetWorldObject ();
					if(world_object != null)
					{
						if(world_object is IInteractiveObject)
						{
							var interactive_object = (IInteractiveObject)world_object;
							interactive_object.OnAction(this, new ActionEventArgs());
							break;
						}
					}
				}
			}
		}
		public bool KeyUp(object sender, KeyboardKeyEventArgs e)
		{
			return true;
		}
		public KeysUpdateEventArgs KeysUpdate (object sender, KeysUpdateEventArgs e)
		{
			float fx = (e.KeysPressedWhen.Contains (Key.A) ? -1.0f : 0.0f) + (e.KeysPressedWhen.Contains (Key.D) ? 1.0f : 0.0f);
			//float fy = (e.KeysPressed.Contains(Key.S) ? -1.0f : 0.0f) + (e.KeysPressed.Contains(Key.W) ? 1.0f : 0.0f);
			fx *= 5f;
			//vy *= 10f;
			//this.Body.ApplyForce(new Microsoft.Xna.Framework.Vector2(vx * _SpriteBody.Mass, vy * _SpriteBody.Mass));
			var v = new Microsoft.Xna.Framework.Vector2 (fx * 0.5f, Body.LinearVelocity.Y);
			Body.LinearVelocity = Microsoft.Xna.Framework.Vector2.Lerp (Body.LinearVelocity, v, _MovementDamping * (float)e.Time);
			if (Body.LinearVelocity.LengthSquared () > 0.1f) {
				_DampVeloNormal = new Vector3d (Body.LinearVelocity.X, Body.LinearVelocity.Y, 0.0);
				_DampVeloNormal /= Math.Max (5.0, _DampVeloNormal.Length);
			}

			float v__x_mag = Math.Abs (v.X);

			if (_AnimationCurrent != AnimationJumping &&
				_AnimationCurrent != AnimationPreJump &&
				_AnimationCurrent != AnimationEndJump) {
				if (v__x_mag > 0.1) {
					_TileX = v.X > 0.0 ? 1.0 : -1.0;
					PlayAnimation (AnimationWalk);
				} else if (v__x_mag < 0.01)
				{
					PlayAnimation (AnimationStationary);
				}
			}

			//Console.WriteLine("On Platform == {0}", OnPlatform);

			// Time tolerance for input controls!
			//OrderedDictionary keytime = new OrderedDictionary();
			foreach(DictionaryEntry de in e.KeysPressedWhen)
			{
				Key k = (Key)de.Key;
				if(k == Key.Space)
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
			AABB main_aabb;
			lock(Body)
				Body.FixtureList[0].GetAABB(out main_aabb, 0);
			float w = main_aabb.UpperBound.X - main_aabb.LowerBound.X;
			float h = main_aabb.UpperBound.Y - main_aabb.LowerBound.Y;
			float x_start = 0.0f;
			float y_end = (float)(5.0 / Configuration.MeterInPixels);
			float y_start = y_end - (float)(_Scale.Y * (0.5 * h));
			y_end = y_start - 2 * y_end;
			for (int i = 0; i < JumpRayXDirections.Length; i++) {
				float dx = (float)(_Scale.X * w * JumpRayXDirections [i]);
				var start = new Microsoft.Xna.Framework.Vector2 (x_start + dx, y_start);
				var end = new Microsoft.Xna.Framework.Vector2 (start.X, y_end);
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
					float jump_imp_y = 3.0f;
					Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, jump_imp_y);
					GoneDown = false;
					PlayAnimation (AnimationPreJump);
					_AnimationNext = AnimationJumping;
				}
				return 0;
			};
			BodyPlatformRayCast(callback);
		}
		public override void Update(double time)
		{
			base.Update(time);
			GoneDown = GoneDown || Body.LinearVelocity.Y < 0.0f;
			//((BooleanIndicator)Program.MainGame.TestIndicators[0]).State = GoneDown;
		}
		#endregion
	}
}

