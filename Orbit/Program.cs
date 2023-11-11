using Engine;
using Raylib_cs;
using System.Numerics;

new GameCore().Run();

public class GameCore : Engine.Core
{
    public const int MAX_FPS = 60;

    VectorInt2 Samsungs22PlusRes = new VectorInt2(1080, 2340);
    VectorInt2 Window = new VectorInt2(1920/2, 1080/2);
    public override void Initialize()
    {
        //                                 /*  WINDOW CONFIGURATION */
        ///--------------------------------------------------------------------------------///
        var windowHints = ConfigFlags.FLAG_WINDOW_RESIZABLE;
        Raylib.SetConfigFlags(windowHints);

        Raylib.SetTargetFPS(MAX_FPS);

        var currDecive = Window;

        WindowWidth = currDecive.Y;
        WindowHeight = currDecive.X;
        

        //                               /*  START WINDOW */
        ///---------------------------------------------------------------------------------///

        base.Initialize();


        //                                 /*  CREATE SCENE */
        ///----------------------------------------------------------------------------------///
        var ratio = 1280f / 720f;
        Scene = new SampleScene("Sample", 720, 1280);
        //Scene.Scaling = Engine.SceneManager.DesignScaling.Truncate;


        //                                  /*  GLOBAL MANAGER */
        ///----------------------------------------------------------------------------------///
        Managers.Add(new ImguiEntityManager());
    }
}
