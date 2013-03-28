using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace positron
{
	public class SpriteObject: SpriteBase, IWorldObject
	{
		protected bool _Preserve;
		protected int _WorldIndex;
		protected Body _SpriteBody;
		protected Fixture _SpriteFixture;
		public int WorldIndex { get { return _WorldIndex; } }
		public bool Preserve {
			get { return _Preserve; }
			set { _Preserve = value; }
		}

		public Body Body { get { return _SpriteBody; } }
		public Fixture Fixture { get { return _SpriteFixture; } }

		public override Vector3d Position {
			get {
				return new Vector3d(
					_SpriteBody.Position.X * Configuration.MeterInPixels,
					_SpriteBody.Position.Y * Configuration.MeterInPixels,
					_Position.Z);
			}
			set {
				_SpriteBody.Position =
					new Microsoft.Xna.Framework.Vector2(
						(float)(value.X / Configuration.MeterInPixels),
						(float)(value.Y / Configuration.MeterInPixels));
			}
		}
		public override double PositionX {
			get { return (double)Body.Position.X; }
			set { Body.Position = new Microsoft.Xna.Framework.Vector2((float)value, Body.Position.Y); }
		}
		public override double PositionY {
			get { return (double)Body.Position.X; }
			set { Body.Position = new Microsoft.Xna.Framework.Vector2(Body.Position.X, (float)value); }
		}
		public override Vector3d Velocity {
			get {
				return new Vector3d(
					_SpriteBody.LinearVelocity.X * Configuration.MeterInPixels,
					_SpriteBody.LinearVelocity.Y * Configuration.MeterInPixels,
					_Velocity.Z);
			}
			set {
				_SpriteBody.LinearVelocity =
					new Microsoft.Xna.Framework.Vector2(
						(float)(value.X / Configuration.MeterInPixels),
						(float)(value.Y / Configuration.MeterInPixels));
			}
		}
		public override double VelocityX {
			get { return (double)Body.LinearVelocity.X; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2((float)value, Body.LinearVelocity.Y); }
		}
		public override double VelocityY {
			get { return (double)Body.LinearVelocity.X; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2((float)value, Body.LinearVelocity.Y); }
		}
		public override double Theta {
			get { return Body.Rotation; }
			set { Body.Rotation = (float)value; }
		}

		#region Behavior
		public SpriteObject(RenderSet render_set):
			this(render_set, 0.0, 0.0, 1.0, 1.0, Texture.DefaultTexture)
		{
		}
		public SpriteObject(RenderSet render_set, Texture texture, params Texture[] textures):
			this(render_set, 0.0, 0.0, 1.0, 1.0, texture, textures)
		{
		}
		public SpriteObject (RenderSet render_set, double x, double y, Texture texture, params Texture[] textures):
			this(render_set, x, y, 1.0, 1.0, texture, textures)
		{		
		}
		// Main constructor:
		public SpriteObject (RenderSet render_set, double x, double y, double scalex, double scaley, Texture texture, params Texture[] textures):
			base(render_set, x, y, scalex, scaley, texture, textures)
		{
			_RenderSet = render_set;
			InitPhysics();
			Position = _Position; // Update body position from initial position
		}
		protected virtual void InitPhysics()
		{
			float w = (float)(_Size.X * Texture.Width / Configuration.MeterInPixels);
			float h = (float)(_Size.Y * Texture.Height / Configuration.MeterInPixels);
			var half_w_h = new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f);
			var msv2 = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, msv2);
			_SpriteFixture = FixtureFactory.AttachRectangle(w, h, 100.0f, half_w_h, _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.5f;
			_SpriteBody.OnCollision += HandleOnCollision;
		}

		bool HandleOnCollision (Fixture fixtureA, Fixture fixtureB, Contact contact)
		{

			return true;
		}
		public override void Render (double time)
		{
			//double w = _Size.X * Texture.Width;
			//double h = _Size.Y * Texture.Height;
			Texture.Bind(Texture);
			GL.Color4 (_Color);
			GL.PushMatrix();
			{
				GL.Translate (
					//Math.Round
					(_SpriteBody.Position.X * Configuration.MeterInPixels),
				    //Math.Round
					(_SpriteBody.Position.Y * Configuration.MeterInPixels), 0.0);
				// Don't even read this line:
				float r = (float)
					//(45.0 * Math.Round(
					(_Theta + (double)OpenTK.MathHelper.RadiansToDegrees(_SpriteBody.Rotation))
					//	/ 45.0))
						;
				GL.Rotate(r, 0.0, 0.0, 1.0);
				//GL.Translate(Math.Floor (Position.X), Math.Floor (Position.Y), Math.Floor (Position.Z));
				DrawQuad();
			}
			GL.PopMatrix();
		}
		public virtual void Update (double time)
		{
			base.Update(time);
		}
		public void SetChange (object sender, SetChangeEventArgs e)
		{
			this._RenderSet = e.To;
		}
		#endregion
	}
}

