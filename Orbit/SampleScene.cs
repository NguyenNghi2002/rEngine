using Engine;
using Engine.DefaultComponents.Render.Primitive;
using Engine.SceneManager;
using Engine.UI;
using ImGuiNET;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

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

        Filter = TextureFilter.TEXTURE_FILTER_POINT;

        var position = ViewPortScale / 2f;
        var radius = MathF.Min(ViewPortWidth, ViewPortHeight) / 4f;


        ///MUST BE ON TOP
        var GameManager = CreateEntity("Manager")
            .SetProcessOrder(int.MaxValue)

            .AddComponent<GameSceneManager>()
            .AddComponent<CameraController>()
            .AddComponent<UICanvas>()
                .SetRenderOrder(int.MaxValue)
            //.AddComponent<FollowCursor>()
            .AddComponent<CircleRenderer>(new (5,Color.RED))

            
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
            .Transform.SetScale(1,1,1f)
            ;




        //CreatePlanet(this,position,40);

        /////////////////////////////////////////////////////////
        var player = CreateEntity("Player", Vector2.UnitY)
                .SetProcessOrder(1)

                .AddComponent<Player>()
                //.AddComponent<Trail>(new())
                .Entity
                ;
        ;

        #region Player's Children (BALL RENDER)
        CreateChildEntity(player, "player-image")
            //.AddComponent<CircleRenderer>(new(10, Color.GREEN))
            .AddComponent<SpriteRenderer>(new(ContentManager.Get<rTexture>("ball"))).SetRenderOrder(10)
            .AddComponent<test>()
            .Transform.SetScale(1f,1f,1f);
                ;
        #endregion
    }

    public static Entity CreatePlanet(Scene scene,Vector2 position,int radius,out Planet planet)
    {
        /////////////////////////////////////////////////////////
        var planet_root = scene.CreateEntity("Planet", position)
                .AddComponent<Planet>(new(radius),out planet)
                .AddComponent<Shakable>()
                .AddComponent<Pulsable>()
                .Entity
            ;
        //--------------------//
        var color = Raylib.ColorFromNormalized(new Vector4(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle(), 1f));

        scene.CreateChildEntity(planet_root, "planet-image", Vector2.Zero, false)
            .AddComponent<RingRenderer>(new(radius,radius /2, color))
            //.AddComponent<SpriteRenderer>(new(ContentManager.Get<rTexture>("moon")));
            ;
        var planet_center =
        scene.CreateChildEntity(planet_root, "planet-center")
            ;

        scene.CreateChildEntity(planet_center, "planet-slot")
            //.AddComponent<RingRenderer>(new(10,10-4, Color.LIGHTGRAY))

            ;
        //--------------------//
        /////////////////////////////////////////////////////////
        Debug.Assert(planet_root != null);
        return planet_root;
        
    }
}
