using Raylib_cs;

namespace Engine
{
    public static class Time
    {
        public static float TimeScale { get; set; } = 1f;

        public static float DeltaTime => UnscaledDeltaTime * TimeScale;
        public static float UnscaledDeltaTime => Raylib.GetFrameTime();
        public static float TotalTime => (float)Raylib.GetTime();

        public static float RawFixedDeltaTime { get; set; } = 1f/60;
        public static float FixedDeltaTime => TimeScale * RawFixedDeltaTime;
    }
}
