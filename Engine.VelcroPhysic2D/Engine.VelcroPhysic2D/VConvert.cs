using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using mVector2 = Microsoft.Xna.Framework.Vector2;
using mVector3 = Microsoft.Xna.Framework.Vector3;

namespace Engine.Velcro.Unit
{
	/// <summary>
	/// Convert units between display and simulation units
	/// </summary>
	public static class VConvert
	{
		/// <summary>
		/// converts simulation (meters) to display (pixels)
		/// </summary>
		public static float SimToDisplay = 100f;

		/// <summary>
		/// converts display (pixels) to simulation (meters)
		/// </summary>
		public static float DisplayToSim = 1 / SimToDisplay;

		public static void SetDisplayUnitToSimUnitRatio(float displayUnitsPerSimUnit)
		{
			SimToDisplay = displayUnitsPerSimUnit;
			DisplayToSim = 1 / displayUnitsPerSimUnit;
		}

		public static float ToDisplayUnits(float simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static float ToDisplayUnits(int simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static mVector2 ToDisplayUnits(mVector2 simUnits)
			=> simUnits * SimToDisplay;
		public static Vector2 ToDisplayUnits(Vector2 simUnits)
		{
			return simUnits * SimToDisplay;
		}

#if false
		public static void ToDisplayUnits(ref Vector2 simUnits, out Vector2 displayUnits)
		{
			//displayUnits =  Vector2.Multiply(ref simUnits, SimToDisplay, out displayUnits);
		} 
#endif

		public static mVector3 ToDisplayUnits(mVector3 simUnits)
			=>simUnits * SimToDisplay;
		public static Vector3 ToDisplayUnits(Vector3 simUnits)
		{
			return simUnits * SimToDisplay;
		}

		public static Vector2 ToDisplayUnits(float x, float y)
		{
			return new Vector2(x, y) * SimToDisplay;
		}

		public static void ToDisplayUnits(float x, float y, out Vector2 displayUnits)
		{
			displayUnits = Vector2.Zero;
			displayUnits.X = x * SimToDisplay;
			displayUnits.Y = y * SimToDisplay;
		}

		public static float ToSimUnits(float displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static float ToSimUnits(double displayUnits)
		{
			return (float)displayUnits * DisplayToSim;
		}

		public static float ToSimUnits(int displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static mVector2 ToSimUnits(mVector2 displayUnits)
			=>displayUnits * DisplayToSim;
		public static Vector2 ToSimUnits(Vector2 displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

		public static mVector3 ToSimUnits(mVector3 displayUnits)
			=> displayUnits * DisplayToSim;
		public static Vector3 ToSimUnits(Vector3 displayUnits)
		{
			return displayUnits * DisplayToSim;
		}

#if false
		public static void ToSimUnits(ref Vector2 displayUnits, out Vector2 simUnits)
		{
			Vector2.Multiply(ref displayUnits, DisplayToSim, out simUnits);
		}

#endif
		public static Vector2 ToSimUnits(float x, float y)
		{
			return new Vector2(x, y) * DisplayToSim;
		}

		public static Vector2 ToSimUnits(double x, double y)
		{
			return new Vector2((float)x, (float)y) * DisplayToSim;
		}

		public static void ToSimUnits(float x, float y, out Vector2 simUnits)
		{
			simUnits = Vector2.Zero;
			simUnits.X = x * DisplayToSim;
			simUnits.Y = y * DisplayToSim;
		}
	}
}
