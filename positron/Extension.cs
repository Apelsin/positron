using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Positron
{
    public abstract class Extension : GameObjectBase
    {
        protected readonly GameObject _GameObject;
        public Extension(GameObjectBase game_object)
        {
            _GameObject = game_object;
        }
        public virtual GameObject mGameObject {
            get { return _GameObject; } }
        public override ThreadedRendering mWindow {
            get { return _GameObject != null ? _GameObject.mWindow : null; } }
        public override PositronGame mGame {
            get { return _GameObject != null ? _GameObject.mGame : null; } }
        public override Scene mScene {
            get { return _GameObject != null ? _GameObject.mScene : null; } }
        public override FarseerPhysics.Dynamics.World mWorld {
            get { return _GameObject != null ? _GameObject.mWorld : null; } }
        public override RenderSet mRenderSet {
            get { return _GameObject != null ? _GameObject.mRenderSet : null; } }
        public override FarseerPhysics.Dynamics.Body mBody {
            get { return _GameObject != null ? _GameObject.mBody : null; } }
        public override Drawable mDrawable {
            get { return _GameObject != null ? _GameObject.mDrawable : null; } }
        public override SpriteBase mSprite {
            get { return _GameObject != null ? _GameObject.mSprite : null; } }
    }
}
