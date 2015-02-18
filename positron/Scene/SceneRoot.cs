using System;
using System.Collections.Generic;

namespace Positron
{
    public class SceneRoot : Xform
    {
        protected Scene _Scene;
        public override Scene mScene { get { return _Scene; } }
        protected Camera _Camera;
        public new Camera mCamera { get { return _Camera; } set { AddChild((_Camera = value).mTransform); } }
        public SceneRoot(Scene scene, Camera camera):
            base(null, null)
        {
            _Scene = scene;
            mCamera = camera;
        }
        public SceneRoot (Scene scene):
            base(null, null)
        {
            _Scene = scene;
            _Camera = new Camera(this);
        }
        public override void Dispose()
        {
            base.Dispose();
            _Scene = null;
        }
    }
}

