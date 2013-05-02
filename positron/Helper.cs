using System;
using System.Collections.Generic;
using System.Drawing;

using OpenTK;

using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;


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
		/// <summary>
		///	Returns the index of the texture region matching a given label
		/// </summary>
		public static int Labeled(this TextureRegion[] regions, string label_seek, int no_match_index = -1)
		{
			for (int i = 0; i < regions.Length; i++)
				if(regions[i].Label == label_seek)
					return i;
			return no_match_index;
		}
		/// <summary>
		/// Returns the index of the texture region matching a given label; unoptimized multi-region lookup
		/// </summary>
		public static int[] Labeled(this TextureRegion[] regions, int no_match_index, params string[] labels_seek)
		{
			int[] region_indices = new int[labels_seek.Length];
			for(int i = 0; i < labels_seek.Length; i++)
				region_indices[i] = regions.Labeled(labels_seek[i], no_match_index);
			return region_indices;
		}
		/// <summary>
		/// Returns the index of the texture region matching a given label; unoptimized multi-region lookup
		/// </summary>
		public static int[] Labeled(this TextureRegion[] regions, params string[] labels_seek)
		{
			return regions.Labeled(-1, labels_seek);
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
//		public static Microsoft.Xna.Framework.Vector2 ContactNormalAbsolute(this Contact contact)
//		{
//			FarseerPhysics.Collision.Manifold manifold = new Manifold();
//			FixedArray2<Microsoft.Xna.Framework.Vector2> points = new FixedArray2<Microsoft.Xna.Framework.Vector2 >();
//			contact.GetWorldManifold(out manifold.LocalNormal, out points); // Oh God why
//			return manifold.LocalNormal;
//		}
		public static float ConactNormalError(this Contact contact, Microsoft.Xna.Framework.Vector2 v2)
		{
			return Microsoft.Xna.Framework.Vector2.Distance(contact.Manifold.LocalNormal, v2);
		}
        public static void GetEnclosingAABB (this Body body, out AABB aabb_enclosing)
        {
            AABB aabb_fixture;
            Microsoft.Xna.Framework.Vector2 lo = new Microsoft.Xna.Framework.Vector2(float.PositiveInfinity);
            Microsoft.Xna.Framework.Vector2 hi = new Microsoft.Xna.Framework.Vector2(float.NegativeInfinity);
            for (int i = 0; i < body.FixtureList.Count; i++) {
                if(body.FixtureList[i].CollisionCategories == Category.None)
                    continue;
                body.FixtureList[i].GetAABB (out aabb_fixture, 0); // Should just be first proxy (0)
                lo.X = Math.Min (lo.X, aabb_fixture.LowerBound.X);
                lo.Y = Math.Min (lo.Y, aabb_fixture.LowerBound.Y);
                hi.X = Math.Max (hi.X, aabb_fixture.UpperBound.X);
                hi.Y = Math.Max (hi.Y, aabb_fixture.UpperBound.Y);
            }

            aabb_enclosing = new AABB(lo, hi);
        }
	}
}

