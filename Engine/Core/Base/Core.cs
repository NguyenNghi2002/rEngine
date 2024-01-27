using Engine.Renderering;
using Engine.SceneManager;
using Engine.Timer;
using Engine.Utilities;
using Raylib_cs;
using System.Collections;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
namespace Engine
{
    /// <summary>
    /// Application Core
    /// Deprive this class to your application
    /// </summary>
    public class Core 
    {
        public static Core Instance;

        public event Action OnWindowSizeChanged;
        public event Action OnWindowPositionChanged;

        public float ClampFixedUpdate = 60;

        Scene _scene;
        Scene _queueScene;
        bool requestExit = false;

        public List<GlobalManager> Managers = new List<GlobalManager>();


        /** BASIC MANAGERs **/
        TimerManager timerManager = new TimerManager();
        CoroutineManager coroutineManager = new CoroutineManager();
        ContentManager contentManager = new ContentManager();


        public ICoroutine TransitionCoroutine;
        internal Transition _sceneTransition;
        public static Scene Scene
        {
            get => Instance._scene;
            set
            {
                Insist.IsNotNull(value);
                if(Instance._scene == null)
                {
                    Instance._scene = value;
                    Instance._scene.Begin();
                }
                else
                {
                    Instance._queueScene = value;
                }
            }
        }

        #region private fields

        private bool minSizeDirty = false;
        private bool sizeDirty = false;
        private bool positionDirty = false;

        private string _title;
        private int _width , _height;
        private int _minWidth , _minHeight ;

        private int _x, _y;
        private float accumulator;
        private Vector2 _prevWindowPosition;
        #endregion

