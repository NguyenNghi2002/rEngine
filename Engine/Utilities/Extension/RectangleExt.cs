using Engine;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Raylib_cs.Extension
{
    public static class RectangleExt
    {


		/// <summary>
		/// Create rectangle utility
		/// </summary>
		/// <param name="position">Top left origin</param>
		/// <param name="scale">scale start from top-left origin</param>
		/// <returns>Ractangle</returns>
        public static Rectangle CreateRectanglePoint(Vector2 min, Vector2 max)
		{
			return CreateRectangle(min,max-min);
		}
        public static Rectangle CreateRectangle(Vector2 position, Vector2 scale)
            => new Rectangle(position.X,position.Y,scale.X,scale.Y);

        #region Move
        public static Rectangle MoveX(this Rectangle rec, float deltaX)
            => rec.Move(new Vector2(deltaX, 0));
        public static Rectangle MoveY(this Rectangle rec, float deltaY)
            => rec.Move(new Vector2(0, deltaY));
        public static Rectangle Move(this Rectangle rec, float deltaX, float deltaY)
        {
            rec.x += deltaX;
            rec.y += deltaY;
            return rec;
        }
        public static Rectangle Move(this Rectangle rec, Vector2 offset)
        {
            rec.x += offset.X;
            rec.y += offset.Y;
            return rec;
        }
        public static Rectangle MoveTo(this Rectangle rec, Vector2 position)
        {
            rec.x = position.X;
            rec.y = position.Y;
            return rec;
        } 
        #endregion


        public static Vector2 Scale(in this Rectangle rec)
            => new Vector2(rec.width, rec.height);
        public static Rectangle MuliplyScale(this Rectangle value,float scale)
        {
			value.width *= scale;
			value.height *= scale;
			return value;
		}
			
        public static Rectangle MuliplyScale(this Rectangle value,Vector2 scale)
        {
			value.width *= scale.X;
			value.height *= scale.Y;
			return value;
        }

		public static Rectangle NegateWidth(in this Rectangle rec)
			=> new Rectangle(rec.x, rec.y, -rec.width, rec.height);
		public static Rectangle NegateHeight(in this Rectangle rec)
			=> new Rectangle(rec.x, rec.y, rec.width, -rec.height);

		#region Rectangle Vertex

		//---------------------------------------------------------------------------
		//			Vertice position setter
		//---------------------------------------------------------------------------
		/// <summary>
		/// Move vertex topleft from rectangle
		/// </summary>
		/// <param name="rectangle">source rectangle</param>
		/// <param name="point">point to expand top left</param>
		/// <returns></returns>
		public static Rectangle SetTopRight(this Rectangle rec, Vector2 point)
			=> rec.SetRight(point.X).SetTop(point.Y);
		public static Rectangle SetTopLeft(this Rectangle rec,Vector2 point)
			=> rec.SetLeft(point.X).SetTop(point.Y);
		public static Rectangle SetBotRight(this Rectangle rec,Vector2 point)
			=> rec.SetRight(point.X).SetBot(point.Y);
		public static Rectangle SetBotLeft(this Rectangle rec,Vector2 point)
			=> rec.SetLeft(point.X).SetBot(point.Y);


		public static Rectangle SetTop(this Rectangle rec, float value)
		{
			var moveError = value - rec.y;
			rec.y = value;
			rec.height -= moveError;
			return rec;
		}
		public static Rectangle SetLeft(this Rectangle rec,float value)
        {
			var moveError = value - rec.x;
			rec.x = value;
			rec.width -= moveError;
			return rec;
		}
		public static Rectangle SetBot(this Rectangle rec,float value)
        {
			var moveError = value - (rec.y + rec.height);
			rec.height += moveError;
			return rec;
        }
		public static Rectangle SetRight(this Rectangle rec, float value)
		{
			var moveError = value - (rec.x + rec.width);
			rec.width += moveError;
			return rec;
		}

		/// <summary>
		/// calculates the union of the two Rectangles. The result will be a rectangle that encompasses the other two.
		/// </summary>
		public static void Union(this Rectangle rec, Rectangle otherRectangle, out Rectangle result)
		{
			result.x = Math.Min(rec.x, otherRectangle.x);
			result.y = Math.Min(rec.y, otherRectangle.y);

			var br1 = rec.BotRight();
			var br2 = rec.BotRight();

			result.width = Math.Max(br1.X, br2.X) - result.x;
			result.height = Math.Max(br1.Y, br2.Y) - result.y;
		}

		/// <summary>
		/// Update first to be the union of first and point
		/// </summary>
		/// <param name="rec">First.</param>
		/// <param name="point">Point.</param>
		/// <param name="result">Result.</param>
		public static void Union(this Rectangle rec, Vector2 point, out Rectangle result)
		{
			var rect = new Rectangle(point.X, point.Y, 0, 0);
			Union(rec, rect, out result);
		}

		//---------------------------------------------------------------------------
		//			Vertice position getter
		//---------------------------------------------------------------------------
		public static Vector2 TopLeft(this Rectangle rec)
            => new Vector2(rec.x, rec.y);
        public static Vector2 TopRight(this Rectangle rec)
            => new Vector2(rec.x + rec.width, rec.y);
        public static Vector2 BotLeft(this Rectangle rec)
            => new Vector2(rec.x, rec.y + rec.height);
        public static Vector2 BotRight(this Rectangle rec)
            => new Vector2(rec.x + rec.width, rec.y + rec.height);
		public static Vector2 Center(this Rectangle rec)
			=>new Vector2(rec.x + rec.width / 2f, rec.y + rec.height/2f);

		public static bool IsQualify(Rectangle rec) => rec.width != 0 && rec.height != 0;
		public static bool IsQualify(in this Rectangle rec) => IsQualify(rec);

		public static Vector2 GetCoord01(Rectangle rec,Vector2 point)
		{
			Insist.IsTrue(rec.IsQualify(), " invalid rectangle");
			var min = rec.TopLeft();
			var max = rec.BotRight();
			return RaymathF.InverseLerp(min,max,point);
		}
		#endregion

		public static Rectangle FromFloats(float x, float y, float w, float h)
			=> new Rectangle(x,y,w,h);
		public static bool Contains(this Rectangle rectangle, Vector2 point)
			=> Raylib.CheckCollisionPointRec(point, rectangle);


	}
}
