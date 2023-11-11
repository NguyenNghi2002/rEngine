namespace Engine
{
    interface IDrawable 
    {
        /// <summary>
        /// Called on every frame
        /// </summary>
        void Draw();
        /// <summary>
        /// sort draw list as asending
        /// </summary>
        int DrawOrder { get; set; }
    }
}