        public Core() :this(1280,720,"Raylib"){ }
        public Core(int width ,int height,string title)
        {
            Instance ??= this;

            _title = title;
            _width = width;
            _height = height;

            //Managers

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Run()
        {
            Instance ??= this;
            Insist.IsNotNull(Instance);

            
            this.Initialize();
            while (!Raylib.WindowShouldClose() && !requestExit)
            {
                #region Window Orientation Triggers
                if (Raylib.IsWindowResized())
                {
                    SetWidth(Raylib.GetScreenWidth());
                    SetHeight(Raylib.GetScreenHeight());
                    // trigger sizeChange = true
                }
                if (Raylib.GetWindowPosition() != new Vector2(_x,_y))
                {
                    var pos = Raylib.GetWindowPosition();
                    SetX((int)pos.X);
                    SetY((int)pos.Y);

                }

                if (sizeDirty)
                {
                    Raylib.SetWindowSize(WindowWidth, WindowHeight);
                    OnWindowSizeChanged?.Invoke();
                    sizeDirty = false;
                    Console.WriteLine("size changed");
                }
                if (positionDirty)
                {
                    Raylib.SetWindowPosition(WindowPositionX, WindowPositionY);
                    OnWindowPositionChanged?.Invoke();
                    positionDirty = false;
                    Console.WriteLine("Position changed");
                }
                if (minSizeDirty)
                {
                    
                    Raylib.SetWindowMinSize(WindowMinWidth, WindowMinHeight);
                    //On Window Min size changed event ?
                    minSizeDirty = false;
                    Console.WriteLine("min size changed");
                }

                #endregion
                Update();
                Render();
            }
            Shutdown();

        }

        #region Getters & Setters
        public void SetTitle(string windowTitle)
        {
            Raylib.SetWindowTitle(windowTitle);
            _title = windowTitle;
        }

        public void SetHeight(int height)
        {
            _height = height;
            sizeDirty = true;
        }
        public void SetWidth(int width)
        {
            _width = width;
            sizeDirty = true;
        }

        public void SetMinWidth(int minWidth)
        {
            _minWidth = minWidth;
            minSizeDirty = true;
        }
        public void SetMinHeight(int minHeight)
        {
            _minHeight = minHeight;
            minSizeDirty = true;
        }

        public void SetX(int x)
        {
            _x = x;
            positionDirty = true;
        }
        public void SetY(int y)
        {
            _y = y;
            positionDirty = true;
        }
        public void SetExitKey(KeyboardKey keyboardKey)
            =>Raylib.SetExitKey(keyboardKey);
        #endregion

        #region ICore
        public string Title
        {
            get => _title;
            set => SetTitle(value);
        }
        public int WindowWidth
        {
            get => _width;
            set => SetWidth(value);
        }
        public int WindowHeight
        {
            get => _height;
            set => SetHeight(value);
        }
        public int WindowMinWidth
        {
            get => _minWidth;
            set => SetMinWidth(value);
        }
        public int WindowMinHeight
        {
            get => _minHeight;
            set => SetMinHeight(value);
        }
        public int WindowPositionX
        {
            get => _x;
            set => SetX(value);
        }
        public int WindowPositionY
        {
            get => _y;
            set => SetY(value);
        }
        #endregion


        #region LifeCycle
        /// <summary>
        /// Setup <see cref="WindowWidth"/>,<see cref="WindowHeight"/> and <see cref="ConfigFlags"/>. <br/>
        /// Default window and <see cref="GlobalManager"/>:
        /// 
        /// <list type="bullet">
        /// <item><see cref="ContentManager"/></item>
        /// <item><see cref="TimerManager"/></item>
        /// <item><see cref="CoroutineManager"/></item>
        /// </list><br/>
        /// 
        /// NOTE: <see cref="Initialize()"/> must be called first.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Initialize()
        {
            ///
            minSizeDirty = sizeDirty = positionDirty = true;
            Raylib.InitAudioDevice();
            Raylib.InitWindow(WindowWidth, WindowHeight, Title);

            _x = (int)Raylib.GetWindowPosition().X;
            _y = (int)Raylib.GetWindowPosition().Y;

            // Set up default Manager
            AddManager(coroutineManager);
            AddManager(timerManager);
            AddManager(contentManager);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Shutdown()
        {
            _scene?.End();
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
        }

        private void GetFixedLoopCount(out int loopCount)
        {
            loopCount = 0;
            if (Time.TimeScale == 0) return;
            accumulator += Raylib.GetFrameTime();
            accumulator = Math.Min(accumulator, ClampFixedUpdate);
            while (accumulator > Time.FixedDeltaTime )
            {
                loopCount++;
                accumulator -= Time.FixedDeltaTime;
            }
        }

        /// <summary>
        /// Base update:
        /// <list type="number" >
        /// <item>Fixed Update</item>
        /// <item>Update</item>
        /// <item>Scene process</item>
        /// </list>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Update()
        {
            GetFixedLoopCount(out int loopCount);

            /** FixedUpdate Managers  **/
            for (int j = 0; j < loopCount; j++)
                for (int i = 0; i < Managers.Count; i++)
                {
                    var manager = Managers[i];

                    if (manager is IFixedUpdatable fixedUpdatable && manager.Enable)
                        fixedUpdatable.FixedUpdate();

                    _scene?.FixedUpdate();
                }


            /** Update Managers  **/
            for (int i = 0; i < Managers.Count; i++)
            {
                var manager = Managers[i];
                if (manager is IUpdatable updatable && manager.Enable)
                    updatable.Update();
            }

            /** Processing scene **/
            if(_scene != null)
            {
                _scene.EarlyUpdate();
                if(_queueScene != null)
                {
                    if(!_scene.NoEnd) 
                        _scene.End();

                    _scene = _queueScene;
                    _queueScene = null;
                    GC.Collect();

                    if(!_scene.NoBegin) 
                        _scene.Begin();
                    // Scene change event
                }

                for (int i = 0; i < loopCount; i++)
                    _scene.FixedUpdate();
                _scene.Update(); 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Render()
        {
          
            _scene?.Render();


            Raylib.BeginDrawing();
            Raylib.ClearBackground(_scene != null ?_scene.FinalClearColor : Color.BLACK);

            #region Final Draw

            if (_scene == null)
            {
                var fontSize = 40;
                var spacing = 10f;
                var prompt = "Empty Scene";
                var font = Raylib.GetFontDefault();
                var textScale = Raylib.MeasureTextEx(font,prompt,fontSize,spacing);
                var position = new Vector2(WindowWidth,WindowHeight)/2f;
                Raylib.DrawTextPro(font, prompt, position,textScale/2f,0f,fontSize,spacing,Color.GRAY);
            }
            else
            {
                ///Render scene
                _scene.FinalRender();

                ///Render Transition on top of Scene
                if(_sceneTransition != null )
                {
                    if(_scene != null && !_sceneTransition.IsPlaying)
                    {
                        StartCoroutine(_sceneTransition.OnBegin());
                        _sceneTransition.IsPlaying = true;
                    }
                    _sceneTransition.Render();

                }
            }
            //Raylib.DrawCircleV(Raylib.GetMousePosition(),200,Color.RED);

            #endregion Final Draw



            foreach (var m in Managers.Where(m=>m.Enable))
            {
                m.OnDrawDebug();
            }
            Raylib.EndDrawing();
        }
        #endregion
        public void AddManager(GlobalManager manager)
        {

            Managers.Add(manager);
        }
        public bool RemoveManager(GlobalManager manager) => Managers.Remove(manager);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITimer Schedule(float waitInSecond, bool repeat, object context, Action<ITimer> onTimeout)
            => Instance.timerManager.Schedule(waitInSecond, repeat, context, onTimeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ITimer ScheduleNextFrame(object context, Action<ITimer> onTimeout)
            => Instance.timerManager.Schedule(0.0f, false, context, onTimeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICoroutine? StartCoroutine(IEnumerator enumerator)
            => Instance.coroutineManager.StartCoroutine(enumerator);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T StartTransition<T>(T transition,bool allowMultiple = false)where T : Transition
        {
            ///If Transition is playing, and new transition is request
            /// then new transition will be cancle
            if (!allowMultiple && Core.Instance._sceneTransition != null) return transition;

            Insist.IsNotNull(transition,"Transition can not be NULL");
            Core.Instance._sceneTransition = transition;
            return transition;
        }

        /// <summary>
        /// Programatic way to exit application
        /// </summary>
        public void ExitApp() => requestExit = true;

    }

}