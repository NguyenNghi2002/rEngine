using Engine;
using Engine.UI;
using Raylib_cs;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Security.Cryptography;

new ReUndoGame().Run();

public class MusicUpdater: GlobalManager,IUpdatable
{
    public List<Music> registerMusics = new List<Music>();
    int IUpdatable.UpdateOrder { get; set; }

    void IUpdatable.Update()
    {
           
        foreach (var music in registerMusics)
        {
            Raylib.UpdateMusicStream(music);
        }
    }

}
public class ReUndoGame : Engine.Core
{
    public const string GameTitle = "Reundo";
    public const int DEFAULT_SCENE_WIDTH = 160 * 2;
    public const int DEFAULT_SCENE_HEIGHT = 160* 2;

    public const int CHECKED_SIZE = 50;
    public const int CHECKED_COUNT = 5;

    public const string mapFileName = "map";
    public const string tilemapDir = ".contents\\tilemaps";

    public static Table SettingWidget;

    public static readonly Dictionary<int, LevelInfo> LevelsDictionary = new Dictionary<int, LevelInfo>()
    {
        [1] = new LevelInfo{Name = "map1",AllowReplay = true},
        [2] = new LevelInfo{Name = "map2"},
        [3] = new LevelInfo{Name = "map3"},
        [4] = new LevelInfo{Name = "map4"},
        [5] = new LevelInfo{Name = "map5"},
        [6] = new LevelInfo{Name = "map6"},
        [7] = new LevelInfo{Name = "map7"},
        [8] = new LevelInfo{Name = "map8"},
        [9] = new LevelInfo{Name = "map9"},
        [10] =new LevelInfo{Name = "map10"},
        [11] =new LevelInfo{Name = "map11"},
        [12] =new LevelInfo{Name = "map12"},
    };
    public static Skin GameSkin;

    public const string DefaultUI = "default";
    public const string MainMenuOptionUI = "mainmenu_option";
    public const string MainMenuQuitUI = "mainmenu_quit";
    public const string MainMenuStartUI = "mainmenu_start_FIXED";

    public const string TitleUI = "title";
    public const string PlaySceneTitleUI = "playscene_title";
    public const string PlayScenePauseUI = "playscene_pause";
    public const string LevelSeletor = "levelSelector";

    public static Sound[] sfxs;
    public static Music[] musics;

