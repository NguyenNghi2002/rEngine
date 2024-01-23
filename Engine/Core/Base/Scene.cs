using Engine.Renderering;
using Raylib_cs;
using System.Diagnostics;
using System.Numerics;

namespace Engine.SceneManager
{
    public enum DesignScaling
    {
        None,LetterBox,Truncate,Fit
    }

    

    public partial class Scene
    {
        /** IDENTITIES  **/
        public string SceneName;
		public string SceneID;

        internal int screenWidth, screenHeight;
        internal float screenRatio;

        /** COMPONENTS  **/
        public Camera2D Camera;
        public Color FinalClearColor = Color.BLANK;
        public Color ClearColor;
        public DesignScaling Scaling = DesignScaling.LetterBox;
        public TextureFilter Filter 
        { set
            {
                Debug.Assert(_sceneRenderTexture.id != 0,"scene render texture have not created yet");
                Debug.Assert(FinalRenderTexture.id != 0,"final scene render texture have not created yet");
                Raylib.SetTextureFilter(_sceneRenderTexture.texture,value);
                Raylib.SetTextureFilter(FinalRenderTexture.texture,value);
                //_sceneRenderTxt.texture. Filter = value;
                //FinalRenderTexture.Filter = value;
            }
        }
        

        /// <summary>
        /// Render list be processed in Renderer
        /// </summary>
        public readonly List<Renderer> Renderers = new List<Renderer>();

        internal List<SceneComponent> _sceneComponents = new List<SceneComponent>();

        internal List<IRenderable> Renderables { get; } = new List<IRenderable>();
        internal List<IUpdatable> Updates { get; } = new List<IUpdatable>();
        internal readonly List<IFixedUpdatable> FixedUpdates = new List<IFixedUpdatable>();

        public EntityList SceneEntitiesList { set; get; } //Added in constructor

        /** RENDER INFOS **/
        RenderTexture2D _sceneRenderTexture;
        protected RenderTexture2D FinalRenderTexture { private set; get; }
        Rectangle _finalRectangle;

        internal IFinalRender FinalRenderer;

        public VectorInt2 Resolution
        {
            get=> new VectorInt2(screenWidth, screenHeight);
            set
            {
                screenWidth = value.X;
                screenHeight = value.Y;
                UpdateRenderers();
            }
            
        }

        public Scene(string sceneName, int width, int height, Color backgroundColor,Color letterBoxClearColor )
        {
            /** IDENTITIES  **/
            SceneName = sceneName;
            SceneID = Guid.NewGuid().ToString();

            this.screenWidth = width;
            this.screenHeight = height;

            /** COMPONENTS  **/
            Camera = new Camera2D(Vector2.Zero, Vector2.Zero, 0, 1f);
            ClearColor = backgroundColor;
            FinalClearColor = letterBoxClearColor;

            SceneEntitiesList = new EntityList(this);
            
            /** EVENTS  **/
            Core.Instance.OnWindowSizeChanged += UpdateRenderers;

        }

        void UpdateRenderers()
        {
            for (int i = 0; i < Renderers.Count; i++)
                Renderers[i].UpdateRenderTexture(this);

            //_screenRatio = _screenWidth / _screenHeight;

            _finalRectangle = GetFinalResolution();

        }



        #region Core
        /// <summary>
        /// Set up scene texture, check , 
        /// </summary>
        public virtual void Begin()
        {
            OnLoad();
             
            /** RENDERER  **/
            FinalRenderTexture = Raylib.LoadRenderTexture(screenWidth, screenHeight);
            _sceneRenderTexture = Raylib.LoadRenderTexture(screenWidth, screenHeight);

            Filter = TextureFilter.TEXTURE_FILTER_BILINEAR;
            //FinalRenderTexture.SetFilter(TextureFilter.TEXTURE_FILTER_BILINEAR);
            //_sceneRenderTxt.SetFilter(TextureFilter.TEXTURE_FILTER_BILINEAR);

            UpdateRenderers();
            if (Renderers.Count == 0)
            {
                AddRenderer(new DefaultRenderer2D());
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ ! ] scene has has no renderer, A default renderer has added");
                Console.ResetColor();
            }
            OnBegined();

