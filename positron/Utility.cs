using System;
using System.Runtime.InteropServices;
using System.IO;

namespace positron.Utility
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

