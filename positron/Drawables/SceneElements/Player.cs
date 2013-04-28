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

		protected Fixture FixtureUpper;
		protected Fixture FixtureLower;

		protected List<Fixture> FixtureLowerColliders = new List<Fixture>();

		protected int _HealthMax = 4;
		protected int _Health;
		protected int MoveXIdx = 0;
		protected int MoveYIdx = 0;
		protected float _MovementDamping = 3.0f;

		protected bool _WieldingGun = false;

		protected Vector3d _DampVeloNormal = new Vector3d();
		protected Stopwatch JumpTimer = new Stopwatch();
		protected Stopwatch WalkAnimationTimer = new Stopwatch();
		protected Stopwatch GunStowTimer = new Stopwatch();

		protected SpriteAnimation AnimationStationary;
		protected SpriteAnimation AnimationWalk;
		protected SpriteAnimation AnimationStationaryFw;
		protected SpriteAnimation AnimationWalkFw;
		protected SpriteAnimation AnimationStationaryBk;
		protected SpriteAnimation AnimationWalkBk;

		protected SpriteAnimation AnimationPreJump;
		protected SpriteAnimation AnimationJumping;
		protected SpriteAnimation AnimationEndJump;

		protected SpriteAnimation AnimationAimGunFwd;
		protected SpriteAnimation AnimationAimGunFwdUp;
		protected SpriteAnimation AnimationAimGunFwdDown;
		protected SpriteAnimation AnimationAimGunFwdCrouch;
		protected SpriteAnimation AnimationAimGunFwdJump;


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
		public bool WieldingGun {
			get { return _WieldingGun; }
			set { _WieldingGun = value; }
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

			AnimationAimGunFwd = 		new SpriteAnimation(texture, 20);
			AnimationAimGunFwdUp = 		new SpriteAnimation(texture, 21);
			AnimationAimGunFwdDown = 	new SpriteAnimation(texture, 22);
			AnimationAimGunFwdCrouch = 	new SpriteAnimation(texture, 23);
			AnimationAimGunFwdJump = 	new SpriteAnimation(texture, 24);

			AnimationPreJump.Frames[0].FrameTime = 100;
			//AnimationPreJump.Frames[1].FrameTime = 50;

			AnimationEndJump.Frames[0].FrameTime = 200;

			_FrameTimer.Start ();
			JumpTimer.Start();
		}
		public void OnHealthChanged (object sender, int health)
		{
			HealthChanged(sender, new HealthChangedEventArgs(this, _Health, Math.Max (0, health)));
		}
		protected override void InitPhysics ()
		{
			float pixel = (float)(1.0 / Configuration.MeterInPixels);

			float fixture_scale_y = 0.75f;

			float corner_clip_x = 7.0f * pixel;
			float corner_clip_y = 2.0f * pixel;
			float y_shift = 2.0f * pixel;


			float w, h, half_h, qtr_h, scl_h;
			if (Texture.Regions != null && Texture.Regions.Length > 0)
			{
				var size = Texture.Regions[0].Size;
				w = (float)(_Scale.X * size.X) * pixel;
				h = (float)(_Scale.Y * size.Y) * pixel;
			}
			else
			{
				w = (float)(_Scale.X * Texture.Width) * pixel;
				h = (float)(_Scale.Y * Texture.Height) * pixel;
			}
			scl_h = fixture_scale_y * h;
			half_h = 0.5f * h;
			var half_w_h = new Microsoft.Xna.Framework.Vector2(w * 0.5f, half_h);
			var msv2 = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X) * pixel,
				(float)(_Position.Y) * pixel);
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, Microsoft.Xna.Framework.Vector2.Zero, this);

			Microsoft.Xna.Framework.Vector2 a, b, c, d, e00, e01, e10, e11;

			var verts = new Vertices(new Microsoft.Xna.Framework.Vector2[] {
				e00 = new Microsoft.Xna.Framework.Vector2(corner_clip_x, 0.0f),
				e10 = new Microsoft.Xna.Framework.Vector2(w - corner_clip_x, 0.0f),
				b = new Microsoft.Xna.Framework.Vector2(w, corner_clip_y),
				c = new Microsoft.Xna.Framework.Vector2(w, scl_h - corner_clip_y),
				e11 = new Microsoft.Xna.Framework.Vector2(w - corner_clip_x, scl_h),
				e01 = new Microsoft.Xna.Framework.Vector2(corner_clip_x, scl_h),
				d = new Microsoft.Xna.Framework.Vector2(0.0f, scl_h - corner_clip_y),
				a = new Microsoft.Xna.Framework.Vector2(0.0f, corner_clip_y),
			});

			var verts_upper = new Vertices();
			verts.ForEach(v => {
				v.X -= half_w_h.X;
				v.Y = y_shift + v.Y * fixture_scale_y + half_h * (0.5f - fixture_scale_y);
				verts_upper.Add(v);
				//Console.Write("{0}, ", v.ToString());
			});
			var verts_lower = new Vertices();
			verts.ForEach(v => {
				v.X -= half_w_h.X;
				v.Y = y_shift + v.Y * fixture_scale_y - half_h;
				verts_lower.Add(v);
				//Console.Write("{0}, ", v.ToString());
			});

			FixtureUpper = FixtureFactory.AttachPolygon(verts_upper, 100.0f, _SpriteBody);
			FixtureLower = FixtureFactory.AttachPolygon(verts_lower, 100.0f, _SpriteBody);

			_SpriteBody.BodyType = BodyType.Dynamic;
			_SpriteBody.FixedRotation = true;
			//_SpriteFixture = FarseerPhysics.Factories.FixtureFactory.AttachEdge
			_SpriteBody.Mass = 100.0f;
			Body.OnCollision += HandleOnCollision;
			Body.OnSeparation += HandleOnSeparation;
			Preserve = true;

			Body.SleepingAllowed = false; // Avoid dumb shit

			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = this.RenderSet.Scene == Program.MainGame.CurrentScene;

			InitBlueprints();

