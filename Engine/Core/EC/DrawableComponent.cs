#if false
using System.Diagnostics;

namespace Engine
{
    public sealed class DrawableComponentComparer : IComparer<DrawableComponent>
    {
        public int Compare(DrawableComponent? x, DrawableComponent? y)
        {
            return x.CompareTo(y);
        }
    }
    public abstract class DrawableComponent : Component, IDrawable, IComparable<DrawableComponent>
    {
        static readonly IComparer<DrawableComponent> comparer = new DrawableComponentComparer();
        private int _drawOrder = 0;
        public int DrawOrder
        {
            get => _drawOrder;
            set => SetDrawOrder(value);
        }
        /// <summary>
        /// function can't be call in initialize since entity didn't attached
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public DrawableComponent SetDrawOrder(int order)
        {
            if (_drawOrder != order)
            {
                _drawOrder = order;
            }
            return this;
        }
        public int CompareTo(DrawableComponent? other)
        {
            Insist.IsNotNull(other, $"{other} is null");

            return DrawOrder.CompareTo(other.DrawOrder);
        }

        /// <summary>
        /// <inheritdoc/>
        /// override this to sub class, <br/>no need to call <see cref="Draw"/> in base class
        /// </summary>
        public virtual void Draw()
        { }

        public virtual void OnGUI()
        { }

    }
}

#endif