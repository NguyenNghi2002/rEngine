using Engine.UI;
using Raylib_cs;
using Keys = Raylib_cs.KeyboardKey;

namespace Engine
{
	public static class InputUtils
	{
		public static bool IsMac;
		public static bool IsWindows;
		public static bool IsLinux;


		static InputUtils()
		{
			IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
			IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
			IsMac = Environment.OSVersion.Platform == PlatformID.MacOSX;
		}


		public static bool IsShiftDown()
		{
			return Input.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) || Input.IsKeyDown(Keys.KEY_RIGHT_SHIFT);
		}


		public static bool IsAltDown()
		{
			return Input.IsKeyDown(Keys.KEY_LEFT_ALT) || Input.IsKeyDown(Keys.KEY_RIGHT_ALT);
		}


		public static bool IsControlDown()
		{
			if (IsMac)
				return Input.IsKeyDown(Keys.KEY_LEFT_SUPER) || Input.IsKeyDown(Keys.KEY_RIGHT_SUPER);

			return Input.IsKeyDown(Keys.KEY_LEFT_CONTROL) || Input.IsKeyDown(Keys.KEY_RIGHT_CONTROL);
		}
		public static bool Caplock
        {
            get
            {
				return Console.CapsLock;
            }
        }
	}
}