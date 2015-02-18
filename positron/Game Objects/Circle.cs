using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Positron
{
    public class Circle : GameObject, IColorable
    {
        Color _Color;
        public Color Color {
            get { return _Color; }
            set { _Color = value; }
        }
        public float Radius { get; set; }

        public Circle (Xform parent, float x, float y, float radius):
            base(parent)
        {
            Color = Color.White;
            Radius = radius;
            mTransform.PositionLocalX = x;
            mTransform.PositionLocalY = y;
        }
        public Circle(GameObject parent, float x, float y, float radius):
            this(parent.mTransform, x, y, radius)
        {
        }
        public override void Draw()
        {
            GL.PointSize(Radius);
            GL.Enable(EnableCap.PointSmooth);
            GL.Begin(PrimitiveType.Points);
            GL.Color4(Color);
            GL.Vertex2(0,0);
            GL.End();
        }
    }
}

