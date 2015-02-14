using System;
using System.Collections.Generic;

namespace Positron
{
    public class SceneRoot : Xform
    {
        protected Scene _Scene;
        public override Scene mScene { get { return _Scene; } }
        public SceneRoot (Scene scene):
            base(null, null)
        {
            _Scene = scene;
        }
        public override void Dispose()
        {
            base.Dispose();
            _Scene = null;
        }
    }
}

