using Raylib_cs;
using System.Text;
using System.Numerics;
using System.Net.NetworkInformation;
using Engine.UI;
using System.Collections.ObjectModel;
using System.Globalization;
using Raylib_cs.Extension;
using System;
using System.Diagnostics;

namespace Engine
{
    /// <summary>
	/// Ultilities for <see cref="Raylib"/>
	/// </summary>
    public static  class RayUtils
    {
		public static Color GetColor(string hexCode)
        {
            byte[] c  = Convert.FromHexString(hexCode);
			return new Color(c[0], c[1], c[2], c.Length > 3 ? c[3] : 255);
        }
		public static NPatchInfo CreateNpatch(Texture2D texture,int top,int bottom,int left,int right,NPatchLayout layout)
			 => new NPatchInfo()
			 {
				 source = texture.Source(),
				 top = top,
				 bottom = bottom,
				 left = left,
				 right = right,
				 layout = layout
			 };
		public static NPatchInfo CreateNPatchInfoPadded(Texture2D texture, int padding, NPatchLayout layout)
			=> CreateNpatch(texture,padding,padding,padding,padding,layout);

		/// <summary>
		/// Set To Directory, If directory doesn't exist, then it will create 
		/// </summary>
		/// <param name="directoryName">folder name</param>
		public static void EnsureDirectory(string directoryName)
        {
            if (!Directory.Exists(@$"./{directoryName}")) Directory.CreateDirectory(directoryName);
            Directory.SetCurrentDirectory(directoryName);
        }

		/// <summary>
		/// Open URL with default system browser (if available)
		/// </summary>
        public static void OpenURL(string url)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(url);

            unsafe
            {
                fixed (byte* p = bytes)
                {
                    sbyte* sp = (sbyte*)p;
                    Raylib_cs.Raylib.OpenURL(sp);
                }
            }
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="points"></param>
		/// <param name="angle">in Degree, of course</param>
		public static IEnumerable<Vector2> RotatePoints(IEnumerable<Vector2> points,Vector2 center,float angle)
        {
			var rotMax = Matrix3x2.CreateRotation(angle * Raylib.DEG2RAD, center);
			var transformedPoints = points.Select((p) =>Vector2.Transform(p,rotMax));
			return transformedPoints;
        }