//			AABB lol;
//			FixtureLower.GetAABB(out lol, 0);
//			Console.WriteLine("AT FIRST I WAS ALL LIKE: {0}", lol.LowerBound);
//			Position = _Position;
//			FixtureLower.GetAABB(out lol, 0);
//			Console.WriteLine("BUT THEN I: {0}", lol.LowerBound);
		}
		protected bool HandleOnCollision (Fixture fixture_a, Fixture fixture_b, Contact contact)
		{
			if(fixture_a == FixtureLower)
				FixtureLowerColliders.Add(fixture_b);
			RayCastCallback callback = (fixture, point, normal, fraction) => {
				// TODO: Have these values be not hard-coded
				lock(Program.MainUpdateLock)
				{
					if(fixture == FixtureUpper || fixture == FixtureLower)
						return 1.0f;
					if(JumpTimer.Elapsed.TotalMilliseconds > 50 && GoneDown)
					{
						if(_AnimationCurrent == AnimationPreJump ||
						   _AnimationCurrent == AnimationJumping)
						{
							PlayAnimation(AnimationEndJump);
							_AnimationNext = AnimationStationary;

							// TODO: See the other comment regarding disabling the
							// lower body fixture; this is for collision rather than
							// separation

							// TODO: Hack apart Farseer Physics to make it suck less
							//var manifold = new Manifold();
							//manifold.LocalPoint = point;
							//manifold.LocalNormal = normal;

							AABB fixture_upper_aabb, fixture_lower_aabb;

							FixtureLower.CollisionCategories = Category.All;

							FixtureUpper.GetAABB(out fixture_upper_aabb, 0);
							FixtureLower.GetAABB(out fixture_lower_aabb, 0);

							if(FixtureLower.OnCollision != null)
								FixtureLower.OnCollision(FixtureLower, fixture, null);
							if(fixture.OnCollision != null)
								fixture.OnCollision(fixture, FixtureLower, null);
							
							PositionWorldY += fixture_upper_aabb.LowerBound.Y - fixture_lower_aabb.LowerBound.Y;
						}
					}
				}
				return 0;
			};
			BodyPlatformRayCast(callback);
			return true;
		}
		protected void HandleOnSeparation (Fixture fixture_a, Fixture fixture_b)
		{
			if(fixture_a == FixtureLower)
				FixtureLowerColliders.Remove(fixture_b);
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
					// TODO: Un-hard-code this:
					Vector2d shot_offset = new Vector2d((SizeX + 3) * TileX, SizeY * 0.23);
					var bullet = new BasicBullet(this._RenderSet.Scene,
					                             this.PositionX + shot_offset.X,
					                             this.PositionY + shot_offset.Y,
					                             1000 * TileX, 0);
					WieldingGun = true;
					GunStowTimer.Restart();
					return true;
                };
                Program.MainGame.AddUpdateEventHandler(this, late);
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
						if(world_object is IActuator)
						{
							var interactive_object = (IActuator)world_object;
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
					if(WieldingGun)
						PlayAnimation(AnimationAimGunFwd);
					else
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
			AABB aabb;
			lock (Body) {
				if(FixtureLower.CollisionCategories == Category.None && _AnimationCurrent == AnimationJumping)
					FixtureUpper.GetAABB (out aabb, 0);
				else
					FixtureLower.GetAABB (out aabb, 0); // Probably disabled lower fixture during jump
			}
			float w = aabb.UpperBound.X - aabb.LowerBound.X;
			float h = aabb.UpperBound.Y - aabb.LowerBound.Y;

			float pixel = 1.0f / (float)Configuration.MeterInPixels;

			float px = (float)PositionX * pixel;
			float py = (float)PositionY * pixel;

			float x_start = 0.0f;
			float spread = 3.0f * pixel;
			float y_start = aabb.LowerBound.Y + spread;
			float y_end = aabb.LowerBound.Y - spread;

			for (int i = 0; i < JumpRayXDirections.Length; i++) {
				float dx = (float)(_Scale.X * w * JumpRayXDirections [i]);
				var start = new Microsoft.Xna.Framework.Vector2 (px + x_start + dx, py + y_start);
				var end = new Microsoft.Xna.Framework.Vector2 (start.X, py + y_end);
				_RenderSet.Scene.RayCast (callback, start, end);
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
				lock(Program.MainUpdateLock)
				{
					// TODO: Have these values be not hard-coded
					if(fixture == FixtureUpper || fixture == FixtureLower)
						return 1.0f;

					if(JumpTimer.Elapsed.TotalMilliseconds > 50 && GoneDown)
					{
	                    JumpTimer.Restart();
						float jump_imp_y = 3.0f;
						Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, jump_imp_y);
						GoneDown = false;
						PlayAnimation (AnimationPreJump);
						_AnimationNext = AnimationJumping;

						FixtureLower.CollisionCategories = Category.None;
					}

					if(FixtureLower.CollisionCategories == Category.None)
					{
						// TODO: Give this functionality to more than just the player class
						// This code is VERY important; this code tells all of the objects
						// currently touching the lower fixture to separate as the fixture
						// is marked as non-colliding. This ensures things like floor switches
						// don't get stuck closed!
						FixtureLowerColliders.Add(fixture);
						FixtureLowerColliders.ForEach(c => {
							if(FixtureLower.OnSeparation != null)
								FixtureLower.OnSeparation(FixtureLower, c);
							if(c.OnSeparation != null)
								c.OnSeparation(c, FixtureLower);
							//Console.WriteLine (c.Body.UserData);
						});
						FixtureLowerColliders.Clear();
					}

					return 1.0f;
				}
			};
			BodyPlatformRayCast(callback);
		}
		public override void Update (double time)
		{
			if (GunStowTimer.Elapsed.TotalSeconds > 0.2) {
				GunStowTimer.Reset();
				WieldingGun = false;
			}
			base.Update(time);
			GoneDown = GoneDown || Body.LinearVelocity.Y < 0.0f;
			//((BooleanIndicator)Program.MainGame.TestIndicators[0]).State = GoneDown;
		}
		#endregion
	}
}

