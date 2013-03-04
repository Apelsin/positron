using System;

namespace positron
{
	public class Vector<T> : IEquatable<Vector<T>> where T: IComparable
	{
		public T X, Y, Z;
		public Vector (T x, T y, T z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		public Vector (T x, T y)
		{
			X = x;
			Y = y;
		}
		public Vector ()
		{
		}
		/// <summary>
		/// Adds two instances.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector<T> operator +(Vector<T> left, Vector<T> right)
		{
			(dynamic)left.X += (dynamic)right.X;
			left.Y += right.Y;
			return left;
		}
		
		/// <summary>
		/// Subtracts two instances.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector<T> operator -(Vector<T> left, Vector<T> right)
		{
			left.X -= right.X;
			left.Y -= right.Y;
			left.Z -= right.Z;
			return left;
		}
		
		/// <summary>
		/// Negates an instance.
		/// </summary>
		/// <param name="vec">The instance.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector<T> operator -(Vector<T> vec)
		{
			vec.X = -vec.X;
			vec.Y = -vec.Y;
			vec.Z = -vec.Z;
			return vec;
		}
		
		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="vec">The instance.</param>
		/// <param name="f">The scalar.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector<T> operator *(Vector<T> vec, T f)
		{
			vec.X *= f;
			vec.Y *= f;
			vec.Z *= f;
			return vec;
		}
		
		/// <summary>
		/// Multiply an instance by a scalar.
		/// </summary>
		/// <param name="f">The scalar.</param>
		/// <param name="vec">The instance.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector<T> operator *(T f, Vector<T> vec)
		{
			vec.X *= f;
			vec.Y *= f;
			vec.Z *= f;
			return vec;
		}
		
		/// <summary>
		/// Divides an instance by a scalar.
		/// </summary>
		/// <param name="vec">The instance.</param>
		/// <param name="f">The scalar.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector<T> operator /(Vector<T> vec, T f)
		{
			double mult = 1.0 / f;
			vec.X *= mult;
			vec.Y *= mult;
			vec.Z *= mult;
			return vec;
		}
		
		/// <summary>
		/// Compares two instances for equality.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>True, if both instances are equal; false otherwise.</returns>
		public static bool operator ==(Vector<T> left, Vector<T> right)
		{
			return left.Equals(right);
		}
		
		/// <summary>
		/// Compares two instances for ienquality.
		/// </summary>
		/// <param name="left">The left instance.</param>
		/// <param name="right">The right instance.</param>
		/// <returns>True, if the instances are not equal; false otherwise.</returns>
		public static bool operator !=(Vector<T> left, Vector<T> right)
		{
			return !left.Equals(right);
		}
		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		}
		
		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Vector<T>))
				return false;
			
			return this.Equals((Vector<T>)obj);
		}

		/// <summary>Indicates whether the current vector is equal to another vector.</summary>
		/// <param name="other">A vector to compare with this vector.</param>
		/// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
		public bool Equals(Vector<T> other)
		{
			return
				X.CompareTo(other.X) == 0 &&
				Y.CompareTo(other.Y) == 0 &&
				Z.CompareTo(other.Z) == 0;
		}
	}
}

