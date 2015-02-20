using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Positron.Utility
{
    public static class Structure
    {
        public enum Endianness
        {
            BigEndian,
            LittleEndian
        }

        public static int FixEndianness (this int self, Endianness endianness = Endianness.BigEndian)
        {
            if ((BitConverter.IsLittleEndian) == (endianness == Endianness.LittleEndian))
            {
                // nothing to change => return
                return self;
            }
            byte[] reverse_array = BitConverter.GetBytes(self);
            Array.Reverse(reverse_array);
            return BitConverter.ToInt32(reverse_array, 0);
        }

        public static string ReadPascalString (this BinaryReader reader, Endianness endianness = Endianness.BigEndian)
        {
            int str_len = reader.ReadInt32().FixEndianness(endianness);
            return new string(reader.ReadChars(str_len));
        }

        private static void MaybeAdjustEndianness(Type type, byte[] data, Endianness endianness, int startOffset = 0)
        {
            if ((BitConverter.IsLittleEndian) == (endianness == Endianness.LittleEndian))
            {
                // nothing to change => return
                return;
            }
            
            
            
            foreach (var field in type.GetFields())
            {
                var fieldType = field.FieldType;
                if (field.IsStatic)
                    // don't process static fields
                    continue;
                
                if (fieldType == typeof(string)) 
                    // don't swap bytes for strings
                    continue;
                
                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

                var effectiveOffset = startOffset + offset;

                // check for sub-fields to recurse if necessary
                var fields = fieldType.GetFields();
                bool sub_fields_exist = false;
                for(int i = 0; i < fields.Length; i++)
                {
                    if(!fields[i].IsStatic)
                    {
                        sub_fields_exist = true;
                        break;
                    }
                }
                if (!sub_fields_exist)
                {
                    Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
                }
                else
                {
                    // recurse
                    MaybeAdjustEndianness(fieldType, data, endianness, effectiveOffset);
                }
            }
        }
        
        internal static T BytesToStruct<T>(byte[] rawData, Endianness endianness, int bytes_offset = 0) where T : struct
        {
            T result = default(T);
            
            MaybeAdjustEndianness(typeof(T), rawData, endianness);
            
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            
            try
            {
                IntPtr rawDataPtr = new IntPtr(handle.AddrOfPinnedObject().ToInt64() + bytes_offset);
                result = (T)Marshal.PtrToStructure(rawDataPtr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
            
            return result;
        }
        
        internal static byte[] StructToBytes<T>(T data, Endianness endianness) where T : struct
        {
            byte[] rawData = new byte[Marshal.SizeOf(data)];
            GCHandle handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                IntPtr rawDataPtr = handle.AddrOfPinnedObject();
                Marshal.StructureToPtr(data, rawDataPtr, false);
            }
            finally
            {
                handle.Free();
            }
            
            MaybeAdjustEndianness(typeof(T), rawData, endianness);
            
            return rawData;
        }
    }
    public static class Codec
    {
        #region Parsing and Encoding
        internal static char[] SpaceSeparator = new char[] { ' ' };
        public static string M4Encode(ref Matrix4 M)
        {
            return String.Format(
                "{00} {01} {02} {03} {04} {05} {06} {07} {08} {09} {10} {11} {12} {13} {14} {15}",
                M.M11, M.M12, M.M13, M.M14,
                M.M21, M.M22, M.M23, M.M24,
                M.M31, M.M32, M.M33, M.M34,
                M.M41, M.M42, M.M43, M.M44
                );
        }
        public static Matrix4 M4Decode(ref string S)
        {
            string[] C = S.Split(SpaceSeparator, 16);
            return new Matrix4(
                float.Parse(C[00]), float.Parse(C[01]), float.Parse(C[02]), float.Parse(C[03]),
                float.Parse(C[04]), float.Parse(C[05]), float.Parse(C[06]), float.Parse(C[07]),
                float.Parse(C[08]), float.Parse(C[09]), float.Parse(C[10]), float.Parse(C[11]),
                float.Parse(C[12]), float.Parse(C[13]), float.Parse(C[14]), float.Parse(C[15]));
        }
        #endregion
    }
}

namespace Positron
{
    public static class P_GL
    {
        public static void __TranslateHard(float x, float y, float z = 0.0f)
        {
            GL.Translate((int)x, (int)y, (int)z);
        }
        public static void __TranslateHard(Vector3 p)
        {
            GL.Translate((int)p.X, (int)p.Y, (int)p.Z);
        }
    }
    public static class Physics
    {
        public delegate float RayCastCallback(FarseerPhysics.Dynamics.Fixture fixture, Microsoft.Xna.Framework.Vector2  point, Microsoft.Xna.Framework.Vector2 normal, float fraction);
    }
    public static class ExtensionMethods
    {
        #region Typecasting
        // C# needs unions or something...
        public static Microsoft.Xna.Framework.Vector3 XNA(this Vector3 self)
        {
            return new Microsoft.Xna.Framework.Vector3(self.X, self.Y, self.Z);
        }
        public static Microsoft.Xna.Framework.Vector2 XNA(this Vector2 self)
        {
            return new Microsoft.Xna.Framework.Vector2(self.X, self.Y);
        }
        public static Vector3 OTK(this Microsoft.Xna.Framework.Vector3 self)
        {
            return new Vector3(self.X, self.Y, self.Z);
        }
        public static Vector2 OTK(this Microsoft.Xna.Framework.Vector2 self)
        {
            return new Vector2(self.X, self.Y);
        }
        #endregion
        #region Dictionary Helpers
        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dict, K key)
        {
            return dict.GetValueOrDefault(key, default(V));
        }
        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dict, K key, V defVal)
        {
            return dict.GetValueOrDefault(key, () => defVal);
        }
        public static V GetValueOrDefault<K, V>(this IDictionary<K, V> dict, K key, Func<V> defValSelector)
        {
            V value;
            return dict.TryGetValue(key, out value) ? value : defValSelector();
        }
        public static bool SetValueIfNotDefined<K, V>(this IDictionary<K, V> dict, K key, V value)
        {
            if (dict.ContainsKey(key))
                return false;
            dict[key] = value;
            return true;
        }
        #endregion
    }
}