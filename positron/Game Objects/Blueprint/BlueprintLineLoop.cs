using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron
{
    public class BlueprintLineLoop : BlueprintBase, IDrawable
    {
        public Vector3[] Vertices;
        public BlueprintLineLoop (GameObject game_object, params Vector3[] vertices):
            base(game_object)
        {
            Vertices = vertices;
        }
        public override void Draw()
        {
            int color_index = 0;
            // Unbind any texture that was previously bound
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.LineWidth(1);
            GL.Begin(PrimitiveType.LineLoop);
            for(int i = 0; i < Vertices.Length; i++)
            {
                Color color = BlueprintBase.BluePrintColorSequence[color_index++];
                GL.Color4(color);
                GL.Vertex3(Vertices[i]);
            }
            GL.End();
        }
    }
}

