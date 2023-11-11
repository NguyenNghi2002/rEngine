namespace Engine
{
    /// <summary>
    /// Customize way to import asset from file, and add to content manager
    /// </summary>
    public interface IResourceLoader
    {
        public Resource Load(string path);
    }


}