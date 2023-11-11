using Raylib_cs;
using System.Numerics;

namespace Engine.Utilities
{
	public static class Camera2DExt
	{
		public static Vector2 WorldToScreenPoint(this Camera2D camera, Vector2 point)
			=> Raylib.GetWorldToScreen2D(point, camera);
		public static Vector2 ScreenToWorldPoint(this Camera2D camera, Vector2 point)
			=> Raylib.GetScreenToWorld2D(point, camera);
	}
}
