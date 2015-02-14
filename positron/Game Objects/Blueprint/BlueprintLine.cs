using System;
using System.Diagnostics;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public class BlueprintLine : BlueprintBase
    {
        public Vector3 A, B;
        public BlueprintLine (GameObject game_object, Vector3 a, Vector3 b):
            base(game_object)
        {
            A = a;
            B = b;
        }
        public override void Draw ()
        {
            // Unbind any texture that was previously bound
            GL.BindTexture (TextureTarget.Texture2D, 0);
            GL.LineWidth (1);
            GL.Begin (PrimitiveType.Lines);
            GL.Color4(BlueprintBase.BluePrintColorSequence[0]);
            GL.Vertex3 (A);
            GL.Color4 (BlueprintBase.BluePrintColorSequence[1]);
            GL.Vertex3 (B);
            GL.End ();
        }
    }
}

