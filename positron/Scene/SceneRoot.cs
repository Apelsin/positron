using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Positron
{
    [DataContract]
    public class SceneRoot : Xform
    {
        protected Scene _Scene;
        public override Scene mScene {
            get { return _Scene; }
            internal set { _Scene = value; }
        }
        public override Camera mCamera
        {
            get { return mScene.Camera; }
            internal set { mScene.Camera = value; }
        }
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

