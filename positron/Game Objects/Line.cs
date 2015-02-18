using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace Positron
{
    public class Line2 : GameObject, IColorable
    {
        protected Body _Body;
        public override Body mBody
        {
            get
            {
                return _Body;
            }
        }
        protected Vector2 _Begin;
        public Vector2 Begin
        {
            get { return _Begin; }
            set { _Begin = value; }
        }
        protected Vector2 _End;
        public Vector2 End
        {
            get { return _End; }
            set { _End = value; }
        }
        protected Color _Color;
        public Color Color
        {
            get { return _Color; }
            set { _Color = value; }
        }
        public float Thickness { get; set; }

        public Line2 (Xform parent, Vector2 start, Vector2 end):
            this(parent, start, end, 1.0f)
        {
        }
        public Line2 (Xform parent, Vector2 start, Vector2 end, float thickness):
            base(parent)
        {
            _Begin = start;
            _End = end;
            _Color = Color.Black;
            Thickness = thickness;
        }
        private void RebuildBody()
        {
            if (_Body == null)
            {
                _Body = BodyFactory.CreateBody(mScene.World, mTransform.PositionLocalXY.XNA());
                _Body.BodyType = BodyType.Static;
            }
            else
            {
                foreach (Fixture fixture in _Body.FixtureList)
                    _Body.DestroyFixture(fixture);
            }
            FixtureFactory.AttachEdge(_Begin.XNA(), _End.XNA(), _Body);
        }
        public override void Draw()
        {
            // Unbind any texture that was previously bound
            GL.BindTexture (TextureTarget.Texture2D, 0);
            GL.LineWidth (Thickness);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4 (_Color);
            GL.Vertex2 (_Begin);
            GL.Vertex2 (_End);
            GL.End ();
        }
    }
}

