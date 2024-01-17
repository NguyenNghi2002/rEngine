using Engine.SceneManager;
using Raylib_cs;
using System.Numerics;

namespace Engine.Renderering
{
    public abstract class Renderer 
    {
        protected RenderTexture2D? _renderTexture;
        public RenderTexture2D? RenderTexture => _renderTexture;


        public Material? Material;
        public Color RenderClearColor = Color.BLANK;
        public BlendMode BlendMode = BlendMode.BLEND_ALPHA;


        public virtual void OnAddedToScene(Scene scene) { }

        public virtual void OnRemovedFromScene(Scene scene){ }

        public virtual void UpdateRenderTexture(Scene scene)
        {
            /// If Rendertexture has value then replace with new scene scale
            /// if not then create rendertexture
            /// When Rendertexture has value , if scene Scale still same with
            /// renderTexture scale then Return
            if (_renderTexture != null)
            {
                var newWidth = scene.screenWidth;
                var newHeight = scene.screenHeight;
                var currentWidth = _renderTexture.Value.texture.width;
                var currentHeight = _renderTexture.Value.texture.height;
                if (currentWidth == newWidth && currentHeight == newHeight)
                    return;

                Raylib.UnloadRenderTexture(_renderTexture.Value);
                _renderTexture = null;
            }
            _renderTexture = Raylib.LoadRenderTexture(scene.screenWidth, scene.screenHeight);
            //RenderTexture =  Raylib.LoadRenderTexture(scene.sceneWidth,scene.sceneHeight);
        }

        public abstract void Render(Scene scene);
        public virtual void Unload()
        {
            if (_renderTexture != null)
            {
                Raylib.UnloadRenderTexture(_renderTexture.Value);
                _renderTexture = null;
            }
        }
        #region Utilities
        protected void HandleBeginBlendingAndMaterial()
        {
            if (Material.HasValue) Raylib.BeginShaderMode(Material.Value.shader);
            Raylib.BeginBlendMode(BlendMode.BLEND_ALPHA);
        }
        protected void HandleEndBlendingAndMaterial()
        {
            Raylib.EndBlendMode();
            if (Material.HasValue)
            {
                Raylib.EndShaderMode();
            }

        } 
        #endregion
    }

    public abstract class Renderer2D :Renderer
    {
        public Camera2D? Camera2D = null;

        protected virtual void BeginRender(Camera2D cam2D)
        {
            if (_renderTexture == null) return;
            Raylib.BeginTextureMode(_renderTexture.Value);
            Raylib.BeginMode2D(cam2D);
            Raylib.ClearBackground(RenderClearColor);
            HandleBeginBlendingAndMaterial();
        }
        protected virtual void EndRender()
        {
            if (_renderTexture == null) return;
            HandleEndBlendingAndMaterial();
            Raylib.EndMode2D();
            Raylib.EndTextureMode();
        }

        public override void OnAddedToScene(Scene scene)
        {
            _renderTexture = Raylib.LoadRenderTexture(scene.screenWidth, scene.screenHeight);
        }

        
            

        public abstract override void Render(Scene scene);

    }

    public abstract class Renderer3D : Renderer
    {
        public Camera3D? Camera3D = new Camera3D(new Vector3(0,10,10),Vector3.Zero,Vector3.UnitY,45f,CameraProjection.CAMERA_PERSPECTIVE);
        protected virtual void BeginRender(Camera3D cam3D)
        {
            if (_renderTexture == null) return;

            Raylib.BeginTextureMode(_renderTexture.Value);
            Raylib.BeginMode3D(cam3D);
            Raylib.ClearBackground(RenderClearColor);
            HandleBeginBlendingAndMaterial();
        }

        protected virtual void EndRender()
        {
            if (_renderTexture == null) return;

            HandleEndBlendingAndMaterial();
            Raylib.EndMode3D();
            Raylib.EndTextureMode();
        }

    }

}