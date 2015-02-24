using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Positron
{
    public class BooleanIndicator: SpriteBase
    {
        public bool Value { get; set; }
        // Main constructor:
        public BooleanIndicator (Xform parent, float x, float y, bool value = false):
            base(parent, x, y, Texture.Get("sprite_indicator"))
        {
            Value = value;
            FrameCurrent.Color = Color.SkyBlue;
            new SpriteBase(mTransform, Texture.Get ("sprite_indicator_gloss"));
        }
        public override void Draw ()
        {
            GL.Color4(Value ? FrameCurrent.Color : Color.SlateGray);
            FrameCurrent.Texture.Bind();
            FrameCurrent.VBO.Render();
        }
    }
}