		/// <summary>
		/// Draw strech-able texture.
		/// Position relative to bottom left as zero zero for the source and top left for screen coordinate.
		/// </summary>
		/// <param name="texture">Texture2D</param>
		/// <param name="src1">Source top left</param>
		/// <param name="src2">Source top right</param>
		/// <param name="src3">Source bottom right</param>
		/// <param name="src4">Source bottom left</param>
		/// <param name="dest1">bottom left</param>
		/// <param name="dest2">bottom right</param>
		/// <param name="dest3">top right</param>
		/// <param name="dest4">top left</param>
		/// <param name="tintColor">tint Color</param>
		/// <param name="quadCount">quad subdivide count</param>
		public static void DrawTextureDynamicPro(Texture2D texture,
			Vector2 src1, Vector2 src2, Vector2 src3, Vector2 src4,
			Vector2 dest1, Vector2 dest2, Vector2 dest3, Vector2 dest4,
			Color tintColor, int quadCount)
		{
			Rlgl.rlCheckRenderBatchLimit(quadCount * 4);
			Rlgl.rlSetTexture(texture.id);

			Rlgl.rlBegin(DrawMode.QUADS);
			{
				//Set quad transform facing along z coord
				Rlgl.rlNormal3f(0f, 0f, 1f);
				Rlgl.rlColor4ub(tintColor.r, tintColor.g, tintColor.b, tintColor.a);

				
				float quadLerp = 1f / quadCount;
				for (int y = 0; y < quadCount; y++)
				{
					var lerpY = y * quadLerp;
					var nextlerpY = (y + 1) * quadLerp;

					//**************
					// Texture coords
					// ys = y coord source
					//**************
					Vector2 ysLeft = Vector2.Lerp(src4, src1, lerpY);
					Vector2 ysRight = Vector2.Lerp(src3, src2, lerpY);

					var ysNextLeft = Vector2.Lerp(src4, src1, nextlerpY);
					var ysNextRight = Vector2.Lerp(src3, src2, nextlerpY);

					//**************
					// Destination coords
					// yd = y coord destination
					//**************
					#region Destination

					Vector2 ydLeft = Vector2.Lerp(dest4, dest1, lerpY);
					Vector2 ydRight = Vector2.Lerp(dest3, dest2, lerpY);

					Vector2 ydNextLeft = Vector2.Lerp(dest4, dest1, nextlerpY);
					Vector2 ydNextRight = Vector2.Lerp(dest3, dest2, nextlerpY);
					#endregion

					for (int x = 0; x < quadCount; x++)
					{
						var lerpX = x * quadLerp;
						var nextLerpX = (x + 1) * quadLerp;

						var ps4 = Vector2.Lerp(ysLeft, ysRight, lerpX);
						var ps3 = Vector2.Lerp(ysLeft, ysRight, nextLerpX);
						var ps2 = Vector2.Lerp(ysNextLeft, ysNextRight, nextLerpX);
						var ps1 = Vector2.Lerp(ysNextLeft, ysNextRight, lerpX);

						var pd4 = Vector2.Lerp(ydLeft, ydRight, lerpX);
						var pd3 = Vector2.Lerp(ydLeft, ydRight, nextLerpX);
						var pd2 = Vector2.Lerp(ydNextLeft, ydNextRight, nextLerpX);
						var pd1 = Vector2.Lerp(ydNextLeft, ydNextRight, lerpX);


						///    source coord
						///   1 *-------------* 2
						///		| \           |
						///		|   \         |
						///		|     \.      |
						///		|        \    |
						///		|			\ |	
						///   4 *-------------* 3

						///    destination coord
						///   4 *-------------* 3
						///		|             |
						///		|             |
						///		|             |
						///		|             |
						///		|			  |	
						///   1 *-------------* 2


						//Bottom left( point 1)
						Rlgl.rlTexCoord2f(ps1.X, ps1.Y);
						Rlgl.rlVertex2f(pd1.X, pd1.Y);

						//Bottom right( point 2)
						Rlgl.rlTexCoord2f(ps2.X, ps2.Y);
						Rlgl.rlVertex2f(pd2.X, pd2.Y);

						//Top right( point 3)
						Rlgl.rlTexCoord2f(ps3.X, ps3.Y);
						Rlgl.rlVertex2f(pd3.X, pd3.Y);

						//Top left( point 4)
						Rlgl.rlTexCoord2f(ps4.X, ps4.Y);
						Rlgl.rlVertex2f(pd4.X, pd4.Y);


						Rlgl.rlSetTexture(0);

					}//Ent of loop x

				}//end of loop y

			}Rlgl.rlEnd();
		}
		public static void DrawRectangleLines(Vector2 position, Vector2 scale, Vector2 org, float rotation, float lineWidth, Color color)
			=> DrawRectangleLines(new Rectangle(position.X,position.Y,scale.X,scale.Y),org,rotation,lineWidth,color);
		public static void DrawRectangleLines(Rectangle rectangle, Vector2 org,float rotation,float lineWidth,Color color)
		{
			var pos = rectangle.TopLeft();
			rectangle = rectangle.Move(-org);
			rotation = rotation * Raylib.DEG2RAD;

			var v0 = rectangle.TopLeft();
			var v1 = rectangle.TopRight();
			var v2 = rectangle.BotRight();
			var v3 = rectangle.BotLeft();

			v0 = RaymathF.Vector2Rotate(pos ,v0,rotation);
			v1 = RaymathF.Vector2Rotate(pos ,v1,rotation);
			v2 = RaymathF.Vector2Rotate(pos ,v2,rotation);
			v3 = RaymathF.Vector2Rotate(pos ,v3,rotation);

			Raylib.DrawLineEx(v0,v1,lineWidth,color);
			Raylib.DrawLineEx(v1,v2,lineWidth,color);
			Raylib.DrawLineEx(v2,v3,lineWidth,color);
			Raylib.DrawLineEx(v3,v0,lineWidth,color);
		}
		public static void DrawCircleLines(Vector2 center,float radius,float lineWidth,Color color)
        {
			Raylib.DrawRing(center,radius - lineWidth,radius,0,360,100,color);
        }
		public static void DrawEllipse(Vector2 center,Vector2 scale,Vector2 origin,float rotation,Color color)
        {
			Rlgl.rlPushMatrix();

			Rlgl.rlTranslatef(center.X + origin.X,center.Y + origin.Y,0f);
			Rlgl.rlScalef(scale.X,scale.Y,1.0f);
			Rlgl.rlRotatef(rotation * Raylib.DEG2RAD,0f,0f,1f);

			Raylib.DrawCircleV(Vector2.Zero,1f,color);

			Rlgl.rlPopMatrix();
        }
		public static void DrawEllipseLines(Vector2 center, Vector2 scale, Vector2 origin, float rotation,float lineWidth, Color color)
		{
			Rlgl.rlPushMatrix();

			Rlgl.rlTranslatef(center.X + origin.X, center.Y + origin.Y, 0f);
			Rlgl.rlRotatef(rotation * Raylib.DEG2RAD, 0f, 0f, 1f);
			Rlgl.rlScalef(scale.X, scale.Y, 1.0f);

			var temp = Rlgl.rlGetLineWidth();
			Rlgl.rlSetLineWidth(lineWidth);

			Raylib.DrawCircleLines(0,0,1f,color);


			Rlgl.rlPopMatrix();
		}
		public static void DrawLineStrip(IEnumerable<Vector2> points,Vector2 origin,float angle,float lineWidth,Color color)
        {
			var temp = Rlgl.rlGetLineWidth();
			Rlgl.rlSetLineWidth(lineWidth);

			var transformedPoints = RotatePoints(points,Vector2.Zero, angle );

			Rlgl.rlPushMatrix();
			Rlgl.rlTranslatef(origin.X,origin.Y,0f);
			Raylib.DrawLineStrip(transformedPoints.ToArray(),transformedPoints.Count(),color);
			Rlgl.rlPopMatrix();

        }
		public static char CharToShiftedUSSymbol(char c)
        {
			return c switch
            {
                '1' => '!',
                '2' => '@',
                '3' => '#',
                '4' => '$',
                '5' => '%',
                '6' => '^',
                '7' => '&',
                '8' => '*',
                '9' => '(',
                '0' => ')',
                '-' => '_',
                '=' => '+',
                '[' => '{',
                ']' => '}',
                '\\' => '|',
                ';' => ':',
                '\'' => '"',
                ',' => '<',
                '.' => '>',
                '/' => '?',
                _ => c,
            };
        }


