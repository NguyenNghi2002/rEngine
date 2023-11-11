using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Engine
{
	public struct VectorInt2 : IEquatable<Vector2>
	{
		public int X, Y;
		public static VectorInt2 Zero=> new VectorInt2(0);
		public static VectorInt2 One=> new VectorInt2(1);
		public static VectorInt2 UnitX=> new VectorInt2(1,0);
		public static VectorInt2 UnitY=> new VectorInt2(0,1);
		public VectorInt2(int x, int y)
		{
			this.X = x;
			this.Y = y;
		}
		public VectorInt2(int val)
		{
			this.X = val;
			this.Y = val;
		}

		public static VectorInt2 operator -(VectorInt2 self)
			=> new VectorInt2(-self.X, -self.Y);
		public static VectorInt2 operator +(VectorInt2 lhs, VectorInt2 rhs)
			=> new VectorInt2(lhs.X + rhs.X, lhs.Y + rhs.Y);
		public static VectorInt2 operator -(VectorInt2 lhs, VectorInt2 rhs)
			=> new VectorInt2(lhs.X - rhs.X, lhs.Y - rhs.Y);
		public static VectorInt2 operator *(VectorInt2 lhs, VectorInt2 rhs)
			=> new VectorInt2(lhs.X * rhs.X, lhs.Y * rhs.Y);
		public static VectorInt2 operator /(VectorInt2 lhs, VectorInt2 rhs)
		{
			if (rhs.Length() == 0)
			{
				throw new DivideByZeroException();
			}

			return new VectorInt2(lhs.X / rhs.X, lhs.Y / rhs.Y);
		}
		public static bool operator ==(VectorInt2 lhs, VectorInt2 rhs)
			=> (lhs.X == rhs.X) && (lhs.Y == rhs.Y);
		public static bool operator !=(VectorInt2 lhs, VectorInt2 rhs)
			=> (lhs.X != rhs.X) && (lhs.Y != rhs.Y);

		public static VectorInt2 operator +(VectorInt2 lhs, int val)
			=> new VectorInt2(lhs.X + val, lhs.Y + val);
		public static VectorInt2 operator -(VectorInt2 lhs, int val)
			=> new VectorInt2(lhs.X - val, lhs.Y - val);
		public static VectorInt2 operator *(VectorInt2 lhs, int val)
			=> new VectorInt2(lhs.X * val, lhs.Y * val);
		public static VectorInt2 operator /(VectorInt2 lhs, int val)
        {
			if (val == 0) throw new DivideByZeroException();
			return new VectorInt2(lhs.X / val, lhs.Y / val);
        }


		public float Length()
			=> (float)Math.Sqrt(X * X + Y * Y);

		public static float Dot(VectorInt2 lhs, VectorInt2 rhs)
			=> lhs.X * rhs.X + lhs.Y * rhs.Y;
		public static Vector2 ToVector2(VectorInt2 vectorInt2)
			=> vectorInt2.ToVector2();
		public Vector2 ToVector2()
			=> new Vector2(X, Y);

		public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
		public override bool Equals([NotNullWhen(true)] object? obj)
			=>
			(obj != null || !this.GetType().Equals(obj.GetType()))
			&&
				this.X == ((VectorInt2)obj).X
			&&
				this.Y == ((VectorInt2)obj).Y
			;
		public override string ToString() => String.Format($"<{X},{Y}>");
	}
}
