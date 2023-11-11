
namespace Engine.Renderering
{
    /// <summary>
    /// Allow being draw by <see cref="Renderer"/>
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Define Render Layer for <see cref="Renderer"/> to process
        /// </summary>
        public int RenderLayer { get; set; }
        public float LayerDepth { get; set; }
        /// <summary>
        /// Draw Object here
        /// </summary>
        public void Render();
    }

}