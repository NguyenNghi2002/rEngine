using Engine;
using Engine.SceneManager;
using Engine.UI;
using Raylib_cs;
using System.Collections;
using System.Numerics;

public class MainMenuScene : Scene
{
    public static Scene Instance;
    public Table _title;
    public MainMenuScene()
        : base($"Main menu", ReUndoGame.DEFAULT_SCENE_WIDTH, ReUndoGame.DEFAULT_SCENE_HEIGHT, Raylib_cs.Color.DARKBLUE, Raylib_cs.Color.BLACK)
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

        _title = new Table()
            .SetFillParent(true);
        _title.Add(new Engine.UI.Image(new SpriteDrawable(ContentManager.Get<Texture2D>("title")), Engine.UI.Scaling.Fill, (int)Align.Bottom))
            .Top();
        uiCanvas.Stage.AddElement(_title);
        CreateEntity("ui")
            .AddComponent<UICanvas>(uiCanvas)
                .SetRenderOrder(999)
            ;

        var backgroundEn = CreateEntity("Background",new Vector2(ViewPortWidth/2f, ViewPortHeight / 2f));


        var path = $".contents\\Clouds\\Clouds 5\\";
        var files = Directory.GetFiles(path);
        for (int i = 0; i < files.Length; i++)
        {
            if (Path.GetExtension(files[i]) != ".png") continue;
            var txture = ContentManager.Load<Texture2D>($"cloud_{i}", files[i]);
            CreateChildEntity(backgroundEn, $"bg_bottom_{i}")
            .AddComponent<ScrollingSpriteRenderer>(new (txture,10+(2*i*i)/1,0)).SetRenderOrder(i)
            ;
        }

        
    }


    Table CreateStart(Stage stage)
    {
        var startStyle = ReUndoGame.GameSkin.Get<TextButtonStyle>(ReUndoGame.MainMenuStartUI);
        var quitStyle = ReUndoGame.GameSkin.Get<TextButtonStyle>(ReUndoGame.MainMenuQuitUI);
        var optionStyle = ReUndoGame.GameSkin.Get<TextButtonStyle>(ReUndoGame.MainMenuOptionUI);

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
            Raylib.PlaySound(ContentManager.Get<Sound>("sfx-forward"));
            var menu = CreateMenuTable(optionStyle,quitStyle,out Table optionsTable);
            stage.AddElement(menu);

            var stageWidth = stage.GetWidth();
            var stageHeight = stage.GetHeight();
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

        Core.StartCoroutine(AnimateElementY(_title, _title.GetY(), (_title.PreferredHeight - _title.GetStage().GetHeight()) / 2f + 10, 1f));
        yield return new WaitForSecond(0.3f);

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

        Table options = new Table();
        Container preview = new Container();
            ;
        menuTable.Add(options);
        menuTable.Add(preview).GrowX().SetPadLeft(Value.PercentWidth(0.2f));

        // Play Button
        var playButton = CreateButton("Play",optionButtonStyle);
        playButton.OnClicked += (btt) =>
        {
            Raylib.PlaySound(ContentManager.Get<Sound>("sfx-click_play"));

            var fadeTransition = new SwipeTransition(() => new LevelSelectorScene());
            fadeTransition.OnFadedOut += ()=> ReUndoGame.SettingWidget.Remove();
            Core.StartTransition(fadeTransition);
        };
        options.Add(playButton).SetExpandX();
        options.Row();

        // Setting Buton
        var settingButton = CreateButton("Setting", optionButtonStyle);
        settingButton.OnClicked += (b) =>
        {
            if (preview.GetElement() == null) 
            {
                Raylib.PlaySound(ContentManager.Get<Sound>("sfx-forward"));
                preview.SetElement(ReUndoGame.SettingWidget.Right());

            }
            else
            {
                ReUndoGame.SettingWidget.Remove();
                Raylib.PlaySound(ContentManager.Get<Sound>("sfx-back"));
            }
        };
        options.Add(settingButton)
            .Left().Pad(5).SetExpandX();
        options.Row();


        

        // Quit Buton
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
                Raylib.PlaySoundMulti(ContentManager.Get<Sound>("sfx-hover"));
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
