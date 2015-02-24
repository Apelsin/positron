using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Positron
{
    [DataContract]
    public class SceneRoot : Xform
    {
        internal virtual void OnDeserialized(StreamingContext context)
        {
            mCamera = (Camera)FindGameObjectById(CameraId);
        }
        protected Scene _Scene;
        public override Scene mScene {
            get { return _Scene; }
            internal set { _Scene = value; }
        }
        internal Camera _Camera;
        public override Camera mCamera
        {
            get { return _Camera; }
            internal set { _Camera = value; CameraId = _Camera.ElementId; }
        }
        [DataMember]
        internal string CameraId
        {
            get;
            set;
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

