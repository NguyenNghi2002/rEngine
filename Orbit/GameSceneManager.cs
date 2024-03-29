﻿using Engine;
using Engine.SceneManager;
using Engine.UI;
using ImGuiNET;
using Raylib_cs;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

public class GameSceneManager : Component,ICustomInspectorImgui
{
    public const uint ShowPlanetCount = 2;
    public static GameSceneManager Instance;

    public float BallFloatSpeed = 200f;

    public Queue<Planet> planetNextQueue = new Queue<Planet>();

    public Player Player;
    public Planet CurrentPlanet;
    public event Action OnJumped;

    public uint Score = 0;
    public Label ScoreLabel_UI;
    public float ScoreLabelScale = 20;

    public float MinPlanet = 15, MaxPlanet = 16;
    public float GameTimeScale = 1f;
    public float GameDeltaTime => Time.UnscaledDeltaTime * GameTimeScale;
    public override void OnAddedToEntity()
    {
        //Setup UI
        if (Entity.TryGetComponent<UICanvas>(out var ui))
        {
            
            var screenCenter = Core.Scene.ViewPortScale / 2f;
            Table mainTable = new Table()
                .SetFillParent(true)
                .Top().Right()
                .Pad(5)
                ;

            Table scoreTable = new Table()
                .SetFillParent(true)
                .Top()
                .PadTop(16)
                ;

            #region Pause dialog/window

            Dialog pauseMenu = new Dialog("Pause", WindowStyle.CreateRaylib());
            pauseMenu.SetFillParent(false);
            pauseMenu.SetMovable(false);
            //pauseMenu.SetSize(screenCenter.X, screenCenter.Y);
            var resetBtt = pauseMenu.AddButton("Reset", TextButtonStyle.CreateRaylib(),out Cell bttCell)
                .AddLeftMouseListener((b)=>ResetScene())
                as TextButton;

            (resetBtt.GetStyle().Up as PrimitiveDrawable).LineWidth = 1f;
            resetBtt.GetLabel().SetFontScale(10);
            bttCell.Size(32,16);

            pauseMenu.Pad(10);
            pauseMenu.SetZIndex(0);

            pauseMenu.Pack();
            pauseMenu.SetPosition(screenCenter.X, screenCenter.Y, (int)Align.Center); 

            #endregion

            var style = TextButtonStyle.CreateRaylib();
            (style.Up as PrimitiveDrawable).LineWidth = 1f;
            TextButton pauseBtt = new TextButton("||",style);
            
            pauseBtt.AddLeftMouseListener((btt) =>
            {
                    var coroutineManager = Core.Instance.Managers.Find(m => typeof(CoroutineManager).IsAssignableFrom(m.GetType())) as CoroutineManager;
                if (!pauseMenu.Remove()) 
                {
                    
                    ui.Stage.AddElement(pauseMenu);

                    //pauseMenu.SetSize(screenCenter.X, screenCenter.Y);
                    Player.Entity.GetComponent<Player>().Enable = false;
                    Entity.GetComponent<CameraController>().Enable = false;
                    //CurrentPlanet.Entity.GetComponent<Shakable>().Enable
                    //GameSceneManager.Instance.GameTimeScale = 0f;
                    coroutineManager.TogglePauseAll(true);
                }
                else
                {
                    Player.Entity.GetComponent<Player>().Enable = true;
                    Entity.GetComponent<CameraController>().Enable = true;
                    coroutineManager.TogglePauseAll(false);
                    //GameSceneManager.Instance.GameTimeScale = 1f;
                }
            });

            mainTable.Add(pauseBtt).Size(16,16);

            ScoreLabel_UI = new Label(Score.ToString(),ScoreLabelScale);
            scoreTable.Add(ScoreLabel_UI);

            ui.Stage.AddElement(mainTable).SetZIndex(10);
            ui.Stage.AddElement(scoreTable);

            ui.SetRenderOrder(10);
            
        }

        ///If you make mistake, insult urself
        if (Instance != null) throw new Exception("why tf you create more");
        Instance = this;

        /// Finding Existance Entities
        Player = Scene.FindComponents<Player>().FirstOrDefault();
        planetNextQueue = new Queue<Planet>( );
        
        AssurePlanets();

    }
    public override void OnRemovedFromEntity()
    {
        var coroutineManager = Core.Instance.Managers.Find(m => typeof(CoroutineManager).IsAssignableFrom(m.GetType())) as CoroutineManager;
        //coroutineManager.Runnings.Where((c) => c is not );

        Instance = null;
    }

    internal void AssurePlanets()
    {
        if(CurrentPlanet == null)
        {
            var radius = RandomSize();
            //If queue have somthing, then pass to current planet
            //else create new entity Planet
            CurrentPlanet = planetNextQueue.TryDequeue(out var planet) ? 
                planet : SampleScene.CreatePlanet(Scene, Vector2.Zero, (int)radius,out _).GetComponent<Planet>();

            Debug.Assert(CurrentPlanet != null);

            //Target the camera control
            Entity.GetComponent<CameraController>().Planet = CurrentPlanet.Entity;

        }
        
        Debug.Assert(CurrentPlanet != null);

        //If there are planet on queue , then 
        if (planetNextQueue.Count < 1)
        {
            AddPlanet();
        }
        

    }