            /** Process all adding/removing request list **/
            SceneEntitiesList.PopEntityRequests();
        }
        public virtual void End()
        {
            foreach (var renderer in Renderers)
                renderer.Unload();

            SceneEntitiesList.Clear();

            Renderers.Clear();
            Updates.Clear();
            FixedUpdates.Clear();

            _sceneComponents.ForEach((c) => c.OnRemoveFromScene());
            _sceneComponents.Clear();

            Raylib.UnloadRenderTexture(FinalRenderTexture);
            Raylib.UnloadRenderTexture(_sceneRenderTexture);

            SceneEntitiesList = null;

            OnUnload();

            GC.Collect();
        }
        internal void EarlyUpdate()
        {
            /** Process all adding/removing request list **/
            ///Adding will called onadd to scene function
            SceneEntitiesList.PopEntityRequests();


        }
        public virtual void FixedUpdate()
        {
            _sceneComponents.ForEach((cpn) => cpn.FixedUpdate());
            SceneEntitiesList.FixedUpdateEntities();
        }
        public virtual void Update()
        {

            _sceneComponents.ForEach((cpn) => cpn.Update());

            /** Update All Entities **/
            SceneEntitiesList.UpdateEntities();

        }
        public virtual void Render()
        {
            if (Renderers.Count == 0)
            {
                Console.WriteLine("No Renderers");
                return;
            }
            //Update each renderer
            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderer renderer = Renderers[i];
                renderer.Render(this);
            }

            Raylib.BeginTextureMode(FinalRenderTexture);
            Raylib.ClearBackground(ClearColor);
#region Update Final render buffer
            //Draw Renderer into finalRenderer
            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderer renderer = Renderers[i];
                Rectangle src = new Rectangle()
                {
                    x = 0,
                    y = 0,
                    width =     renderer.RenderTexture.Value.texture.width,
                    height = - renderer.RenderTexture.Value.texture.height, // Flip for opengl coordinate reason
                };
                Raylib.DrawTexturePro(renderer.RenderTexture.Value.texture, src, new Rectangle(0, 0, screenWidth, screenHeight), Vector2.Zero, 0, Color.WHITE);
                OnRender();
            }

            Raylib.BeginMode2D(Camera);
            Debugging.DrawEntityDebug(this);
            Raylib.EndMode2D();
