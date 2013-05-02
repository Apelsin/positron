using System;

namespace positron
{
    public class RadioProp : SpriteObject
    {
        public RadioProp (RenderSet render_set, double x, double y) :
            base(render_set, x, y, Texture.Get ("sprite_radio"))
        {
            PlayAnimation(new SpriteAnimation(Texture, 250, true, "f1", "f2"));
        }
        protected override void InitPhysics()
        {
            base.InitPhysics();
            _SpriteBody.CollisionCategories = FarseerPhysics.Dynamics.Category.None;
        }
    }
}

