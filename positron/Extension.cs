using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Positron
{
    public abstract class Extension : GameObjectBase, IDisposable
    {
        protected GameObject _GameObject;
        public Extension(GameObject game_object) : base ()
        {
            _GameObject = game_object;
        }
        public override Xform mTransform { get { return _GameObject.mTransform; } }
        public virtual GameObject mGameObject {
            get { return _GameObject; } }
        public virtual void Dispose()
        {
            _GameObject = null;
        }
    }
}
