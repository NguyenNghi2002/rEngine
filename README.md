# rEngine
Custom C# Engine built on top of Raylib

Raylib Github: https://github.com/raysan5/raylib  <br/>
Raylib Website: https://www.raylib.com

## Example 
- Create an Application Core class inherited from ```Engine.Core``` class
```csharp
public class GameCore : Engine.Core
{
    public const int MAX_FPS = 60; // Set cap fps to 60

    VectorInt2 Samsungs22PlusRes = new VectorInt2(1080, 2340); // samsung22S resolution
    VectorInt2 Window = new VectorInt2(1920/2, 1080/2); // window resolution

    
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

        base.Intitialize();

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
```
- Creating Scene <br/>
An example from Planet Lander project
``` csharp
internal class SampleScene : Scene
{
    public SampleScene(string sceneName, int width, int height)
        : base(sceneName, width, height, Raylib.GetColor(0x001C31FF), Color.BLACK)
    {
    }
    //                                 /*  ASSET LOADING */
    ///--------------------------------------------------------------------------------///
    public override void OnLoad()
    {
        Console.WriteLine();


        //Load Your asset using 
        ContentManager.Load<rTexture>("background_01",".asset\\background01.png");
        ContentManager.Load<rTexture>("moon",".asset\\moon.png");
        ContentManager.Load<rTexture>("ball",".asset\\ball.png");
        ContentManager.Load<rTexture>("trail_01",".asset\\trail01.png");
    }


    //                                 /*  SETUP SCENE */
    ///--------------------------------------------------------------------------------///
    public override void OnBegined()
    {

        Filter = TextureFilter.TEXTURE_FILTER_BILINEAR;

        var position = ViewPortScale / 2f;
        var radius = MathF.Min(ViewPortWidth, ViewPortHeight) / 4f;


        ///MUST BE ON TOP
        var GameManager = CreateEntity("Manager")
            .SetProcessOrder(int.MaxValue)

            .AddComponent<GameSceneManager>()
            .AddComponent<CameraController>()
            .AddComponent<UICanvas>()
                .SetRenderOrder(int.MaxValue)

            
            .Entity
            ;



        var col = Raylib.GetColor(0X004153FF);
        var graCol = Raylib.GetColor(0X004153A0);
        var col2 = Raylib.GetColor(0X004153AF);


        CreateEntity("Background")
            .AddComponent(new TiledSpriteRenderer(ContentManager.Get<rTexture>("background_01")))
                .SetTintColor(col)
            .AddComponent(new Background(0.7f))
            .AddComponent(new Gradient(16,16,Color.BLANK,Color.BLANK, graCol, graCol))
            .Transform.SetScale(1,1,5f)
            ;




        //CreatePlanet(this,position,40);

        /////////////////////////////////////////////////////////
        var player = CreateEntity("Player", Vector2.UnitY)
                .SetProcessOrder(1)

                .AddComponent<Ball>()
                //.AddComponent<Trail>(new())
                .Entity
                ;
        ;

        #region Player's Children
        CreateChildEntity(player, "player-image")
            //.AddComponent<CircleRenderer>(new(10, Color.GREEN))
            .AddComponent<SpriteRenderer>(new(ContentManager.Get<rTexture>("ball"))).SetRenderOrder(10)
            .AddComponent<test>()
                ;
        #endregion
    }

    
}
```

- Create Component
``` csharp
public class SamepleComponent : Component, IUpdatable
    public override void Update()
    {
        // Update every frame
    }
}

```


- Create Render Component
``` csharp
public class SamepleRenderComponent : RenderableComponent
    public override void Render()
    {
        // Draw every frame
    }
}

```


# Engine Todos list:
- [x] Give it a name
- [x] Create a fully released game with the engine
- [ ] Create shader systems
- [ ] Update trail system
- [ ] Update particle system