    public Planet AddPlanet()
    {
        var radius = RandomSize();
        var position = RandomPosition();

        SampleScene.CreatePlanet(Scene, position.ToInt2(), (int)radius, out Planet planet);

        if(!planetNextQueue.Contains(planet))
            planetNextQueue.Enqueue(planet);

        return planet;
    }
    void ExecuteJumpNext()
    {
        Player ball = Player;
        Planet next = planetNextQueue.TryDequeue(out Planet? p) ? p : AddPlanet();

        ball.Transform.Parent = next.slotTF;
        ball.Transform.LocalPosition2 = Vector2.Zero;

        if (Entity.TryGetComponent<CameraController>(out var cc))
            cc.Planet = next.Entity;


        var prev = CurrentPlanet;
        CurrentPlanet = next;

        ///Run Fade out animation then Destroy planet
        junpAndDes =  Core.StartCoroutine(SpaceOutAndDestroy(CurrentPlanet, prev));
    }
    ICoroutine junpAndDes;
    /// <summary>
    /// Jump to next <see cref="Planet.slotTF"/>
    /// </summary>
    public void JumpNext()
    {
        ExecuteJumpNext();
        CurrentPlanet.Shake();
        CurrentPlanet.Pulse();
        OnJumped?.Invoke();

        //ScoreLabel_UI.SetFontScale(ScoreLabel_UI.GetStyle().FontScale + 30);
        Core.StartCoroutine(BounceText(2));
        ScoreLabel_UI.SetText((++Score).ToString());
        AssurePlanets();
        
    }

    IEnumerator BounceText(float scale)
    {
        float from = ScoreLabelScale;
        float to = from * scale;
        float offset = from-to;

        float fdadeinDuration = 0.07f;
        float fadeoutDuration = 0.3f;
        float elapse = 0;
        while (elapse < fdadeinDuration)
        {
            elapse += GameSceneManager.Instance.GameDeltaTime;
            //Console.WriteLine(elapse);

            var fontScale = Easings.EaseExpoOut(elapse, from, -offset, fdadeinDuration);

            ScoreLabel_UI.SetFontScale(fontScale);

            yield return null;
        }

        elapse = 0;
        while (elapse < fadeoutDuration)
        {
            elapse += GameSceneManager.Instance.GameDeltaTime;
            //Console.WriteLine(elapse);

            var bufferFontScale = Easings.EaseLinearNone(elapse, to, offset, fadeoutDuration);

            ScoreLabel_UI.SetFontScale(bufferFontScale);

            yield return null;
        }
        ScoreLabel_UI.SetFontScale(ScoreLabelScale);

    }
    IEnumerator SpaceOutAndDestroy(Planet curr,Planet prev)
    {
        
        float duration = 3f;
        float elapse = 0;
        Vector2 from = prev.Transform.Position2;
        Vector2 to = curr.Transform.Position2;
        Vector2 offset = -(to - from) * 2f;

        while (elapse < duration)
        {
            elapse += GameSceneManager.Instance.GameDeltaTime;
            //Console.WriteLine(elapse);

            var easingX = (int)Easings.EaseSineIn(elapse, from.X, offset.X, duration);
            var easingY = (int)Easings.EaseSineIn(elapse, from.Y, offset.Y, duration);

            ///Check in case entity removed but still access to current component
            prev?.Entity?.Transform?.SetPosition(new Vector2(easingX, easingY));

            yield return null;
        }

        ///Check in case entity remove but still access to current component
        if(prev.Entity != null)
            Entity.Destroy(prev.Entity);

        junpAndDes = null;
    }



    internal void CalculateSlotTransform(Player ball,float circleAmount,Planet curr,Planet next, bool clowise = false)
    {
        var r = next.Radius;
        var offset = next.centerTF.Position2 - curr.centerTF.Position2;

        Debug.Assert(circleAmount <= r*2);

        var x = circleAmount - r;
        var y = (clowise? -1:1) * MathF.Sqrt(r*r - x*x);

        var localSlotPos = new Vector2(x, y);
        var localSlotAngle = RaymathF.Vector2Randian(localSlotPos);

        //Watch for angle error
        var originSlotAngle = RaymathF.Vector2Randian(offset) - MathF.PI/2f;

        var offsetSlotAngle = localSlotAngle + originSlotAngle;
        //Console.WriteLine($"{localSlotAngle * Raylib.RAD2DEG}");

        next.centerTF.SetLocalRotationZ(offsetSlotAngle);
 

    }


    void ICustomInspectorImgui.OnInspectorGUI()
    {
        if(ImGui.Button("Reset Scene"))
        {
            ResetScene();
        }
        ImGui.SliderFloat("ballSpeed",ref BallFloatSpeed,0,1000);

        ImGui.TextUnformatted($"Currnet -> {CurrentPlanet.Entity.Name}");

        if (ImGui.BeginChild("QueueNext"))
        {

            foreach (var p in planetNextQueue)
            {
                ImGui.Text($"{p.Entity.Name}-- {p.Entity.ID}");
            }
            

            ImGui.EndChild();
        }
        ImGui.Text(Time.TimeScale.ToString());
    }

    float RandomSize()
        => RayUtils.RandF(MinPlanet, MaxPlanet);
    Vector2 RandomPosition()
    {
        var minLengthRadius = (Scene.ViewPortWidth / 2f) - 30;
        var maxLengthRadius = (Scene.ViewPortWidth / 2f);
        var position = CurrentPlanet != null ? CurrentPlanet.centerTF.Position2 : Vector2.Zero;

        return RayUtils.RandomPointInRing(position, minLengthRadius, maxLengthRadius);
    }
    void ResetScene()
    {
        var coroutineManager = Core.Instance.Managers.Find(m => typeof(CoroutineManager).IsAssignableFrom(m.GetType())) as CoroutineManager;


        var transtion = new FadeTransition(() => new SampleScene("Sample", 128, 128));


        if(!transtion.IsPlaying)
            Core.ScheduleNextFrame(null,(o)=>  Core.StartTransition(transtion,false) );
    }
}
