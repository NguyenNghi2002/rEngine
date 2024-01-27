using Engine;
using Engine.SceneManager;
using Engine.UI;
using Raylib_cs;

public class LevelSelectorScene : Scene
{

    public const int BUTTON_SIZE = 30;
    public const int BUTTON_THRESHOLD = 0;
    public const int CELL_PAD = 3;

    public const int TITLE_FONTSCALE = (int)(64); 
    public LevelSelectorScene()
        : base($"level selector",UndoGame.DEFAULT_SCENE_WIDTH, UndoGame.DEFAULT_SCENE_HEIGHT, Raylib_cs.Color.DARKBLUE,Raylib_cs.Color.BLACK)
    {
    }

    public override void OnBegined() 
    {
        NoEnd = NoBegin = true;
        Filter = Raylib_cs.TextureFilter.TEXTURE_FILTER_POINT;

        var skinTitle = UndoGame.TitleUI;
        var skinUsual = UndoGame.DefaultUI;
        var uiCanvas = new UICanvas();


        Table mainTable = new Table()
            .SetFillParent(true)
            .Top();
            ;

        var backBtt = new TextButton("<",UndoGame.GameSkin,skinUsual);
        backBtt.SetPosition(5,5);
        backBtt.OnClicked += (btt) => Core.StartTransition(new FadeTransition(() => MainMenuScene.Instance));
        uiCanvas.Stage.AddElement(backBtt);

        uiCanvas.Stage.AddElement(mainTable);
        mainTable.Add(new Label("Levels",UndoGame.GameSkin,skinTitle))
            .Top()
            ;
        mainTable.Row();

        #region Level Buttons
        Table levelsTable = new Table();
        ScrollPane levelScrollPanel = new ScrollPane(levelsTable);
        mainTable.Add(levelScrollPanel).Width(Value.PercentWidth(0.5f, mainTable))
            .Top().SetExpandY();
            ;

        var counter = 0;
        foreach (var map in UndoGame.LevelsDictionary)
        {
            var path = $"{UndoGame.tilemapDir}\\{map.Value}.tmx";
            var button = new TextButton($"{map.Key}",UndoGame.GameSkin);
            button.ButtonBoundaryThreshold = BUTTON_THRESHOLD;

            if (File.Exists(path))
                button.AddLeftMouseListener((btt) => GotoLevelScene(map.Key));
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

        var scaleX = UndoGame.DEFAULT_SCENE_WIDTH / (float)UndoGame.CHECKED_SIZE;
        var scaleY = UndoGame.DEFAULT_SCENE_HEIGHT / (float)UndoGame.CHECKED_SIZE;
        CreateEntity("Background")
            .AddComponent(new ScrollingSpriteRenderer(ContentManager.Get<Texture2D>("checkedTexture")))
            .SetRenderOrder(-1)
            .Entity
            .ScaleTo(scaleX, scaleY, 1f)
            .MoveTo(UndoGame.DEFAULT_SCENE_WIDTH / 2f, UndoGame.DEFAULT_SCENE_HEIGHT / 2f, 1f);
            ;
    }

    void GotoLevelScene(int levelID)
    {
        var transition = new FadeTransition(() => new PlayScene(levelID));
        transition.FadeInDuration = transition.FadeOutDuration = 0.3f;
        transition.HoldDuration = 0.1f;
        Core.StartTransition(transition);
    }
}


