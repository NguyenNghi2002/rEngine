namespace Engine.SceneManager
{
    public class SceneComponent : IComparable<SceneComponent>
    {
        /// <summary>
        /// Component that belong to
        /// </summary>
        public Scene Scene;

        #region Properties
        public bool Enable
        {
            get => _enable;
            set => SetEnable(value);
        }

        public int UpdateOrder
        {
            get => _updateOrder;
            set => SetUpdateOrder(value);
        } 
        #endregion

        int _updateOrder;
        bool _enable = true;


        #region Setters
        public SceneComponent SetEnable(bool value)
        {
            if (_enable != value)
            {
                _enable = value;
                if (_enable) OnEnabled();
                else OnDisabled();
            }
            return this;
        }
        public SceneComponent SetUpdateOrder(int order)
        {
            if (UpdateOrder != order)
            {
                UpdateOrder = order;
                Core.Scene._sceneComponents.Sort();
            }

            return this;
        } 
        #endregion
        public int CompareTo(SceneComponent? other)
            => this.UpdateOrder.CompareTo(other.UpdateOrder);

        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }
        public virtual void OnRemoveFromScene() { }
        public virtual void OnAddedToScene() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }
}