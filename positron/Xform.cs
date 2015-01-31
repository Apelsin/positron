using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Positron
{
    public class Xform : Extension
    {
        protected Matrix4 _Local;
        protected Xform _Parent;
        public Xform Parent { get { return _Parent; } set { _Parent = value; } }
        protected GameObject _GameObject;
        
        // Xform GameObject is get-only
        public override GameObject GameObject { get { return _GameObject; } }
        public Xform (GameObject game_object)
        {
            _Transform = new Matrix4();
            _GameObject = game_object;
        }

        public Matrix4 GlobalMatrix
        {
            get
            {
                if (GameObject.Body != null)
                {
                    // do a little dance
                }
                return _Local;
            }
        }
        public Matrix4 ParentMatrix
        {
            get
            {
                if (GameObject.Body != null)
                {
                    // do a little dance
                }
                if (Parent != null)
                    return Parent.GlobalMatrix;
                else
                    return _Local;
            }
        }
        public Matrix4 LocalMatrix
        {
            get
            {
                if(GameObject.Body != null)
                {
                    // do a little dance
                }
                return _Local;
            }
        }
        public Vector3 Position
        {
            get
            {
                if (GameObject.Body != null)
                {
                    // do a little dance
                }
                return _Transform;
            }
        }
    }
}
