namespace Engine.SceneManager
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Scene
    {
        /// <summary>
        /// load your asset
        /// </summary>
        public virtual void OnLoad() { }

        /// <summary>
        /// Create your entities and add component using <see cref="CreateEntity(string, System.Numerics.Vector2)"/>
        /// </summary>
        public virtual void OnBegined() { }

        public virtual void OnRender() { }

        /// <summary>
        /// UnLoad your asset when this scene here
        /// </summary>
        public virtual void OnUnload() { }
    }
}
