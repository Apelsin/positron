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

namespace Positron
{
	public class SpriteObject: SpriteBase, IWorldObject
	{
		public event DerezEventHandler DerezEvent;
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
		#region Hell
		private static void noop() { }
		public Vector3 PositionWorld {
            get { if(Body == null) return Vector3.Zero; return new Vector3(Body.Position.X, Body.Position.Y, _Position.Z / Configuration.MeterInPixels); }
			set {
                if(Body == null) return;
                Body.Position = new Microsoft.Xna.Framework.Vector2((float)value.X, (float)value.Y); _Position.Z = value.Z * Configuration.MeterInPixels; }
		}
		public float PositionWorldX {
            get { if(Body == null) return 0.0f; return Body.Position.X; }
            set { if(Body == null) return; Body.Position = new Microsoft.Xna.Framework.Vector2((float)value, Body.Position.Y); }
		}
		public float PositionWorldY {
            get { if(Body == null) return 0.0f; return Body.Position.Y; }
            set { if(Body == null) return; Body.Position = new Microsoft.Xna.Framework.Vector2(Body.Position.X, (float)value); }
		}
		public override Vector3 Position {
			get { return PositionWorld * Configuration.MeterInPixels; }
			set { PositionWorld = value / Configuration.MeterInPixels; }
		}
		public override float PositionX {
			get { return PositionWorldX * Configuration.MeterInPixels; }
			set { PositionWorldX = value / Configuration.MeterInPixels; }
		}
		public override float PositionY {
			get { return PositionWorldY * Configuration.MeterInPixels; }
			set { PositionWorldY = value / Configuration.MeterInPixels; }
		}
		public Vector3 VelocityWorld {
			get {
				return new Vector3(
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
		public float VelocityWorldX {
			get { return (float)Body.LinearVelocity.X; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2((float)value, Body.LinearVelocity.Y); }
		}
		public float VelocityWorldY {
			get { return (float)Body.LinearVelocity.Y; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, (float)value); }
		}
		public override Vector3 Velocity {
			get { return VelocityWorld * Configuration.MeterInPixels; }
			set { VelocityWorld = value / Configuration.MeterInPixels; }
		}
		public override float VelocityX {
			get { return (float)Body.LinearVelocity.X *  Configuration.MeterInPixels; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2((float)(value / Configuration.MeterInPixels), Body.LinearVelocity.Y); }
		}
		public override float VelocityY {
			get { return (float)Body.LinearVelocity.Y *  Configuration.MeterInPixels; }
			set { Body.LinearVelocity = new Microsoft.Xna.Framework.Vector2(Body.LinearVelocity.X, (float)(value / Configuration.MeterInPixels)); }
		}

        // TODO: thrown an exception instead of silently failing
		public override float Theta {
            get { if(Body == null) return 0.0f; return Body.Rotation; }
            set { if(Body == null) return; Body.Rotation = (float)value; }
		}
		#endregion

		#region Behavior
		public SpriteObject(RenderSet render_set):
			this(render_set,Texture.DefaultTexture)
		{
		}
		public SpriteObject (RenderSet render_set, float x, float y, Texture texture):
			this(render_set, texture)
		{
			Corner = new Vector3(x, y, 0.0f);
		}
		public SpriteObject (RenderSet render_set, SpriteObject relative_to, float x, float y, Texture texture):
			this(render_set, x, y, texture)
		{
			Corner = new Vector3(relative_to.CornerX + x, relative_to.CornerY + y, relative_to.PositionZ);;
		}
		public SpriteObject (RenderSet render_set, Texture texture):
			base(render_set, texture)
		{
			_RenderSet = render_set;

			InitPhysics();
			// HACK: Positioning the body must occur after world
			// has been solved at least one time in order to avoid
			// fixtures being shifted rather than the bodies.
			_RenderSet.Scene.Game.WorldMain.Step (0.0f);

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
			_SpriteBody = BodyFactory.CreateBody(_RenderSet.Scene.World, Microsoft.Xna.Framework.Vector2.Zero, _Theta, BodyType.Static, this);
			FixtureFactory.AttachRectangle(w, h, 100.0f, Microsoft.Xna.Framework.Vector2.Zero, _SpriteBody);
			_SpriteBody.Friction = 0.5f;

			// HACK: Only enable bodies for which the object is in the current scene
			Body.Enabled = _RenderSet.Scene == _RenderSet.Scene.Game.CurrentScene;

			InitBlueprints();
		}
		protected virtual void EnteredRenderSet (object sender, RenderSetChangeEventArgs e)
		{
            if(Body != null)
			    Body.Enabled = (Set.Scene == e.To.Scene);
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
		public override void Render (float time)
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
                    (_Theta + (float)OpenTK.MathHelper.RadiansToDegrees(_SpriteBody.Rotation))
                    //	/ 45.0))
                        ;
                GL.Rotate(r, 0.0, 0.0, 1.0);
                GL.Scale(_Scale);
                Draw();
            }
            GL.PopMatrix();
		}
		public override void Update (float time)
		{
			base.Update(time);
		}
		public virtual void InitBlueprints ()
		{
			// Set up the blueprint from the vertices of he body here
			_Blueprints = new List<IRenderable>();
			for (int i = 0; i < Body.FixtureList.Count; i++) {
				Fixture fixture = Body.FixtureList[i];
				if(fixture.Shape.ShapeType == FarseerPhysics.Collision.Shapes.ShapeType.Polygon)
				{
					var poly_shape = (FarseerPhysics.Collision.Shapes.PolygonShape)fixture.Shape;
					Vector3[] verts = new Vector3[poly_shape.Vertices.Count];
					for(int j = 0; j < poly_shape.Vertices.Count; j++)
						verts[j] = new Vector3(
							(poly_shape.Vertices[j].X) * Configuration.MeterInPixels,
							(poly_shape.Vertices[j].Y) * Configuration.MeterInPixels, 0.0f);
					_Blueprints.Add (new BlueprintLineLoop(this, 0, verts));
				}
				else if(fixture.Shape.ShapeType == FarseerPhysics.Collision.Shapes.ShapeType.Edge)
				{
					var poly_shape = (FarseerPhysics.Collision.Shapes.EdgeShape)fixture.Shape;
					var p0 = new Vector3(
						(poly_shape.Vertex1.X) * Configuration.MeterInPixels,
						(poly_shape.Vertex1.Y) * Configuration.MeterInPixels, 0.0f);
					var p1 = new Vector3(
						(poly_shape.Vertex2.X) * Configuration.MeterInPixels,
						(poly_shape.Vertex2.Y) * Configuration.MeterInPixels, 0.0f);
					_Blueprints.Add (new BlueprintLineLoop(this, 0, p0, p1));
				}
			}
		}
		public virtual void Derez()
		{
			if(DerezEvent != null)
				DerezEvent(this, null);
            _SpriteBody.Dispose();
			_RenderSet.Remove(this);
		}
		public override void Dispose()
		{
			DerezEvent = null;
            if(Body != null)
			    Body.Dispose();
			base.Dispose();
		}
		#endregion
	}
}

