using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace Positron
{
    public class Xform : Extension
    {
        public class State
        {
            public bool Modified = false;
        }
        public readonly State mState;
        protected Matrix4 _Local;
        protected Matrix4 _Global;
        protected Xform _Parent;
        public Xform Parent {
            get
            {
                return _Parent;
            }
            set
            {
                _Parent = value;
            }
        }
        protected GameObject _GameObject;
        
        // Xform GameObject is get-only

        public Xform (GameObject game_object): base(game_object)
        {
            _Local = new Matrix4();
            _GameObject = game_object;
            mState = new State();
        }
        #region Matrices
        protected void UpdateGlobalMatrix()
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
