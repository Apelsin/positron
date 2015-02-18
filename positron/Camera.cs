using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Positron
{
    public class Camera : GameObject
    {
        protected Xform _Target;
        /// <summary>
        /// Target for the camera to focus on; affects camera rotation
        /// </summary>
        public Xform Target { get { return _Target; } set { _Target = value; } }
        protected float _FieldOfView = (float)Math.PI / 2f;
        /// <summary>
        /// Field of view angle in radians
        /// </summary>
        public float FieldOfView { get { return _FieldOfView; } set { _FieldOfView = value; } }
        /// <summary>
        /// Field of view angle in degrees
        /// </summary>
        public float FieldOfViewDeg {
            get { return MathHelper.RadiansToDegrees(_FieldOfView); }
            set { _FieldOfView = MathHelper.DegreesToRadians(value); }
        }
        /// <summary>
        /// Camera object for controlling viewports
        /// </summary>
        public Camera(Xform parent) : base(parent)
        {
        }
    }
}