		public static Vector2 RandomPointInCircle(Vector2 center, float radius)
			=> RandomPointInRing(center,0,radius);
		public static Vector2 RandomPointInRing(Vector2 center, float minRadius, float maxRadius)
			=>RandomPointInFan(center,minRadius,maxRadius,0f,MathF.PI * 2);
		public static Vector2  RandomPointInFan(Vector2 center, float minRadius,float maxRadius,float minRadian,float maxRadian)
		{
			Debug.Assert(minRadius >= 0);
			Debug.Assert(maxRadius >= 0);

			// Generate a random angle in radians
			float randomAngle = RandF(minRadian, maxRadian);

			// Generate a random radius within the circle's radius
			float randomRadius = RandF(minRadius, maxRadius);

			// Calculate the x and y coordinates based on polar coordinates
			float x = center.X + randomRadius * MathF.Cos(randomAngle);
			float y = center.Y + randomRadius * MathF.Sin(randomAngle);

			return new Vector2(x, y);
		}

		public static float RandF(float min, float max)
		=> Random.Shared.NextSingle() * (max - min) + min;
		public static Vector2 RandF2(float minX, float maxX, float minY, float maxY)
			=> new Vector2(RandF(minX, maxX), RandF(minY, maxY));
		public static Vector2 RandF2(Vector2 min, Vector2 max)
			=> new Vector2(RandF(min.X, max.X), RandF(min.Y, max.Y));
	}
}
