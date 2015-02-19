using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;

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