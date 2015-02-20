using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using OpenTK;

namespace Positron
{
    [DataContract]
    public class Xform : Extension, IDisposable
    {
        internal override void OnDeserialized(StreamingContext context)
        {
            UpdateGlobalMatrix();
            if (_GameObject != null)
            {
                _GameObject.mTransform = this;
            }
            foreach (Xform child in _Children)
                child._Parent = this;
        }
        [DataContract]
        public class State
        {
            public bool Modified = false;
        }
        protected State _State;
        [DataMember]
        public State mState
        {
            get { return _State; }
            internal set { _State = value; }
        }

        // These are public for optimization
        public Matrix4 _Local = Matrix4.Identity;
        public Matrix4 _Global = Matrix4.Identity;
        protected Xform _Parent;

        public Xform Parent {
            get { return _Parent; }
            internal set { _Parent = value; }
        }

        protected HashSet<Xform> _Children = new HashSet<Xform>();
        [DataMember]
        public HashSet<Xform> Children {
            get { return _Children; }
            internal set { _Children = value; }
        }
        public void RemoveChild(Xform xform)
        {
            if (xform.Parent == this)
                _Children.Remove(xform);
            xform.Parent = this;
            _Children.Add(xform);
        }
        public void AddChild(Xform xform)
        {

            if (this.IsDescendantOf(xform))
                throw new InvalidOperationException("Attepted to make Xform be a descendant of itself.");
            if (xform.Parent != null)
                xform.Parent.RemoveChild(xform);
            xform.Parent = this;
            _Children.Add(xform);
        }

        public bool IsChildOf(Xform parent)
        {
            return _Parent == parent;
        }
        public bool IsDescendantOf(Xform parent)
        {
            return IsDescendantOf(parent, this);
        }

