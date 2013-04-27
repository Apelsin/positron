using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace positron
{
	public class SpriteObject: SpriteBase, IWorldObject
	{
		protected int _WorldIndex;
		protected Body _SpriteBody;
		public int WorldIndex { get { return _WorldIndex; } }
		public Body Body {
			get { return _SpriteBody; }
			set { _SpriteBody = value; }
		}
		/// <summary>
		/// Gets or sets the position world.
		/// </summary>
		/// <description>
		/// NOTE: UPDATING THE BODY POSITION WHILE THE BODY IS
		/// DISABLED WILL ALTER ITS FIXTURES AND AABBS!!!
		///</description>
		public Vector3d PositionWorld {
			get {
				// HACK: tl;dr redo this
				if(Body != null && Body.Enabled)
					return new Vector3d(Body.Position.X, Body.Position.Y, _Position.Z);
				else
					return _Position;
			}
			set {
				// HACK: same as above
				_Position = value;
				if(Body != null && Body.Enabled)
				{
					Body.Position =
						new Microsoft.Xna.Framework.Vector2(
							(float)value.X,
							(float)value.Y);
				}
				else
				{
//					if(this is Door && this.RenderSet.Scene.GetType() == typeof(SceneThree))
//					{
//						Console.WriteLine ("{{ Move door to {0} }}, idx becomes {1}", value, Program.MainGame.UpdateEventList.Count);
//					}
					Program.MainGame.AddUpdateEventHandler(this, (sender, e) => {
						if(Body.Enabled)
						{
							Body.Position =
								new Microsoft.Xna.Framework.Vector2(
									(float)value.X,
									(float)value.Y);
							return true;
						}
						return false;
					});
				}
			}
		}
		public double PositionWorldX {
			get { return PositionWorld.X; }
			set { PositionWorld = new Vector3d(value, PositionWorldY, PositionWorld.Z); }
		}
		public double PositionWorldY {
			get { return PositionWorld.Y; }
			set { PositionWorld = new Vector3d(PositionWorldX, value, PositionWorld.Z); }
		}
		public override Vector3d Position {
			get { return PositionWorld * Configuration.MeterInPixels; }
			set { PositionWorld = value / Configuration.MeterInPixels; }
		}
		public override double PositionX {
			get { return PositionWorldX * Configuration.MeterInPixels; }
			set { PositionWorldX = value / Configuration.MeterInPixels; }
		}
		public override double PositionY {
			get { return PositionWorldY * Configuration.MeterInPixels; }
			set { PositionWorldY = value / Configuration.MeterInPixels; }
		}
		public Vector3d Corner {
			get { return Position - new Vector3d(0.5 * SizeX, 0.5 * SizeY, Position.Z); }
			set { Position = value + new Vector3d(0.5 * SizeX, 0.5 * SizeY, Position.Z); }
		}
		public double CornerX {
			get { return PositionX - 0.5 * SizeX; }
			set { PositionX = value + 0.5 * SizeX; }
		}
		public double CornerY {
			get { return PositionY - 0.5 * SizeY; }
			set { PositionY = value + 0.5 * SizeY; }
		}
		public Vector3d VelocityWorld {
			get {
				return new Vector3d(
					_SpriteBody.LinearVelocity.X,
					_SpriteBody.LinearVelocity.Y,
					_Velocity.Z);
			}
			set {
				_SpriteBody.LinearVelocity =
					new Microsoft.Xna.Framework.Vector2(
						(float)value.X,
						(float)value.Y);
			}
		}
		public double VelocityWorldX {
			get { return (double)Body.LinearVelocity.X; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2((float)value, Body.LinearVelocity.Y); }
		}
		public double VelocityWorldY {
			get { return (double)Body.LinearVelocity.Y; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, (float)value); }
		}
		public override Vector3d Velocity {
			get { return VelocityWorld * Configuration.MeterInPixels; }
			set { VelocityWorld = value / Configuration.MeterInPixels; }
		}
		public override double VelocityX {
			get { return (double)Body.LinearVelocity.X *  Configuration.MeterInPixels; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2((float)(value / Configuration.MeterInPixels), Body.LinearVelocity.Y); }
		}
		public override double VelocityY {
			get { return (double)Body.LinearVelocity.Y *  Configuration.MeterInPixels; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, (float)(value / Configuration.MeterInPixels)); }
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
		public SpriteObject(RenderSet render_set, Texture texture):
			this(render_set, 0.0, 0.0, 1.0, 1.0, texture)
		{
		}
		public SpriteObject (RenderSet render_set, double x, double y, Texture texture):
			this(render_set, x, y, 1.0, 1.0, texture)
		{		
		}
		// Main constructor:
		public SpriteObject (RenderSet render_set, double x, double y, double scalex, double scaley, Texture texture):
			base(render_set, x, y, scalex, scaley, texture)
		{
			Position = new Vector3d(_Position.X + 0.5 * SizeX, _Position.Y + 0.5 * SizeY, 0.0);
			_RenderSet = render_set;
			InitPhysics();

			RenderSetEntry += EnteredRenderSet; // Virtual default event handler
		}
		protected virtual void InitPhysics()
		{
            float w, h;
            if (Texture.Regions != null && Texture.Regions.Length > 0)
            {
                var size = Texture.DefaultRegion.Size;
                w = (float)(_Scale.X * size.X / Configuration.MeterInPixels);
                h = (float)(_Scale.Y * size.Y / Configuration.MeterInPixels);
            }
            else
            {
                w = (float)(_Scale.X * Texture.Width / Configuration.MeterInPixels);
                h = (float)(_Scale.Y * Texture.Height / Configuration.MeterInPixels);
            }
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, Microsoft.Xna.Framework.Vector2.Zero, this);
			FixtureFactory.AttachRectangle(w, h, 100.0f, Microsoft.Xna.Framework.Vector2.Zero, _SpriteBody);
			_SpriteBody.BodyType = BodyType.Static;
			_SpriteBody.FixedRotation = true;
			_SpriteBody.Friction = 0.5f;

			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = this.RenderSet.Scene == Program.MainGame.CurrentScene;

			InitBlueprints();
		}
		protected virtual void EnteredRenderSet (object sender, RenderSetChangeEventArgs e)
		{
			Body.Enabled = (RenderSet.Scene == e.To.Scene);
		}
		public void Debug(string context)
		{
			AABB aabb;
			_SpriteBody.FixtureList[0].GetAABB(out aabb, 0);
			Console.WriteLine("FixtureList[0]: ({0})", context);
			Console.WriteLine( "      AABB: {0}", aabb.UpperBound);
			Console.WriteLine ("ProxyCount: {0}", _SpriteBody.FixtureList[0].ProxyCount);
			Console.WriteLine( "Body.Enabl: {0}", _SpriteBody.Enabled);
		}
		public override void Render (double time)
		{
            GL.PushMatrix();
            {
                GL.Translate(
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
                GL.Scale(_Scale);
                Draw();
            }
            GL.PopMatrix();
		}
		public override void Update (double time)
		{
			base.Update(time);
		}
		public virtual void InitBlueprints ()
		{
			// Set up the blueprint from the vertices of he body here
			_Blueprints = new List<IRenderable>();
			for (int i = 0; i < Body.FixtureList.Count; i++) {
				Fixture fixture = Body.FixtureList[i];
				if(fixture.ShapeType == FarseerPhysics.Collision.Shapes.ShapeType.Polygon)
				{
					var poly_shape = (FarseerPhysics.Collision.Shapes.PolygonShape)fixture.Shape;
					Vector3d[] verts = new Vector3d[poly_shape.Vertices.Count];
					for(int j = 0; j < poly_shape.Vertices.Count; j++)
						verts[j] = new Vector3d(
							(poly_shape.Vertices[j].X) * Configuration.MeterInPixels,
							(poly_shape.Vertices[j].Y) * Configuration.MeterInPixels, 0.0);
					_Blueprints.Add (new BlueprintLineLoop(this, 0, verts));
				}
				else if(fixture.ShapeType == FarseerPhysics.Collision.Shapes.ShapeType.Edge)
				{
					var poly_shape = (FarseerPhysics.Collision.Shapes.EdgeShape)fixture.Shape;
					var p0 = new Vector3d(
						(poly_shape.Vertex1.X) * Configuration.MeterInPixels,
						(poly_shape.Vertex1.Y) * Configuration.MeterInPixels, 0.0);
					var p1 = new Vector3d(
						(poly_shape.Vertex2.X) * Configuration.MeterInPixels,
						(poly_shape.Vertex2.Y) * Configuration.MeterInPixels, 0.0);
					_Blueprints.Add (new BlueprintLineLoop(this, 0, p0, p1));
				}
			}
		}
		public virtual void Derez()
		{
            _SpriteBody.Dispose();
			_RenderSet.Remove(this);
		}
		#endregion
	}
}

