using Engine;
using Engine.SceneManager;
using Engine.UI;
using Raylib_cs;


public class LevelSelectorScene : Scene
{

    public const int BUTTON_SIZE = 32;
    public const int BUTTON_THRESHOLD = 0;
    public const int CELL_PAD = 3;

    public const int TITLE_FONTSCALE = (int)(64); 

    public void GoToMainMenu()
    {
        var transition = new SwipeTransition(() => MainMenuScene.Instance);
        transition.OnBegined += ()=>Raylib.PlaySound(ContentManager.Get<Sound>("sfx-back"));

        Core.StartTransition(transition);
    }
    public LevelSelectorScene()
        : base($"level selector",ReUndoGame.DEFAULT_SCENE_WIDTH, ReUndoGame.DEFAULT_SCENE_HEIGHT, Raylib_cs.Color.DARKBLUE,Raylib_cs.Color.BLACK)
    {
    }

    public override void OnBegined() 
    {
        //NoEnd = NoBegin = true;
        Filter = Raylib_cs.TextureFilter.TEXTURE_FILTER_POINT;

        var skinTitle = ReUndoGame.TitleUI;
        var skinUsual = ReUndoGame.DefaultUI;
        var uiCanvas = new UICanvas();

        CreateEntity("Manager")
            .AddComponent<LevelSelectorInputManager>(new())
            ;

        Table mainTable = uiCanvas.Stage.AddElement(new Table())
            .SetFillParent(true)
            .Top();
            ;


        /// back Button
        var backSfx = ContentManager.Get<Sound>("sfx-back");
        var hoverSfx = ContentManager.Get<Sound>("sfx-hover");
        var backBtt = new IconButton(ReUndoGame.GameSkin,skinUsual);
        Console.WriteLine(uiCanvas.Stage.GetWidth());
        backBtt.SetPosition(ViewPortWidth- backBtt.PreferredWidth*2 - 5, 5);
        backBtt.SetSize(backBtt.PreferredWidth * 2, backBtt.PreferredHeight * 2);
        backBtt.Layout();
        backBtt.OnHovered += i => Raylib.PlaySound(hoverSfx);
        backBtt.OnClicked += (btt) => GoToMainMenu();
        uiCanvas.Stage.AddElement(backBtt);

        mainTable.Add(new Label("Levels",ReUndoGame.GameSkin,skinTitle))
            .Top()
            ;
        mainTable.Row();

        #region Level Buttons
        Table levelsTable = new Table();
        ScrollPane levelScrollPanel = new ScrollPane(levelsTable);
        mainTable.Add(levelScrollPanel).Width(Value.PercentWidth(0.5f, mainTable))
            .Top().SetExpandY();
            ;

        var playSfx = ContentManager.Get<Sound>("sfx-click_play");
        var counter = 0;
        var disableBuffer = false;
        foreach (var map in ReUndoGame.LevelsDictionary)
        {
            var path = $"{ReUndoGame.tilemapDir}\\{map.Value.Name}.tmx";
            var button = new TextButton($"{map.Key}",ReUndoGame.GameSkin,ReUndoGame.LevelSeletor);
            button.ButtonBoundaryThreshold = BUTTON_THRESHOLD;

            if (File.Exists(path))
            {
                button.OnHovered += i =>
                {
                    if (i && map.Value.AllowReplay) 
                        Raylib.PlaySound(hoverSfx);
                };
                button.OnClicked += (btt) => 
                {
                    Raylib.PlaySound(playSfx);
                    GotoLevelScene(map.Key);
                };
            }

#if UNLOCK_LEVELS
            button.SetDisabled(false);
#else
            button.SetDisabled(!map.Value.AllowReplay);
#endif

            levelsTable.Add(button)
                 .Pad(CELL_PAD).Top().Left().Size(BUTTON_SIZE);
            ;

            if (++counter % 3 == 0)
                levelsTable.Row();
        } 
#endregion

        /// TODO: FIX Scrollpanel bug where button in scroll panel stay in pressed state when panel scrolled.
        CreateEntity("table")
            .AddComponent(uiCanvas);

        var scaleX = ReUndoGame.DEFAULT_SCENE_WIDTH / (float)ReUndoGame.CHECKED_SIZE;
        var scaleY = ReUndoGame.DEFAULT_SCENE_HEIGHT / (float)ReUndoGame.CHECKED_SIZE;
        CreateEntity("Background")
            .AddComponent(new ScrollingSpriteRenderer(ContentManager.Get<Texture2D>("checkedTexture")))
            .SetRenderOrder(-1)
            .Entity
            .ScaleTo(scaleX, scaleY, 1f)
            .MoveTo(ReUndoGame.DEFAULT_SCENE_WIDTH / 2f, ReUndoGame.DEFAULT_SCENE_HEIGHT / 2f, 1f);
            ;
    }

    void GotoLevelScene(int levelID)
    {
        var transition = new SwipeTransition(() => new PlayScene(levelID));
        transition.EnterDuration = transition.ExitDuration = 0.3f;
        transition.HoldDuration = 0.1f;
        Core.StartTransition(transition);
    }
}


public class LevelSelectorInputManager : Component, IUpdatable
{
    public int UpdateOrder { get; set; }

    public void Update()
    {
        if (Input.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
        {
            (Scene as LevelSelectorScene)?.GoToMainMenu();
        }
    }
}