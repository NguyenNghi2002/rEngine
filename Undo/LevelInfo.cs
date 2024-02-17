using Engine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;


[XmlRoot]
public class LevelInfo
{
        public string Name = string.Empty;
        public bool AllowReplay = false;
}
