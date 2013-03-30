using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK;

namespace positron
{
	public static class Helper
	{
		public static bool KeyPressedInTime (DateTime pressed, DateTime now)
		{
			return (now - pressed).TotalSeconds < Configuration.KeyPressTimeTolerance;
		}
        public static Color Blend(this Color S, Color C, float alpha)
        {
            return Color.FromArgb(
                S.A + (byte)((C.A - S.A) * alpha),
                S.R + (byte)((C.R - S.R) * alpha),
                S.G + (byte)((C.G - S.G) * alpha),
                S.B + (byte)((C.B - S.B) * alpha));
        }
        public static void BuildTiledRegions(this TextureRegion[] regions, int count_x, double w, double h)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                var low = new Vector2d(w * (i % count_x), h * (i / count_x));
                var high = new Vector2d(low.X + w, low.Y + h);
                regions[i] = new TextureRegion(low, high);
                if (i == regions.Length)
                    break;
            }
        }
	}
}

