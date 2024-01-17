namespace Engine
{
    /// <summary>
    /// Customize way to import asset from file, and add to content manager
    /// </summary>
    public interface IResourceHandler
    {
        public object Load(string path);
        public void Unload(object resource);
    }


}