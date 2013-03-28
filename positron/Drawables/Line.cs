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
	public class Line : Drawable, IColorable, IWorldObject
	{
		private Color _Color;
		private Vector3d _Direction;

		private RenderSet _Scene;
		private Body _LineBody;
		private Fixture _LineFixture;
		
		public RenderSet RenderSet { get { return _Scene; } }
		public bool Preserve { get; set; }
		
		public Body Body { get { return _LineBody; } }
		public Fixture Fixture { get { return _LineFixture; } }

		public Color Color {
			get { return _Color; }
			set { _Color = value; }
		}
		public Vector3d Direction {
			get { return _Direction; }
			set { _Direction = value; }
		}
		public float Thickness { get; set; }

		public Line (Vector3d start, Vector3d end, RenderSet scene):
			this(start, end, 1.0f, scene)
		{
		}
		public Line (Vector3d start, Vector3d end, float thickness, RenderSet render_set):
			base(render_set)
		{
			_Position = start;
			_Direction = end;
			Color = Color.Black;
			Thickness = thickness;
			InitPhysics();
		}
		private void InitPhysics()
		{
			var start = new Microsoft.Xna.Framework.Vector2(
				(float)(_Position.X / Configuration.MeterInPixels),
				(float)(_Position.Y / Configuration.MeterInPixels));
			var direction = new Microsoft.Xna.Framework.Vector2(
				(float)(_Direction.X / Configuration.MeterInPixels),
				(float)(_Direction.Y / Configuration.MeterInPixels));
			_LineBody = BodyFactory.CreateBody(RenderSet.Scene.World, start);
			_LineFixture = FixtureFactory.AttachEdge(Microsoft.Xna.Framework.Vector2.Zero, direction, _LineBody);
			_LineBody.BodyType = BodyType.Static;
		}
		public override void Render (double time)
		{
			// Unbind any texture that was previously bound
			GL.BindTexture (TextureTarget.Texture2D, 0);
			GL.LineWidth (Thickness);
			GL.PushMatrix ();
			{
				GL.Translate (_LineBody.Position.X * Configuration.MeterInPixels,
				              _LineBody.Position.Y * Configuration.MeterInPixels, 0.0);
				GL.Rotate (_Theta + (double)OpenTK.MathHelper.RadiansToDegrees (_LineBody.Rotation), 0.0, 0.0, 1.0);
				GL.Begin (BeginMode.Lines);
				GL.Color4 (_Color);
				GL.Vertex3 (Vector3d.Zero);
				GL.Vertex3 (_Direction.X, _Direction.Y, _Direction.Z);
				GL.End ();
			}
			GL.PopMatrix ();
		}
		public void Update(double time)
		{
			// Do something!
		}
		public void SetChange (object sender, SetChangeEventArgs e)
		{
			this._Scene = e.To;
		}
		public override double RenderSizeX() { return _Size.X; }
		public override double RenderSizeY() { return _Size.Y; }
	}
}

