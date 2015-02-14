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

        public Circle (SceneRoot render_set, float radius):
            base(render_set)
        {
            Color = Color.Black;
            Radius = radius;
        }
        public override void Draw()
        {
            GL.PointSize(Radius);
            GL.Begin(PrimitiveType.Points);
            GL.Color4(Color);
            GL.Vertex2(mTransform.PositionLocalXY);
            GL.End();
        }
    }
}

