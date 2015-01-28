using System;

namespace positron
{
	public static class MathUtil
	{
		public static T Clamp<T>(T x, T hi, T lo) where T : System.IComparable<T> {
			return x.CompareTo(hi) > 0 ? hi : x.CompareTo(lo) < 0 ? lo: x;
		}
		public static float Trapz(float x, float width, float offset)
		{
			return MathUtil.Clamp(3.0f * width - Math.Abs(2.0f * x - offset), 2.0f * width, 0.0f) * 0.5f;
		}
		public static float Trapz(float x) // width: 1, offset: 3, period == 4
		{
			return MathUtil.Clamp(3.0f - Math.Abs(2.0f * x - 3.0f), 2.0f, 0.0f) * 0.5f;
		}
		public static int Trapz(int x, int width, int offset)
		{
			int two_width = 2 * width;
			return MathUtil.Clamp(3 * width - Math.Abs(2 * x - offset), 2 * width, 0) / 2;
		}
	}
}

