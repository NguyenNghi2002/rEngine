using Raylib_cs;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Raylib_cs.Raylib;
using Keys = Raylib_cs.KeyboardKey;
namespace Engine
{
    public static class Input
    {
        public static Vector2 MousePosition => Core.Scene != null ? Core.Scene.GetMouseLocalPosition() : Raylib.GetMousePosition();

        public static bool LeftMouseButtonPressed => IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
        public static bool LeftMouseButtonReleased => IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
        public static bool RightMouseButtonPressed => IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT);

        public static bool IsKeyDown(Keys key) => Raylib.IsKeyDown(key);
        public static bool IsKeyPressed(Keys key) => Raylib.IsKeyPressed(key);
        public static bool IsKeyReleased(Keys key) => Raylib.IsKeyReleased(key);
        public static bool IsKeyUp(Keys key) => Raylib.IsKeyUp(key);

        public static bool RightMouseButtonReleased => IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT);

        public static float MouseWheelDelta => Raylib.GetMouseWheelMove();

        public static Keys[] CurrentKeysDown
        {
            get
            {
                var res = from k in Enum.GetValues<Keys>()
                          where IsKeyDown(k)
                          select k;
                return res.ToArray();
            }
        }
        public static Keys[] CurrentKeysPressed
        {
            get
            {
                List<Keys> keys = new List<Keys>();
                int key = Raylib.GetKeyPressed();
                while (key != 0)
                {
                    keys.Add((Keys)key);
                    //Console.WriteLine($"{(char)key} at {Raylib.GetTime()}");
                    key = Raylib.GetKeyPressed();

                }
                return keys.ToArray();
            }
        }
    }

    public struct KeyboardState
    {
        public bool CapsLock => Console.CapsLock;//(((ushort)User32.GetKeyState(0x14)) & 0xffff) != 0;
        public bool NumLock => Console.NumberLock;//(((ushort)User32.GetKeyState(0x90)) & 0xffff) != 0;
        //bool ScrollLock => (((ushort)User32.GetKeyState(0x91)) & 0xffff) != 0;


    }
}