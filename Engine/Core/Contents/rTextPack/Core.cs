using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Engine.Texturepacker
{
    /// <summary>
    /// Support Xml
    /// </summary>
    public class AtlasDocument  
    {
        public string Directory { get; set; }

        protected XDocument ReadXml(string path)
        {
            Directory = Path.GetDirectoryName(path);
            XDocument xDoc = XDocument.Load(path);

            return xDoc;

        }
    }
}