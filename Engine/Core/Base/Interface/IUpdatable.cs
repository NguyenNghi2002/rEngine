namespace Engine
{
    public interface IFixedUpdatable 
    {
        /// <summary>
        /// Order to sort Updatable component list
        /// </summary>
        int UpdateOrder { get; set; }
        /// <summary>
        /// Call on every frame
        /// </summary>
        void FixedUpdate();

    }
    public interface IUpdatable
    {
        /// <summary>
        /// Call on every frame
        /// </summary>
        void Update();

        /// <summary>
        /// Order to sort Updatable component list
        /// </summary>
        // TODO: Havent implimentented Sorted yet
        int UpdateOrder { get; set; }
    }

}
