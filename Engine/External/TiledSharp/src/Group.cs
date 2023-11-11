using System;
using System.Linq;
using System.Numerics;
using System.Xml;
using System.Xml.Linq;

namespace Engine.TiledSharp
{
    public class TmxGroup : ITmxLayer
    {
        public TmxMap Map;
        public string Name { get; private set; }

        public double Opacity { get; private set; }
        public bool Visible { get; private set; }
        public double OffsetX { get; private set; }
        public double OffsetY { get; private set; }
        public double ParallaxFactorX { get; private set; }
        public double ParallaxFactorY { get; private set; }
        public TmxColor Color { get; private set; }

        public Vector2 ParallaxFactor => new Vector2((float)ParallaxFactorX, (float)ParallaxFactorY);
        public Vector2 Offset => new Vector2((float)OffsetX, (float)OffsetX);


        public TmxList<ITmxLayer> Layers { get; private set; }

        public TmxList<TmxLayer> TileLayers { get; private set; }
        public TmxList<TmxObjectGroup> ObjectGroups { get; private set; }
        public TmxList<TmxImageLayer> ImageLayers { get; private set; }
        public TmxList<TmxGroup> Groups { get; private set; }
        public PropertyDict Properties { get; private set; }

        public TmxGroup(TmxMap map,XElement xGroup, int width, int height, string tmxDirectory)
        {
            Map = map;
            Name = (string)xGroup.Attribute("name") ?? String.Empty;
            Opacity = (double?)xGroup.Attribute("opacity") ?? 1.0;
            Visible = (bool?)xGroup.Attribute("visible") ?? true;
            OffsetX = (double?)xGroup.Attribute("offsetx") ?? 0.0;
            OffsetY = (double?)xGroup.Attribute("offsety") ?? 0.0;
            ParallaxFactorX = (double?)xGroup.Attribute("parallaxx") ?? 1.0;
            ParallaxFactorY = (double?)xGroup.Attribute("parallaxy") ?? 1.0;

            var colorAttr = xGroup.Attribute("color");
            Color = colorAttr != null ? new TmxColor(colorAttr) : TmxColor.Default;

            Properties = new PropertyDict(xGroup.Element("properties"));

            Layers = new TmxList<ITmxLayer>();
            TileLayers = new TmxList<TmxLayer>();
            ObjectGroups = new TmxList<TmxObjectGroup>();
            ImageLayers = new TmxList<TmxImageLayer>();
            Groups = new TmxList<TmxGroup>();
            foreach (var e in xGroup.Elements().Where(x => x.Name == "layer" || x.Name == "objectgroup" || x.Name == "imagelayer" || x.Name == "group"))
            {
                ITmxLayer layer;
                switch (e.Name.LocalName)
                {
                    case "layer":
                        var tileLayer = new TmxLayer(Map,e, width, height);
                        layer = tileLayer;
                        TileLayers.Add(tileLayer);
                        break;
                    case "objectgroup":
                        var objectgroup = new TmxObjectGroup(Map,e);
                        layer = objectgroup;
                        ObjectGroups.Add(objectgroup);
                        break;
                    case "imagelayer":
                        var imagelayer = new TmxImageLayer(Map,e, tmxDirectory);
                        layer = imagelayer;
                        ImageLayers.Add(imagelayer);
                        break;
                    case "group":
                        var group = new TmxGroup(Map,e, width, height, tmxDirectory);
                        layer = group;
                        Groups.Add(group);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                Layers.Add(layer);
            }
        }
    }
}
