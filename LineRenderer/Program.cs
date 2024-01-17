
using Raylib_cs;
using Clipper2Lib;
using System.Numerics;
using Engine;
using LineRenderer;

public class Program
{
    static void Main(string[] args)
    {

#if   true
        inflateLine();
#else
        Raylib.InitWindow(1280,720,"line renderer");

        Texture2D uvmap = Raylib.LoadTexture("uv.png");
        Shader shader = Raylib.LoadShader(null, "wave.fs");
        var line2d = new Line2D();
        while (!Raylib.WindowShouldClose())
        {

            var mouse = Raylib.GetMousePosition();
            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                line2d.AddPoint(mouse);
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
                line2d.Clear();
            line2d.LineWidth += Raylib.GetMouseWheelMove();
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLANK);


            line2d.DrawLineDebug();
            /*
            RayUtils.DrawTextureDynamicPro(uvmap,new Vector2(0,1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0),
                new Vector2(0, 500), Raylib.GetMousePosition()- new Vector2(200,200), new Vector2(500, 0), new Vector2(0,0) , Color.WHITE,10);
            */


            Raylib.EndDrawing();
        } 
#endif


    }

    static void inflateLine()
    {
        Raylib.InitWindow(1280, 720, "line renderer");
        var paths = new Paths64();
        float offset = 0;

        while (!Raylib.WindowShouldClose())
        {
            var mouse = Raylib.GetMousePosition();
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_SPACE))
                paths.Add(new());
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_BACKSPACE))
                paths.Clear();

            if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
            {
                if (paths.Count == 0)
                    paths.Add(new());

                paths.Last().Add(new((long)mouse.X, (long)mouse.Y));
            }
            offset += Raylib.GetMouseWheelMove();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLANK);
            var originPaths = paths.Select((path) => path.Select((p) => new Vector2(p.X, p.Y)).ToList()).ToList();
            var a = Clipper.InflatePaths(paths, offset, JoinType.Bevel, EndType.Round);
            var inflatedPaths = Clipper.SimplifyPaths(a, 0.025, false).Select((path) => path.Select((p) => new Vector2(p.X, p.Y)).ToList()).ToList();
            DrawPaths(originPaths);
            DrawPaths(inflatedPaths);

            Raylib.EndDrawing();
        }


    }
    static void DrawPaths(List<List<Vector2>> paths)
    {
        foreach (List<Vector2> path in paths)
        {
            if(path.Count >= 2)
            {
                Raylib.DrawLineStrip(path.Select((intp) => new Vector2(intp.X, intp.Y)).ToArray(), path.Count, Color.GREEN);
                Raylib.DrawLineV(path.Last(),path.First(),Color.GREEN);
            }
            foreach (Vector2 p in path)
            {
                Raylib.DrawCircle((int)p.X, (int)p.Y, 4, Color.WHITE);
                Raylib.DrawText($"{paths.IndexOf(path)}", (int)p.X, (int)p.Y, 5, Color.RED);
            }
        }
    }
}
