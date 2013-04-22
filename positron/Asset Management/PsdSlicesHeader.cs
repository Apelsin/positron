using System;
using System.Runtime.InteropServices;

namespace positron
{
	/// <summary>
	/// Specification: http://www.adobe.com/devnet-apps/photoshop/fileformatashtml/PhotoshopFileFormats.htm#50577409_19931
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct PsdSlicesHeader
	{
		public int Version;
		public int Top;
		public int Left;
		public int Bottom;
		public int Right;
		public int SliceCount;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String GroupName;
	}
	/// <summary>
	/// Specification: http://www.adobe.com/devnet-apps/photoshop/fileformatashtml/PhotoshopFileFormats.htm#50577409_19931
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	public struct PsdSlice {
		public int Id;
		public int GroupId;
		public int Origin;
		public int AssociatedLayerId;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String Name;
		public int Type;
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String URL;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String Target;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String Message;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String AltTag;
		public Boolean CellIsHTML;
		[MarshalAs(UnmanagedType.ByValTStr)]
		public String CellText;
		public int HorizontalAlignment;
		public int VerticalAlignment;
		public Byte A;
		public Byte R;
		public Byte G;
		public Byte B;
	}
}