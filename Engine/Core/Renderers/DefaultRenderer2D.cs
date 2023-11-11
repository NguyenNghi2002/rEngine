using Engine.SceneManager;
using Raylib_cs;
using System.Numerics;

namespace Engine.Renderering
{
    public class LayerRenderer : Renderer2D
    {
        private readonly int[] layers;

        public LayerRenderer(params int[] layers)
        {
            this.layers = layers;
            Array.Sort(this.layers);
            Array.Reverse(this.layers);
        }

        public override void Render(Scene scene)
        {
            var cam = Camera2D ?? scene.Camera;
            BeginRender(cam);
            {
                foreach (IRenderable renderable in scene.Renderables)
                {

                    if (!layers.Contains(renderable.RenderLayer))
                        continue;
                    if (renderable is Component component)
                        if (!component.Enable || !component.Entity.Enable) 
                            continue;
                    renderable.Render();
                }
            }
            EndRender();
        }
    }
    public class ExcludeLayerRenderer : Renderer2D
    {
        private readonly int[] excludeLayers;

        public ExcludeLayerRenderer(params int[] excludeLayers)
        {
            this.excludeLayers = excludeLayers;
            Array.Sort(this.excludeLayers);
            Array.Reverse(this.excludeLayers);
        }

        public override void Render(Scene scene)
        {
            var cam = Camera2D ?? scene.Camera;

            BeginRender(cam);
            {
                foreach (IRenderable renderable in scene.Renderables)
                {

                    if (excludeLayers.Contains(renderable.RenderLayer)) 
                        continue;
                    if (renderable is Component component)
                        if (!component.Enable || !component.Entity.Enable) 
                            continue;
                    renderable.Render();
                }
            }
            EndRender();
        }
    }
    public class DefaultRenderer2D : Renderer2D
    {
        public override void Render(Scene scene)
        {
            var cam = Camera2D ?? scene.Camera;

            BeginRender(cam);
            {
                foreach (IRenderable renderable in scene.Renderables)
                {
                    if (renderable is Component component)
                        if (!component.Enable || !component.Entity.Enable) 
                            continue;
                    renderable.Render();
                }
            }
            EndRender();
        }
    }

}