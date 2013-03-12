using System;

namespace positron
{
	public static class MathUtil
	{
		public static T Clamp<T>(T x, T hi, T lo) where T : System.IComparable<T> {
			return x.CompareTo(hi) > 0 ? hi : x.CompareTo(lo) < 0 ? lo: x;
		}
	}
}