        protected static bool IsDescendantOf(Xform parent, Xform child)
        {
            if (child == null)
                return false;
            else if (child.IsChildOf(parent))
                return true;
            return IsDescendantOf(parent, child._Parent);
        }
        public Xform FindChildById(string element_id)
        {
            return FindChildById(this, element_id);
        }
        protected static Xform FindChildById(Xform parent, string element_id)
        {
            Xform x;
            foreach(Xform child in parent._Children)
            {
                if (child.ElementId == element_id)
                    return child;
                else if((x = FindChildById(child, element_id)) != null)
                    return x;
            }
            return null;
        }
        public GameObject FindGameObjectById(string element_id)
        {
            return FindGameObjectById(this, element_id);
        }
        protected static GameObject FindGameObjectById(Xform parent, string element_id)
        {
            GameObject o;
            foreach (Xform child in parent._Children)
            {
                if (child.mGameObject != null && child.mGameObject.ElementId == element_id)
                    return child.mGameObject;
                else if ((o = FindGameObjectById(child, element_id)) != null)
                    return o;
            }
            return null;
        }
        public Xform(GameObject game_object, Xform parent)
            : base(game_object)
        {
            if(parent != null)
                parent.AddChild(this);
            UpdateGlobalMatrix();
            mState = new State();
        }
        #region Matrices
        public void UpdateGlobalMatrix()
        {
            _Global = _Local * ParentMatrix;
        }
        public Matrix4 GlobalMatrix
        {
            get
            {
                return _Global;
            }
            // TODO: GlobalMatrix set
        }
        public Matrix4 ParentMatrix
        {
            get
            {
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
                return _Local;
            }
            set
            {
                _Local = value;
            }
        }
        [DataMember]
        internal String M
        {
            get
            {
                return Positron.Utility.Codec.M4Encode(ref _Local);
            }
            set
            {
                _Local = Positron.Utility.Codec.M4Decode(ref value);
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            _Parent = null;
            _Children = null;
        }
        #endregion
        #region Positions
        public Vector3 PositionLocal
        {
            get
            {
                return new Vector3(_Local[3, 0], _Local[3, 1], _Local[3, 2]);
            }
            set
            {
                _Local[3,0] = value.X;
                _Local[3,1] = value.Y;
                _Local[3,2] = value.Z;
            }
        }
        public Vector2 PositionLocalXY
        {
            get
            {
                return new Vector2(_Local[3, 0], _Local[3, 1]);
            }
            set
            {
                _Local[3, 0] = value.X;
                _Local[3, 1] = value.Y;
            }
        }
        public float PositionLocalX
        {
            get { return _Local[3, 0]; }
            set { _Local[3, 0] = value; }
        }
        public float PositionLocalY
        {
            get { return _Local[3, 1]; }
            set { _Local[3, 1] = value; }
        }
        public float PositionLocalZ
        {
            get { return _Local[3, 2]; }
            set { _Local[3, 2] = value; }
        }
        public Vector3 Position
        {
            get
            {
                Matrix4 M = GlobalMatrix;
                return new Vector3(M[3, 0], M[3, 1], M[3, 2]);
            }
        }
        public Vector2 PositionXY
        {
            get
            {
                Matrix4 M = GlobalMatrix;
                return new Vector2(M[3, 0], M[3, 1]);
            }
        }
        public float PositionX
        {
            get
            {
                Matrix4 M = GlobalMatrix;
                return M[3, 0];
            }
        }
        public float PositionY
        {
            get
            {
                Matrix4 M = GlobalMatrix;
                return M[3, 1];
            }
        }
        public float PositionZ
        {
            get
            {
                Matrix4 M = GlobalMatrix;
                return M[3, 2];
            }
        }
        #endregion
        #region Scales
        public Vector3 ScaleLocal
        {
            get
            {
                return new Vector3(_Local[0, 0], _Local[1, 1], _Local[2, 2]);
            }
            set
            {
                _Local[0, 0] = value.X;
                _Local[1, 1] = value.Y;
                _Local[2, 2] = value.Z;
            }
        }
        public Vector2 ScaleLocalXY
        {
            get
            {
                return new Vector2(_Local[0, 0], _Local[1, 1]);
            }
            set
            {
                _Local[0, 0] = value.X;
                _Local[1, 1] = value.Y;
            }
        }
        public float ScaleLocalX
        {
            get
            {
                return _Local[0, 0];
            }
            set
            {
                _Local[0, 0] = value;
            }
        }
        public float ScaleLocalY
        {
            get
            {
                return _Local[1, 1];
            }
            set
            {
                _Local[1, 1] = value;
            }
        }
        public float ScaleLocalZ
        {
            get
            {
                return _Local[2, 2];
            }
            set
            {
                _Local[2, 2] = value;
            }
        }
        #endregion
        #region Rotations
        #region Euler Angles
        /// <summary>
        /// Euler angle: Heading
        /// </summary>
        public float LocalEulerH
        {
            get
            {
                return GetEulerH(ref _Local);
            }
            set
            {
                SetEulerH(ref _Local, value);
            }
        }
        /// <summary>
        /// Euler angle: Pitch (Elevation)
        /// </summary>
        public float LocalEulerP
        {
            get
            {
                return GetEulerP(ref _Local);
            }
            set
            {
                SetEulerP(ref _Local, value);
            }
        }
        /// <summary>
        /// Euler angle: Bank
        /// </summary>
        public float LocalEulerB
        {
            get
            {
                return GetEulerB(ref _Local);
            }
            set
            {
                SetEulerB(ref _Local, value);
            }
        }
        /// <summary>
        /// Euler angles: Heading, Pitch (Elevation) and Bank
        /// </summary>
        public Vector3 LocalEulerAngles
        {
            get
            {
                return GetEulerAngles(ref _Local);
            }
            set
            {
                UpdateEulerAngles(ref _Local, value.X, value.Y, value.Z);
            }
        }
        #region Static
        public static bool IsAligned(ref Matrix4 M)
        {
            return M[0, 0] == 1f || M[0, 0] == -1f;
        }
        public static float GetEulerH(ref Matrix4 M)
        {
            if (IsAligned(ref M))
                return (float)Math.Atan2(M[0, 2], M[2, 3]);
            return (float)Math.Atan2(-M[2, 0], M[0, 0]);
        }
        public static float GetEulerP(ref Matrix4 M)
        {
            if (IsAligned(ref M))
                return 0f;
            return (float)Math.Asin(M[1, 0]);
        }
        public static float GetEulerB(ref Matrix4 M)
        {
            if (IsAligned(ref M))
                return 0f;
            return (float)Math.Atan2(-M[1, 2], M[1, 1]);
        }
        public static Vector3 GetEulerAngles(ref Matrix4 M)
        {
            if (IsAligned(ref M))
                return new Vector3((float)Math.Atan2(M[0, 2], M[2, 3]), 0f, 0f);
            return new Vector3(
                (float)Math.Atan2(-M[2, 0], M[0, 0]),
                (float)Math.Asin(M[1, 0]),
                (float)Math.Atan2(-M[1, 2], M[1, 1])
                );
        }
        public static void SetEulerH(ref Matrix4 M, float angle)
        {
            if (IsAligned(ref M))
                UpdateEulerAngles(ref M, angle, 0f, 0f);
            else
                UpdateEulerAngles(ref M,
                    angle,
                    (float)Math.Asin(M[1, 0]),
                    (float)Math.Atan2(-M[1, 2], M[1, 1]));
        }
        public static void SetEulerP(ref Matrix4 M, float angle)
        {
            if (IsAligned(ref M))
                UpdateEulerAngles(ref M, (float)Math.Atan2(M[0, 2], M[2, 3]), angle, 0f);
            else
                UpdateEulerAngles(ref M,
                    (float)Math.Atan2(-M[2, 0], M[0, 0]),
                    angle,
                    (float)Math.Atan2(-M[1, 2], M[1, 1]));
        }
        public static void SetEulerB(ref Matrix4 M, float angle)
        {
            if (IsAligned(ref M))
                UpdateEulerAngles(ref M, (float)Math.Atan2(M[0, 2], M[2, 3]), 0f, angle);
            else
                UpdateEulerAngles(ref M,
                    (float)Math.Atan2(-M[2, 0], M[0, 0]),
                    (float)Math.Asin(M[1, 0]),
                    angle);
        }
        /// <summary>
        /// Update the rotation of the matrix from HPB Euler angles
        /// </summary>
        /// <param name="M">Matrix to update</param>
        /// <param name="angle_h">Heading angle</param>
        /// <param name="angle_p">Pitch angle</param>
        /// <param name="angle_b">Bank angle</param>
        public static void UpdateEulerAngles(ref Matrix4 M, float angle_h, float angle_p, float angle_b)
        {
            float Ch = (float)Math.Cos(angle_h);
            float Sh = (float)Math.Sin(angle_h);
            float Cp = (float)Math.Cos(angle_p);
            float Sp = (float)Math.Sin(angle_p);
            float Cz = (float)Math.Cos(angle_b);
            float Sz = (float)Math.Sin(angle_b);

            float AD = Ch * Sp;
            float BD = Sh * Sp;

            M[0, 0] = Cp * Cz;
            M[0, 1] = -Cp * Sz;
            M[0, 2] = -Sp;
            M[1, 0] = -BD * Cz + Ch * Sz;
            M[1, 1] = BD * Sz + Ch * Cz;
            M[1, 2] = -Sh * Cp;
            M[2, 0] = AD * Cz + Sh * Sz;
            M[2, 1] = -AD * Sz + Sh * Cz;
            M[2, 2] = Ch * Cp;

            M[0, 3] = M[1, 3] = M[2, 3] = M[3, 0] = M[3, 1] = M[3, 2] = 0;
            M[3, 3] = 1;
        }
        #endregion
        #endregion
        #endregion    
    }
}
