using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Positron
{
    class Attachment : GameObject
    {
        protected GameObject _Target;
        public GameObject Target {
            get { return _Target; } set { _Target = value; } }
        public override ThreadedRendering Window {
            get { return _Target != null ?_Target.Window : null; } }
        public override PositronGame Game {
            get { return _Target != null ? _Target.Game : null; } }
        public override Scene Scene {
            get { return _Target != null ? _Target.Scene : null; } }
        public override RenderSet RenderSet {
            get { return _Target != null ? _Target.RenderSet : null; } }
        public override FarseerPhysics.Dynamics.Body Body {
            get { return _Target != null ?_Target.Body : null; } }
        public override Drawable Drawable {
            get { return _Target != null ?_Target.Drawable : null; } }
        public override SpriteBase Sprite {
            get { return _Target != null ?_Target.Sprite : null; } }
    }
}