    public override void Initialize()
    {
        Title = GameTitle;
        WindowWidth = DEFAULT_SCENE_WIDTH*2;
        WindowHeight = DEFAULT_SCENE_HEIGHT*2;
        Debugging.EnableConsoleLog = false;
        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        base.Initialize();
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(KeyboardKey.KEY_NULL);

        var icon = Raylib.LoadImage("icon.png");
        Raylib.SetWindowIcon(icon);
        Raylib.UnloadImage(icon);
        
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
        Font gameFont = ContentManager.Load<Font>("gamer", ".contents\\fonts\\Gamer.ttf");
        sfxs = new Sound[]
        {
            ContentManager.Load<Sound>("sfx-click"      , ".contents\\click.wav"),
            ContentManager.Load<Sound>("sfx-hover"      , ".contents\\select.wav"),
            ContentManager.Load<Sound>("sfx-click_play" , ".contents\\click_play.wav"),
            ContentManager.Load<Sound>("sfx-back"       , ".contents\\back.wav"),
            ContentManager.Load<Sound>("sfx-forward"    , ".contents\\forward.wav"),
            ContentManager.Load<Sound>("sfx-walk"       , ".contents\\walk.wav"),
            ContentManager.Load<Sound>("sfx-undo"       , ".contents\\undo.wav"),
            ContentManager.Load<Sound>("sfx-redo"       , ".contents\\redo.wav"),
        };
        musics = new Music[]
        {
            ContentManager.Load<Music>("music-TVG"      , ".contents\\The Verdant Grove.ogg"),
        };
        var buttonIconSprite        = ContentManager.Load<Texture2D>("button-icon"       , ".contents\\button-icon.png");
        var buttonSprite = ContentManager.Load<Texture2D>("button"       , ".contents\\button.png");
        var buttonOverSprite    = ContentManager.Load<Texture2D>("button-over"   , ".contents\\button-over.png");
        var buttonDownSprite  = ContentManager.Load<Texture2D>("button-down", ".contents\\button-down.png");
        var buttonDisableSprite  = ContentManager.Load<Texture2D>("button-locked", ".contents\\button-disabled.png");
        var lockSprite          = ContentManager.Load<Texture2D>("lock"         , ".contents\\lock.png");
        var titleSprite = ContentManager.Load<Texture2D>("title"         , ".contents\\title.png");

        var baseSize        = gameFont.baseSize;

        #region Game skin
        GameSkin = Skin.CreateDefaultSkin();
        GameSkin.Add(DefaultUI, gameFont);
        GameSkin.Add(DefaultUI, new LabelStyle(gameFont, Color.WHITE, gameFont.baseSize, 1f));
        var btt = GameSkin.Add(DefaultUI, GameSkin.Get<TextButtonStyle>().Clone());
        btt.Font = gameFont;
        btt.FontScale = gameFont.baseSize ;
        btt.DisabledFontColor = Color.DARKGRAY;
        btt.PressedOffsetY = 1;
        

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
        GameSkin.Add(DefaultUI, new IconButtonStyle(){
            Up = new SpriteDrawable(buttonIconSprite),
            Down = new SpriteDrawable(buttonIconSprite) { Offset = -Vector2.UnitY},
            Over = new SpriteDrawable(buttonIconSprite) { Offset = Vector2.UnitY},
        });
        GameSkin.Add(TitleUI, new LabelStyle()
        {
            Font = gameFont,
            FontColor = Color.WHITE,
            FontScale = gameFont.baseSize * 2f,
            Spacing = 1,
        });
        
        var levelButton = GameSkin.Add(LevelSeletor, GameSkin.Get<TextButtonStyle>(DefaultUI).Clone());
        #region Level Button
        levelButton.Up = new SpriteDrawable(buttonSprite);
        levelButton.Over = new SpriteDrawable(buttonOverSprite) { Offset = Vector2.UnitY };
        levelButton.Down = new SpriteDrawable(buttonDownSprite) { Offset = -Vector2.UnitY };
        levelButton.Disabled = new SpriteDrawable(lockSprite);
        levelButton.UnpressedOffsetY = -6;
        levelButton.PressedOffsetY = -6 + 1;
        levelButton.FontScale = 20;
        levelButton.DownFontColor = Color.GRAY;
        levelButton.OverFontColor = Color.WHITE; 
        levelButton.DisabledFontColor =  Color.BLANK;
        #endregion
        GameSkin.Add(PlaySceneTitleUI, new LabelStyle()
        {
            Font = gameFont,
            FontColor = Color.WHITE,
            FontScale = gameFont.baseSize,
            Spacing = 1,
        });
        GameSkin.Add(PlayScenePauseUI, new TextButtonStyle
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
        GameSkin.Add(PlayScenePauseUI, new WindowStyle
        {
            Background = new PrimitiveDrawable(new Color(10, 10, 10, 255)),
            TitleFont = gameFont,
            TitleFontScale = gameFont.baseSize * 1.5f,
            TitleFontColor = Color.RAYWHITE,
        }); 
        #endregion

        Scene = new MainMenuScene();
        //Managers.Add(new ImguiEntityManager());


        var musicManager = new MusicUpdater();
        musicManager.registerMusics.Add(musics[0]);
        AddManager(musicManager);

        Raylib.PlayMusicStream(musics[0]);
        SettingWidget = CreateSettingWidget(GameSkin,"default","default");
    }

    static Slider masterVolume ;
    static Slider sfxVolume ;
    static Slider musicVoume;

    Table CreateSettingWidget(Skin skin,string labelStyle,string sliderStyle)
    {
        float min = 0f, max = 1f;
        float step = .1f;
        bool verticle = true;
        Table tb = new Table();


        tb.Add(CreateTextSlider("Master", min, max, step, skin, labelStyle, sliderStyle, out masterVolume));
        tb.Row();
        tb.Add(CreateTextSlider("SFX", min, max, step, skin, labelStyle, sliderStyle, out sfxVolume));
        tb.Row();
        tb.Add(CreateTextSlider("Music", min, max, step, skin, labelStyle, sliderStyle,out musicVoume));

        masterVolume.OnChanged += MasterVolume_OnChanged;
        sfxVolume.OnChanged += SfxVolume_OnChanged;
        musicVoume.OnChanged += MusicVoume_OnChanged;

        return tb; 
    }

    private void MusicVoume_OnChanged(float v)
    {
        foreach (var music in musics)
            Raylib.SetMusicVolume(music, v);
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-walk"));
    }

    private void SfxVolume_OnChanged(float v)
    {
        foreach (var sound in sfxs)
            Raylib.SetSoundVolume(sound, v);
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-walk"));
    }

    private void MasterVolume_OnChanged(float v)
    {
        Raylib.SetMasterVolume(v);
        Raylib.PlaySound(ContentManager.Get<Sound>("sfx-walk"));

    }

    Table CreateTextSlider(string text,float min,float max,float step,Skin skin,string labelStyle,string sliderStyle, out Slider slider)
    {
        Table group = new Table();
        Label label = new Label(text,skin,labelStyle);
         slider = new Slider(min, max, step, false, skin, sliderStyle)
        {
            SliderBoundaryThreshold = float.MaxValue,
            Value = max,
        };

        group.Add(label);
        group.Row();
        group.Add(slider);

        return group;
    }

}


#if false
public class BinaryTreePacking<T>
{
    private class Node<T>
    {
        public T Value;
        public Node<T>? Parent;
        public Node<T>[] children;
        public float X, Y, W, H;

        Node<T>? Insert(T value, float width, float height)
        {

            if (this.children != null) // If is not leaf
            {
                Node<T>? newNode;
                newNode = this.children[0].Insert(value, width, height); // go to 1st child 
                if (newNode != null) return newNode;
                newNode = this.children[1].Insert(value, width, height); // go to 2nd child 
                if (newNode != null) return newNode;
            }
            else // If is leaf
            {
                ///Decline Guard
                if (this.Value != null) return null; //current node is occupied

                if (width > this.W || height > this.H) return null; //current node has no space to fit new node

                ///Accept phase
                Node<T> newNode = new Node<T>()
                {
                    Parent = this,
                    W = width,
                    H = height
                };

                if (width == this.W && height == this.H) // Fit perfectly
                    return newNode;

                //otherwise
                newNode.children = new Node<T>[2];

            }
            return Insert(value, width, height);
        }
        Node<T> Left => this.children[0];
        Node<T> Right => this.children[1];
        Node<T>[] CreateChildren() => new Node<T>[2];
    }


} 
#endif