using Engine.Renderering;

namespace Engine
{
    public class RenderableAscendingOrderComparer : IComparer<IRenderable>
    {
        /// Greater than zero = closer to begin of list
        /// Less than zero = closer to end of list
        public int Compare(IRenderable? x, IRenderable? y)
        {
            return x.LayerDepth.CompareTo(y.LayerDepth);
        }
    }

    public abstract class RenderableComponent : Component, IRenderable
    {
        public static readonly RenderableAscendingOrderComparer RenderComparer = new RenderableAscendingOrderComparer();

        public int RenderLayer { get; set; }
        public float LayerDepth
        {
            get => _layerDepth;
            set => SetRenderOrder(value);
        }
        /// <summary>
        /// default order is 0, if same depth, then it will compare is creation order
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public RenderableComponent SetRenderOrder(float depth)
        {
            _layerDepth = depth;
            Core.ScheduleNextFrame(null, (timer) =>
            {
                Scene.Renderables.Sort(RenderComparer);
            });
            return this;
        }

        private float _layerDepth = 0;

        public abstract void Render();
    }
}