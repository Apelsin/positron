using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;

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
            }
        }
		public static IWorldObject GetWorldObject(this Body body)
		{
			return (IWorldObject)body.UserData;
		}
		public static double SmootherStep(double edge0, double edge1, double x)
		{
			x = MathUtil.Clamp(((x - edge0)/(edge1 - edge0)), 1.0, 0.0);
			return x*x*x*(x*(x*6 - 15) + 10);
		}
		public static IEnumerable<Type> FindAllEndClasses(this Type self)
		{
			foreach(var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				var types = asm.GetTypes();
				foreach (var type in types)
				{
					if(type.BaseType == self)
					{
						int i = 0;
						IEnumerable<Type> recurse = FindAllEndClasses (type);
						foreach(Type t in recurse)
							i++;
						if(i == 0)
							yield return type;
						else
						{
							foreach(Type t in recurse)
								yield return t;
						}
					}
				}
			}
		}
	}
}

