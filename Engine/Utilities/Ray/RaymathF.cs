using Raylib_cs;
using Raylib_cs.Extension;
using System.Diagnostics;
using System.Numerics;

namespace Engine
{
    public static partial class RaymathF
    {
        public const float PI = MathF.PI;
        public const float RADIAN2ANGLE = 180f / MathF.PI;
        public const float ANGLE2RADIAN = MathF.PI / 180f;

        public static float Lerp(float from, float to, float time)
            => from * (1 - time) + to * time;
		public static Vector2 Lerp(Vector2 from, Vector2 to, float time)
			=> new Vector2(
				Lerp(from.X,to.X,time),
				Lerp(from.Y,to.Y,time)
				);

        public static float InverseLerp(float a, float b, float value)
            => (value - a) / (b - a);
		public static Vector2 InverseLerp(Vector2 v0, Vector2 v1, Vector2 value)
			=> new Vector2(
				InverseLerp(v0.X, v1.X, value.X),
				InverseLerp(v0.Y, v1.Y, value.Y)
			);

        public static int Remap(int value, int iMin, int iMax, int oMin, int oMax)
			=> (int)Remap((float)value, (float)iMin, (float)iMax, (float)oMin, (float)oMax);
        public static float Remap(float value, float iMin, float iMax, float oMin, float oMax)
		{
			Insist.IsTrue(iMin != iMax && iMin != iMax,"min and max can't be same");
            return Lerp(oMin, oMax, InverseLerp(iMin, iMax, value));
		}
        public static float ClampedRemap(float value, float iMin, float iMax, float oMin, float oMax)
		{
			var res = Remap((float)value, (float)iMin, (float)iMax, (float)oMin, (float)oMax);
			return res.Clamp(oMin,oMax);
		}

		public static bool Within(float value, float min, float max)
			=> value >= min && value <= max;
		


        public static float ClampMax(this float value,float max)
            =>Math.Clamp(value, float.MinValue, max);  
        public static float ClampMin(this float value,float min)
            =>Math.Clamp(value, min, float.MaxValue);  
        public static float Clamp(this float value,float min,float max)
            =>Math.Clamp(value, min, max);

        public static Vector2 Half(this Vector2 value)
			=> new Vector2(value.X/2f, value.Y/2f);
        public static Vector2 HalfX(this Vector2 value)
			=> new Vector2(value.X/2f, value.Y);
		public static Vector2 HalfY(this Vector2 value)
			=> new Vector2(value.X, value.Y/2f);
			

		public static Vector2 SetX(this Vector2 vector2,float value)
			=> new Vector2(value, vector2.Y);	
		public static Vector2 SetY(this Vector2 vector2,float value)
			=> new Vector2(vector2.X, value);	

        public static bool LineCollideCircle(Vector2 p0,Vector2 p1,Vector2 center,float radius)
        {
            if (PointInCircle(p0, center, radius) ||
                PointInCircle(p1, center, radius)) return true;

            Vector2 closest =  Project(center - p0,p1-p0,out _) + p0;


            bool inside = closest.OnLine(p0,p1);
            if (PointInCircle(closest,center,radius)
                && inside)
                return true;

            return false;
        }

        public static bool OnLine(Vector2 v,Vector2 v1,Vector2 v2)
        {
            // A---C--------B
            var AB = Vector2.Distance(v1,v2);
            var AC = Vector2.Distance(v,v1);
            var CB = Vector2.Distance(v,v2);

            return AC + CB == AB;
        }

        public static bool Inside(Vector2 point,Vector2 min,Vector2 max)
            =>point.X>=min.X && point.X<=max.X &&
              point.Y>=min.Y && point.Y<=max.Y;
        public static Vector2 Nearest( Vector2 point,Vector2 v1,Vector2 v2)
        {
            var range1 = Vector2.Distance(point,v1);
            var range2 = Vector2.Distance(point,v2);

            return range1 < range2 ?v1:v2;
        }
        public static Vector2 Farest(Vector2 point, Vector2 v1, Vector2 v2)
        {
            var range1 = Vector2.Distance(point, v1);
            var range2 = Vector2.Distance(point, v2);

            return range1 > range2 ? v1 : v2;
        }
        public static bool PointInCircle(Vector2 point,Vector2 center,float radius)
            =>Vector2.Distance(point,center) <= radius;
		public static Vector2 ClampMaxLength(this Vector2 v,float maxLength)
			=>(v.Length() > maxLength)? v.Scale(maxLength) : v;
		
		public static Vector2 Remap(Vector2 value,Vector2 inV0,Vector2 inV1, Vector2 outV0, Vector2 outV1)
		{
			var x = Remap(value.X, inV0.X, inV1.X, outV0.X, outV1.X);
			var y = Remap(value.Y, inV0.Y, inV1.Y, outV0.Y, outV1.Y);
			return new Vector2(x, y);
		}
        public static Vector2 Project(Vector2 caster,Vector2 lineToProject,out bool insideLine)
        {
            var dist = lineToProject.Length();
            insideLine = true;
            // formular :( A . B ) / |B|^2
            var dot = Vector2.Dot(caster, lineToProject) / (dist * dist);
            if (dot < 0 || dot > dist )
                insideLine = false;
            return dot * lineToProject;
        }

