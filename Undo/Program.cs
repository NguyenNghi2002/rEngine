using Engine;
using Engine.SceneManager;
using Engine.UI;
using Raylib_cs;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Security.Cryptography;

new UndoGame().Run();

public class UndoGame : Engine.Core
{
    public const int DEFAULT_SCENE_WIDTH = 160 * 2;
    public const int DEFAULT_SCENE_HEIGHT = 160* 2;

    public const int CHECKED_SIZE = 50;
    public const int CHECKED_COUNT = 5;

    public const string mapFileName = "map";
    public const string tilemapDir = ".contents\\tilemaps";

    public static readonly Dictionary<int, string> LevelsDictionary = new Dictionary<int, string>()
    {
        [1] = "map1",
        [2] = "map2",
        [3] = "map3",
        [4] = "map4",
        [5] = "map5",
        [6] = "map6",
        [7] = "map7",
        [8] = "map8",
        [9] = "map9",
        [10] = "map10",
        [11] = "map11",
        [12] = "map12",
    };
    public static Skin GameSkin;

    public const string DefaultUI = "default";
    public const string MainMenuOptionUI = "mainmenu_option";
    public const string MainMenuQuitUI = "mainmenu_quit";
    public const string MainMenuStartUI = "mainmenu_start_FIXED";

    public const string TitleUI = "title";
    public const string PauseUI = "playscene_pause";

    public override void Initialize()
    {
        Raylib.SetExitKey(KeyboardKey.KEY_NULL);
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        base.Initialize();
        Raylib.SetTargetFPS(60);
        //Create Scene
        //Scene = new PlayScene(5);

        
        ContentManager.Load("checkedTexture", () =>
        {
            var checkedImg = Raylib.GenImageChecked(CHECKED_SIZE, CHECKED_SIZE, CHECKED_COUNT, CHECKED_COUNT, Color.BLUE, Color.DARKBLUE);
            var checkedTexture = Raylib.LoadTextureFromImage(checkedImg);
            Raylib.UnloadImage(checkedImg);
            return checkedTexture;
        }); ;
        //var mainFont = ContentManager.Load<Font>("UpheavalPro", ".contents\\fonts\\UpheavalPro.ttf");
        //var mainFont = ContentManager.Load<Font>("Fipps-Regular", ".contents\\fonts\\Fipps-Regular.otf");
        //var mainFont = ContentManager.Load<Font>("8-bit", ".contents\\fonts\\8-BIT WONDER.ttf");
        var gameFont = ContentManager.Load<Font>("gamer", ".contents\\fonts\\Gamer.ttf");

        var baseSize = gameFont.baseSize;

        GameSkin = Skin.CreateDefaultSkin();
        GameSkin.Add(DefaultUI, gameFont);
        GameSkin.Add(DefaultUI, new LabelStyle(gameFont, Color.WHITE, gameFont.baseSize,1f));
        GameSkin.Add(DefaultUI, new TextButtonStyle
        {
            Font = gameFont,
            FontScale = gameFont.baseSize,
            Over = GameSkin.Get<TextButtonStyle>().Over,
            Up = GameSkin.Get<TextButtonStyle>().Up,
            Down = GameSkin.Get<TextButtonStyle>().Down,
            Checked = GameSkin.Get<TextButtonStyle>().Checked,

            PressedOffsetY = 1,
        });

        GameSkin.Add(MainMenuOptionUI, new TextButtonStyle
        {
            Font = gameFont,
            FontScale = gameFont.baseSize,
            FontColor = Color.WHITE,
            PressedOffsetY = 1,
            Over = null,
            Up = null,
            Down = null,
            Checked = null,
        });
        GameSkin.Add(MainMenuQuitUI, new TextButtonStyle
        {
            Font = gameFont,
            FontScale = gameFont.baseSize,
            FontColor = Color.RED,
            OverFontColor = Color.ORANGE,
            DownFontColor = Color.DARKPURPLE,
            CheckedOffsetY = 1,
            Over = null,
            Up = null,
            Down = null,
            Checked = null,
        });
        GameSkin.Add(MainMenuStartUI, new TextButtonStyle
        {
            Font = gameFont,
            FontScale = gameFont.baseSize,
            CheckedOffsetY = 1,
            Over = null,
            Up = null,
            Down = null,
            Checked = null,
        });

        GameSkin.Add(TitleUI, new LabelStyle()
        {
            Font = gameFont,
            FontColor = Color.WHITE,
            FontScale = gameFont.baseSize * 2f,
            Spacing = 1,
        });

        GameSkin.Add(PauseUI, new TextButtonStyle
        {
            Font = gameFont,
            FontScale = gameFont.baseSize,

            FontColor = Color.LIGHTGRAY,
            OverFontColor = Color.WHITE,
            DownFontColor = Color.DARKPURPLE,

            Over = GameSkin.Get<TextButtonStyle>().Over,
            Up = GameSkin.Get<TextButtonStyle>().Up,
            Down = GameSkin.Get<TextButtonStyle>().Down,
            Checked = GameSkin.Get<TextButtonStyle>().Checked,

            PressedOffsetY = 1,
        });
        GameSkin.Add(PauseUI, new WindowStyle
        {
            Background = new PrimitiveDrawable(new Color(10,10,10,255)),
            TitleFont = gameFont,
            TitleFontScale = gameFont.baseSize * 1.5f,
            TitleFontColor = Color.RAYWHITE,
        });

        Scene = new MainMenuScene();
        //Managers.Add(new ImguiEntityManager());
    }
}

