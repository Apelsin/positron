using System;

using OpenTK;

using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
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

        protected float PassThruMargin = 2.0f / (float)Configuration.MeterInPixels;

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
            Body.OnCollision += HandleOnCollision;
		}
        protected bool HandleOnCollision (Fixture fixture_a, Fixture fixture_b, Contact contact)
        {
//            if (fixture_b.Body.UserData is Player) {
//                if(fixture_b == ((Player)fixture_b.Body.UserData).FixtureLower)
//                    return true;
//            }
            if(contact == null)
                return true;

            AABB platform_aabb, collider_body_aabb;
            //Console.WriteLine("fixture_a belongs to {0}", fixture_a.Body.UserData);
            //Console.WriteLine("fixture_b belongs to {0}", fixture_b.Body.UserData);

            // Get the enclosing AABBs because overlaps in fixtures can cause issues here
            // Collision will be called each time fixtures begin to overlap but this function
            // regards bodily interactions (not per-fixture)
            lock (fixture_a.Body) // assumes fixture_a.Body == this.Body
                fixture_a.Body.GetEnclosingAABB (out platform_aabb);
            lock (fixture_b.Body)
                fixture_b.Body.GetEnclosingAABB (out collider_body_aabb);

            // Because the platform is static, its AABB is absolute
            float platform_y_upper = platform_aabb.UpperBound.Y;

//            new BlueprintLine (
//                new Vector3d (
//                (platform_aabb.LowerBound.X) * Configuration.MeterInPixels - 60,
//                platform_y_upper * Configuration.MeterInPixels, 0.0),
//                new Vector3d (
//                (platform_aabb.UpperBound.X) * Configuration.MeterInPixels - 20,
//                platform_y_upper * Configuration.MeterInPixels, 0.0), this.RenderSet.Scene.WorldBlueprint, 1000);

            // Because the collider is dynamic (and not static), its AABB is relative to its body's center (I THINK)
            float collider_y_lower = fixture_b.Body.Position.Y + collider_body_aabb.LowerBound.Y;

//            new BlueprintLine (
//                new Vector3d (
//                (fixture_b.Body.Position.X - collider_body_aabb.Extents.X) * Configuration.MeterInPixels + 60,
//                collider_y_lower * Configuration.MeterInPixels, 0.0),
//                new Vector3d (
//                (fixture_b.Body.Position.X + collider_body_aabb.Extents.X) * Configuration.MeterInPixels - 20,
//                collider_y_lower * Configuration.MeterInPixels, 0.0), this.RenderSet.Scene.WorldBlueprint, 2000);

            bool interact = collider_y_lower + PassThruMargin > platform_y_upper;
            Console.WriteLine (interact ? "Collide" : "Pass-thru");
            contact.Enabled = interact;
            return interact;
        
            //return (fixture_b.Body.LinearVelocity.Y - Body.LinearVelocity.Y) < -0.1;
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
//			var half_w_h = new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f);
//			var msv2 = new Microsoft.Xna.Framework.Vector2(
//				(float)(_Position.X / Configuration.MeterInPixels),
//				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, Microsoft.Xna.Framework.Vector2.Zero, this);
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

