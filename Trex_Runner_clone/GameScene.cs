
using Engine;
using Engine.SceneManager;
using Engine.UI;
using Engine.Velcro;
using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Dynamics;
using Raylib_cs;

namespace Trex_runner
{
    public class GameManager : Component,IUpdatable
    {
        Label uiScore;

        public float score;
        public static GameManager Instance { get; private set; }
        public bool IsPlaying = true;
        public int UpdateOrder { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public GameManager()
        {
            if (Instance == null) Instance = this;
            else throw new ("Singleton already exist");
        }

        public override void OnAddedToEntity()
        {
            if(Scene.TryFindComponent<UICanvas>(out var uICanvas))
            {
                Table table = new Table()
                .SetFillParent(true)
                .Top().Right()
                //.DebugAll()
                .Pad(20);
                ;

                uiScore = new Label("0000", rFont.Default, Color.DARKGRAY, 20);

                table.Add(uiScore);

                uICanvas.Stage
                    .AddElement(table);
            }
        }
        public void StopGame()
        {
            if (!IsPlaying) return;


            IsPlaying = false;
            Time.TimeScale = 0f;
            if (Scene.TryFindComponent<CactusManager>(out var cactusManager))
            {
                cactusManager.StopSpawning();
            }
        }
        public void StartGame()
        {
            if (IsPlaying) return;

            IsPlaying = true;

            Time.TimeScale = 1f;
            if (Scene.TryFindComponent<CactusManager>(out var cactusManager))
            {
                cactusManager.ClearCactus();
                cactusManager.BeginSpawning();
            }
            score = 0;
        }
        public void RestartGame()
        {
            StopGame();
            StartGame();
        }

        public void Update()
        {
            if (IsPlaying)
            {
                score += Time.DeltaTime;
                uiScore.SetText( ((int)score).ToString());

            }
            if (Input.IsKeyPressed(KeyboardKey.KEY_SPACE))
            {
                if(!IsPlaying)
                    StartGame();
            }
        }

    }

    public class GameScene : Scene
    {
        #region Ctor
        public GameScene(string sceneName, int width, int height, Color backgroundColor, Color letterBoxClearColor) : base(sceneName, width, height, backgroundColor, letterBoxClearColor)
        {
        }

        public GameScene() : this("Gameplay", 800, 300, Color.WHITE, Color.BLACK)
        {
        } 
        #endregion

        float groundPad = 20;
        float playerPadLeft = 80;
        float groundColliderHeight = 50;
        public override void OnLoad()
        {
            GetOrCreateSceneComponent<VWorld2D>().World.Gravity = new Microsoft.Xna.Framework.Vector2(0,0.98f);
            Camera.offset = Resolution.ToVector2()/2f;

            //var texture = new TextureAtlas(@"Trex/100-offline-sprite.png");
            var groundTexture = rTexture.Load(@"Trex/asset/ground.png");
            var dioTexture = rTexture.Load(@"Trex/asset/dinasour.png");
            var cactusTexture = rTexture.Load(@"Trex/asset/small-cactus.png");

            // TODO: need default scale in case image doesn't work

#region Player
            var player = CreateEntity("Player");
            player.Move(-Resolution.X/2f + playerPadLeft, 0);

            /* Control */
            player.AddComponent(new VRigidBody2D())
                .SetBodyType(BodyType.Dynamic);
            player.AddComponent(new VCollisionBox(20,40)); ;
            player.AddComponent(new VCollisionCircle(5))
                .SetCenter(0, 40/2f);
            player.AddComponent(new JumpController());
            player.AddComponent(new Jumper());

            /* Render */
            player.AddComponent(new SpriteRenderer(dioTexture));
#endregion


#region Ground
            Entity ground = CreateEntity("ground");
            ground.Move(0, Resolution.Y / 2f - groundColliderHeight/2f );
            ground.AddComponent(new VRigidBody2D());
            ground.AddComponent(new VCollisionBox(Resolution.X, groundColliderHeight))
                .SetCollideGroup(Category.Cat2);

            //Render Ground
            Entity groundSprite = CreateEntity("ground-sprite");
            groundSprite.Move(0, Resolution.Y / 2f - groundTexture.Height/2f - groundColliderHeight);
            groundSprite.AddComponent(new ScrollingSpriteRenderer(groundTexture,-400,0))
                ;
#endregion

#region Cactus
            Entity cactus = CreateEntity("cactus-original");
            cactus.Move(Resolution.X/2f, Resolution.Y / 2f - cactusTexture.Height/2f- groundColliderHeight);
            cactus.Enable = false;

            cactus.AddComponent(new VRigidBody2D())
                .SetBodyType(BodyType.Kinematic);
            cactus.AddComponent(new VCollisionBox( cactusTexture.Width, cactusTexture.Height));
            cactus.AddComponent(new SpriteRenderer(cactusTexture));
            cactus.AddComponent(new Cactus());
#endregion Cactus

            Entity cactusManager = CreateEntity("manager");
            cactusManager.AddComponent(new CactusManager(cactus));

            Entity gameManager = CreateEntity("GameManager");
            gameManager.AddComponent(new GameManager());

       
            var ui = CreateEntity("ui");
            ui.AddComponent<UICanvas>();
            
        }
        public override void Update()
        {
            base.Update();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}