        public static List<Vector2> SmoothLine(List<Vector2> input,float lerp = 0.25f)
        {
			var invert = 1 - lerp;

            //expected size
            var output = new List<Vector2>(input.Count);

            //first element
            output.Add(input[0]);

            //average elements
            for (int i = 0; i < input.Count - 1; i++)
            {
                Vector2 p0 = input[i];
                Vector2 p1 = input[i + 1];

                Vector2 Q = new Vector2(
                    invert * p0.X + lerp * p1.X,
                    invert * p0.Y + lerp * p1.Y);
                Vector2 R = new Vector2(
                    lerp * p0.X + invert * p1.X,
                    lerp * p0.Y + invert * p1.Y);
                output.Add(Q);
                output.Add(R);
            }

            //last element
            output.Add(input[input.Count - 1]);
            return output;
        }

        public static Vector2 Rot90CCW(Vector2 vector) => new Vector2(-vector.Y, vector.X);
        public static Vector2 Rot90CW(Vector2 vector) => new Vector2(vector.Y, -vector.X);

		public static Vector2 Vector2Rotate(Vector2 vector2, float angle)
			=> Vector2Rotate(Vector2.Zero,vector2,angle);
        public static Vector2 Vector2Rotate(Vector2 center,Vector2 point,float angle)
        {
			var rad = angle;

			var vector = point - center;
			
            point.X = vector.X * MathF.Cos(rad) - vector.Y * MathF.Sin(rad);
            point.Y = vector.X * MathF.Sin(rad) + vector.Y * MathF.Cos(rad);


			return point + center ;
        }
        public static float Vector2Angle(Vector2 vector)
            => Vector2Randian(vector) * 180f / MathF.PI;
        public static float Vector2Randian(Vector2 vector)
            =>MathF.Atan2(vector.Y,vector.X);
        public static Vector2 ClampPoint(Vector2 v2, Vector2 min, Vector2 max)
        {
            Vector2 result = v2;
            result.X = (result.X > max.X) ? max.X : result.X;
            result.X = (result.X < min.X) ? min.X : result.X;
            result.Y = (result.Y > max.Y) ? max.Y : result.Y;
            result.Y = (result.Y < min.Y) ? min.Y : result.Y;
            return result;
        }

		public static Rectangle GetBorder(Vector2[] points)
		{
			Vector2 min = points[0], max = points[0];
			for (int i = 1; i < points.Length; i++)
			{
				var p = points[i];

				if(p.X < min.X) min.X = p.X;
				if(p.X > max.X) max.X = p.X;
				if(p.Y < min.Y) min.Y = p.Y;
				if(p.Y > max.Y) max.Y = p.Y;
			}

			return RectangleExt.CreateRectanglePoint(min, max);
		}

		public static float SmoothDamp(float from, float to, float value)
		{
			return Lerp(from, to, 1 - MathF.Exp(-value));
		}
		public static Vector2 SmoothDamp(Vector2 from, Vector2 to, float value)
		{
			var invertExp = 1- MathF.Exp(-value);
			return new Vector2(
				Lerp(from.X, to.X, invertExp),
				Lerp(from.Y, to.Y, invertExp)
				);
		}
		public static Vector3 SmoothDamp(Vector3 from, Vector3 to, float value)
		{
			var invertExp = 1- MathF.Exp(-value);
			return new Vector3(
				Lerp(from.X, to.X, invertExp),
				Lerp(from.Y, to.Y, invertExp),
				Lerp(from.Z, to.Z, invertExp)
			);
		}

	}

	public static class RaymathExt
	{
		public static Vector2 Scale(this Vector2 v, float length)
		{
			return v.Length() != 0 ? Vector2.Normalize(v) * length : v;
		}
		public static Vector2 ProjectOn(this Vector2 caster, Vector2 lineToProject)
			=> RaymathF.Project(caster, lineToProject, out _);
		public static Vector2 Move(this Vector2 vector, float deltaXY)
			   => vector.Move(deltaXY, deltaXY);
		public static Vector2 Move(this Vector2 vector, float deltaX, float deltaY)
			=> new Vector2(vector.X + deltaX, vector.Y + deltaY);
		public static Vector2 Move(this Vector2 vector, Vector2 delta)
		{
			vector.X += delta.X;
			vector.Y += delta.Y;
			return vector;
		}
		public static Vector2 MoveX(this Vector2 vector, float deltaX)
			=> new Vector2(vector.X + deltaX, vector.Y);
		public static Vector2 MoveY(this Vector2 vector, float deltaY)
			=> new Vector2(vector.X, vector.Y + deltaY);

		public static Vector2 NegateX(this Vector2 vector)
			=> new Vector2(-vector.X, vector.Y);
		public static Vector2 NegateY(this Vector2 vector)
			=> new Vector2(vector.X, -vector.Y);
		public static Vector2 Negate(this Vector2 vector)
			=> new Vector2(-vector.X, -vector.Y);


		public static bool Within(this float value, float min, float max)
		=> value >= min && value <= max;

		public static bool Inside(this Vector2 v, Vector2 min, Vector2 max)
		   => v.X >= min.X && v.X <= max.X &&
			 v.Y >= min.Y && v.Y <= max.Y;
		public static bool OnLine(this Vector2 v, Vector2 v1, Vector2 v2)
		=> RaymathF.OnLine(v, v1, v2);
		public static Vector2 Normalize(this Vector2 vector) => Vector2.Normalize(vector);

		public static Rectangle GetBound(this Vector2[] points)
			=> RaymathF.GetBorder(points);
	}
}
