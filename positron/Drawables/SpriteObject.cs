using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace positron
{
	public class SpriteObject: SpriteBase, IWorldObject
	{
		protected Scene _Scene;
		protected bool _Preserve;
		protected int _WorldIndex;
		protected Body _SpriteBody;
		protected Fixture _SpriteFixture;

		public Scene Scene { get { return _Scene; } }
		public int WorldIndex { get { return _WorldIndex; } }
		public bool Preserve {
			get { return _Preserve; }
			set { _Preserve = value; }
		}

		public Body Body { get { return _SpriteBody; } }
		public Fixture Fixture { get { return _SpriteFixture; } }

		#region Behavior
		public SpriteObject(Scene scene):
			this(0.0, 0.0, 1.0, 1.0, Texture.DefaultTexture, scene)
		{
		}
		public SpriteObject(Texture texture, Scene scene):
			this(0.0, 0.0, 1.0, 1.0, texture, scene)
		{
		}
		public SpriteObject (double x, double y, Scene scene):
			this(x, y, 1.0, 1.0, Texture.DefaultTexture, scene)
		{		
		}
		public SpriteObject (double x, double y, Texture texture, Scene scene):
			this(x, y, 1.0, 1.0, texture, scene)
		{		
		}
		// Main constructor:
		public SpriteObject (double x, double y, double scalex, double scaley, Texture texture, Scene scene):
			base(x, y, scalex, scaley, texture)
		{
			_Scene = scene;
			InitPhysics();
		}
		protected virtual void InitPhysics()
		{
			float w = (float)(_Size.X * _Texture.Width / Configuration.MeterInPixels);
			float h = (float)(_Size.Y * _Texture.Height / Configuration.MeterInPixels);
			var half_w_h = new Microsoft.Xna.Framework.Vector2(w * 0.5f, h * 0.5f);
			var msv2 = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			_SpriteBody = BodyFactory.CreateBody(_Scene.World, msv2);
			_SpriteFixture = FixtureFactory.AttachRectangle(w, h, 100.0f, half_w_h, _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.0f;
		}
		public override void Render (double time)
		{
			double w = _Size.X * _Texture.Width;
			double h = _Size.Y * _Texture.Height;
			Texture.Bind(_Texture);
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
				GL.Begin (BeginMode.Quads);
				{
					GL.TexCoord2(0.0, 0.0);
					GL.Vertex2(0.0, 0.0);
					GL.TexCoord2(_TileX, 0.0);
					GL.Vertex2(w, 0.0);
					GL.TexCoord2(_TileX, -_TileY);
					GL.Vertex2(w, h);
					GL.TexCoord2(0.0, -_TileY);
					GL.Vertex2(0.0, h);
				}
				GL.End ();
			}
			GL.PopMatrix();
		}
		public virtual void Update (double time)
		{
			// Put stuff here!
		}
		public void SceneChange (object sender, SceneChangeEventArgs e)
		{
			this._Scene = e.To;
		}
		#endregion
	}
}

