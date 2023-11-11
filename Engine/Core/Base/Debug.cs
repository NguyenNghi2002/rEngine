#define ENGINE
using Engine.SceneManager;
using Raylib_cs;
using System.Text;

namespace Engine
{
    public static class Debugging
    {
        public enum LogLevel 
        {
            Debug = 0,
            Warning ,
            Error  ,
            Comment,
            System ,
        }


        private static StringBuilder _sb = new StringBuilder();


        /// <summary>
        /// Default true
        /// </summary>
        public static bool EnableConsoleLog = false;

        /// <summary>
        /// Default true
        /// </summary>
        public static bool EnableDrawDebug = false;


        public static void Log(string arg, params object?[]? args)
            => Log(arg,Debugging.LogLevel.Debug,args);
        public static void Log(string format, LogLevel logLevel = LogLevel.Debug, params object?[]? args)
        {
            if (EnableConsoleLog)
            {
                PrepareString(logLevel,ref _sb);
                _sb.Append(String.Format(format,args));
                Console.WriteLine(_sb.ToString());
#if ENGINE

                //Write in game debug
#endif


                Console.ResetColor();
                _sb.Clear();

            }
        }


        private static void PrepareString(LogLevel logLevel,ref StringBuilder sb)
        {
            sb.Append('[');
            switch (logLevel)
            {
                case LogLevel.System:
                    sb.Append("System");
                    Console.ForegroundColor = ConsoleColor.Green;


                    break;
                case LogLevel.Debug:
                    sb.Append("Debug");
                    Console.ForegroundColor = ConsoleColor.White;


                    break;
                case LogLevel.Warning:
                    sb.Append("Warning");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;


                    break;
                case LogLevel.Error:
                    sb.Append("Error");
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.DarkRed;


                    break;
                case LogLevel.Comment:
                    sb.Append("Comment");

                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    break;
            }
            sb.Append("] - ");
        }

#if ENGINE
        internal static void DrawEntityDebug(Scene scene)
        {
            if (!EnableDrawDebug) return;
            foreach (var entity in scene.SceneEntitiesList)
            {
                foreach (var component in entity.components.SolidComponents)
                {
                    component.OnDebugRender();
                }
            }
        }
#endif



    }
}