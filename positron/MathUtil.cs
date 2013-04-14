using System;

namespace positron
{
	public static class MathUtil
	{
		public static T Clamp<T>(T x, T hi, T lo) where T : System.IComparable<T> {
			return x.CompareTo(hi) > 0 ? hi : x.CompareTo(lo) < 0 ? lo: x;
		}
		public static double Trapz(double x, double width, double offset)
		{
			return MathUtil.Clamp(3.0 * width - Math.Abs(2.0 * x - offset), 2.0 * width, 0.0) * 0.5;
		}
		public static double Trapz(double x) // width: 1, offset: 3, period == 4
		{
			return MathUtil.Clamp(3.0 - Math.Abs(2.0 * x - 3.0), 2.0, 0.0) * 0.5;
		}
		public static int Trapz(int x, int width, int offset)
		{
			int two_width = 2 * width;
			return MathUtil.Clamp(3 * width - Math.Abs(2 * x - offset), 2 * width, 0) / 2;
		}
	}
}