#endregion
            Raylib.EndTextureMode();
        } 
        #endregion

        internal void FinalRender()
        {
            if(FinalRenderer != null)
            {
                FinalRenderer.HandleFinalRender(this);
                return;
            }


            var src = new Rectangle()
            {
                x = 0,
                y = 0,

                width = FinalRenderTexture.texture.width,
                height = -FinalRenderTexture.texture.height,
            };
            Raylib.DrawTexturePro(FinalRenderTexture.texture,
                src,
                _finalRectangle, Vector2.Zero,
                0, Color.WHITE
                );
            Raylib.DrawFPS((int)(_finalRectangle.x + 10), (int)(_finalRectangle.y + 10));

        }
        public Rectangle GetFinalResolution()
        {
            var windowWidth = Raylib.GetScreenWidth();
            var windowHeight = Raylib.GetScreenHeight();

            switch (Scaling)
            {
                case DesignScaling.None:

                    return new Rectangle()
                    {
                        x = 0,
                        y = 0,
                        width = FinalRenderTexture.texture.width,
                        height = FinalRenderTexture.texture.height,
                    };
                case DesignScaling.LetterBox:

                    screenRatio = Math.Min((float)Raylib.GetScreenWidth() / screenWidth, (float)Raylib.GetScreenHeight() / screenHeight);
                    return new Rectangle()
                    {
                        x = (windowWidth - (screenWidth * screenRatio)) * 0.5f,
                        y = (windowHeight - (screenHeight * screenRatio)) * 0.5f,
                        width = screenWidth * screenRatio,
                        height = screenHeight * screenRatio,
                    };

                case DesignScaling.Truncate:
                    screenRatio = Math.Max((float)Raylib.GetScreenWidth() / screenWidth, (float)Raylib.GetScreenHeight() / screenHeight);
                    return new Rectangle()
                    {
                        x = (windowWidth - (screenWidth * screenRatio)) * 0.5f,
                        y = (windowHeight - (screenHeight * screenRatio)) * 0.5f,
                        width = screenWidth * screenRatio,
                        height = screenHeight * screenRatio,
                    };
                case DesignScaling.Fit:

                    return new Rectangle()
                    {
                        x = 0,
                        y = 0,
                        width = windowWidth,
                        height = windowHeight,
                    };
            }
            return new Rectangle();
            
        }
        public Vector2 WindowSpaceToViewSpace(Vector2 point)
        {
            var core = Core.Instance;
            var scene = Core.Scene;

            switch (scene.Scaling)
            {
                case DesignScaling.None:
                    return point;
                case DesignScaling.LetterBox:
                    return new Vector2()
                    {
                        X = (point.X - (core.WindowWidth - (scene.screenWidth * scene.screenRatio)) * 0.5f) / scene.screenRatio,
                        Y = (point.Y - (core.WindowHeight - (scene.screenHeight * scene.screenRatio)) * 0.5f) / scene.screenRatio,
                    };
                case DesignScaling.Truncate:
                    return new Vector2()
                    {
                        X = (point.X - (core.WindowWidth - (scene.screenWidth * screenRatio)) * 0.5f) / screenRatio,
                        Y = (point.Y - (core.WindowHeight - (scene.screenHeight * screenRatio)) * 0.5f) / screenRatio,
                    };
                case DesignScaling.Fit:
                    return new Vector2()
                    {
                        X = (point.X - (core.WindowWidth - (scene.screenWidth * scene.screenRatio)) * 0.5f) / scene.screenRatio,
                        Y = (point.Y - (core.WindowHeight - (scene.screenHeight * scene.screenRatio)) * 0.5f) / scene.screenRatio,
                    };
                default:
                    return point;
            }
        }
        
        public Vector2 ViewSpaceToWindowSpace(Vector2 point)
        {
            var core = Core.Instance;
            var scene = Core.Scene;
            var c = (core.WindowWidth - (scene.screenWidth * screenRatio));

            var a = new Vector2()
            {
                X = (point.X * screenRatio  + ( core.WindowWidth - (scene.screenWidth  * screenRatio)) * 0.5f),
                Y = (point.Y * screenRatio + (core.WindowHeight - (scene.screenHeight * screenRatio)) * 0.5f),
            };
            return a;
        }

        


        public void AddRenderer(Renderer renderer)
        {
            Renderers.Add(renderer);
            renderer.OnAddedToScene(this);
        }
        public bool RemoveRenderer(Renderer renderer)
        {
            var removed =  Renderers.Remove(renderer);
            if(removed) renderer.OnRemovedFromScene(this);
            return removed;
        }


        public T AddSceneComponent<T>()
            where T : SceneComponent, new()
            => AddSceneComponent<T>(new T());
        public T AddSceneComponent<T>(T component) 
            where T : SceneComponent
        {
            _sceneComponents.Add(component);
            component.Scene = this;
            component.OnAddedToScene();
            return component;
        }
        public void RemoveSceneComponent<T>()
            where T : SceneComponent
        {
            var component = GetSceneComponent<T>();

            if (component != null)
                RemoveSceneComponent(component);
        }
        public void RemoveSceneComponent(SceneComponent component)
        {
            Insist.IsTrue(_sceneComponents.Contains(component),
                "SceneComponent{0} is not availaible");
            component.OnRemoveFromScene();
            component.Scene = null;
            _sceneComponents.Remove(component);

        }
        public T? GetSceneComponent<T>()
            where T : SceneComponent
        {
            return _sceneComponents.Find((c) => c is T) as T;
        }
        public T GetOrCreateSceneComponent<T>() where T : SceneComponent,new()
        {
            var cpn = GetSceneComponent<T>();
            if (cpn == null)
                cpn = AddSceneComponent<T>();
            return cpn;
        }


        ~Scene()
        {
            Debugging.Log("scene {0} ended", Debugging.LogLevel.Comment, this);
        }
    }



}