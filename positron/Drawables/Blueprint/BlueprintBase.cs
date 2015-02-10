using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Positron
{
    public abstract class BlueprintBase : Extension, IRenderable
    {
        public static Color[] BluePrintColorSequence = { Color.MediumAquamarine, Color.Aqua };
        public abstract void Render();
        public BlueprintBase(GameObject game_object) : base(game_object) { }
    }
}