public class MainMenuScene : Scene
{
    public static Scene Instance;
    public MainMenuScene()
        : base($"Main menu", UndoGame.DEFAULT_SCENE_WIDTH, UndoGame.DEFAULT_SCENE_HEIGHT, Raylib_cs.Color.DARKBLUE, Raylib_cs.Color.BLACK)
    {
    }

    public override void OnBegined()
    {
        NoBegin = NoEnd = true;
        MainMenuScene.Instance = this;

        Filter = Raylib_cs.TextureFilter.TEXTURE_FILTER_POINT;
        var uiCanvas = new UICanvas();
        //uiCanvas.Stage.SetDebugAll(true);
        

        uiCanvas.Stage.AddElement(CreateStart(uiCanvas.Stage)); ;

        CreateEntity("ui")
            .AddComponent<UICanvas>(uiCanvas)
            ;

    }


    Table CreateStart(Stage stage)
    {
        var startStyle = UndoGame.GameSkin.Get<TextButtonStyle>(UndoGame.MainMenuStartUI);
        var quitStyle = UndoGame.GameSkin.Get<TextButtonStyle>(UndoGame.MainMenuQuitUI);
        var optionStyle = UndoGame.GameSkin.Get<TextButtonStyle>(UndoGame.MainMenuOptionUI);

        var flipflop = true;
        var flashingTimer = Core.Schedule(0.04f, true, flipflop, (timer) =>
        {
            var style = startStyle;
            style.FontColor = flipflop ? Color.RED : Color.YELLOW;
            flipflop = !flipflop;
        }); /// Stopped when button pressed

        var btt = new TextButton("[start]", startStyle);
        btt.SetFillParent(true);
        btt.GetLabel().Bottom();
        btt.OnClicked += (b) =>
        {
            var startButtonDuration = 1f;
            var introButtonDuration = 0.65f;
            var intervalButton = 0.07f;

            flashingTimer.Stop();
            startStyle.FontColor = new Color(255,255,255,230);
            b.SetDisabled(true);

            Core.StartCoroutine(AnimateElementY(b, b.GetY(),startStyle.FontScale, startButtonDuration));
            Core.Schedule(startButtonDuration, false, null, t => b.Remove());

            var menu = CreateMenuTable(optionStyle,quitStyle,out Table optionsTable);
            stage.AddElement(menu);

            var stageWidth = stage.GetWidth();
            var buttons = optionsTable.GetCells().Select(c => c.GetElement<TextButton>()).ToArray();
            Core.StartCoroutine(AnimateElements(stageWidth, -stageWidth, introButtonDuration,intervalButton,buttons));
        };

        return btt;
    }
    IEnumerator AnimateElements(float start,float offset ,float durationEach,float interval,params Element[] elements)
    {
        foreach (var item in elements)
        {
            item.SetVisible(false);
            item.SetTouchable(Touchable.Disabled);
        }

        foreach (var item in elements)
        {
            item.SetVisible(true);
            Core.StartCoroutine(AnimateElementX(item, start, offset, durationEach));
            Core.Schedule(durationEach,false,null,(t)=> item.SetTouchable(Touchable.Enabled));

            yield return new WaitForSecond(interval);
        } 

    }
    Table CreateMenuTable(TextButtonStyle optionButtonStyle, TextButtonStyle quitButtonStyle,out Table optionsTable)
    {
        Table menuTable = new Table()
            .SetFillParent(true).PadLeft(5)
            .Left()
            ;

        Table options = new Table()
            ;
        menuTable.Add(options).SetExpandY();

        var playButton = CreateButton("Play",optionButtonStyle);
        playButton.OnClicked += (btt) =>
        {
            Core.StartTransition(new FadeTransition(() => new LevelSelectorScene()));
        };
        options.Add(playButton).SetExpandX();
        options.Row();


        options.Add(CreateButton("Setting", optionButtonStyle))
            .Left().Pad(5).SetExpandX();
        options.Row();


        

        var quitBtt = CreateButton("Quit",quitButtonStyle);
        quitBtt.OnClicked+= (b) => Core.Instance.ExitApp();
        options.Add(quitBtt).Left().Pad(5).SetExpandX();

        optionsTable = options;
        return menuTable;
    }
    TextButton CreateButton(string text,TextButtonStyle style)
    {
        var button = new TextButton(text, style);
        button.PadLeft(1).PadTop(2);

        button.ButtonBoundaryThreshold = 0;
        button.OnHovered += (isHovered) =>
        {
            var label = button.GetLabel();
            if (isHovered)
            {
                Core.StartCoroutine(AnimateElementX(label,1,5f,1f));
            }
            else
            {
                Core.StartCoroutine(AnimateElementX(label,5f, -4f,1f));
            }
        };

        return button;
    }
    IEnumerator AnimateElementX(Element element,float start,float offset,float duration)
    {
        float elapse = 0;

        while (elapse < duration)
        {
            elapse += Time.DeltaTime;
            element.SetX(Easings.EaseExpoOut(elapse, start, offset, duration));
            yield return null;
        }
        element.SetX(start + offset);
    }
    IEnumerator AnimateElementY(Element element, float start, float offset, float duration)
    {
        float elapse = 0;

        while (elapse < duration)
        {
            elapse += Time.DeltaTime;
            element.SetY(Easings.EaseExpoOut(elapse, start, offset, duration));
            yield return null;
        }
        element.SetY(start + offset);
    }



}


