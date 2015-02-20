using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Positron
{
    [DataContract]
    public abstract class Extension : GameObjectBase, IDisposable
    {
        protected GameObject _GameObject;
        public Extension(GameObject game_object) : base ()
        {
            _GameObject = game_object;
        }
        public override Xform mTransform {
            get { return _GameObject.mTransform; }
            internal set { _GameObject.mTransform = value;  }
        }
        [DataMember]
        public virtual GameObject mGameObject {
            get { return _GameObject; }
            internal set { _GameObject = value;  }
        }
        public virtual void Dispose()
        {
            _GameObject = null;
        }
    }
}
