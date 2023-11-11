// Distributed as part of TiledSharp, Copyright 2012 Marshall Ward
// Licensed under the Apache License, Version 2.0
// http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Diagnostics;

namespace Engine.TiledSharp
{
    public partial class TmxLayer
    {
        public TmxLayerTile GetTile(int x, int y)
            =>Tiles[x + y * Map.Width];
    }

    /// <summary>
    /// Tile layer 
    /// </summary>
    public partial class TmxLayer : ITmxLayer
    {
        public readonly TmxMap Map;
        public string Name {get; private set;}

        // TODO: Legacy (Tiled Java) attributes (x, y, width, height)

        #region ITmxLayer 
        public double Opacity { get; private set; }
        public bool Visible { get; private set; }
        public double OffsetX { get; private set; }
        public double OffsetY { get; private set; }
        public double ParallaxFactorX { get; private set; }
        public double ParallaxFactorY { get; private set; }
        public TmxColor Color { get; private set; }
        #endregion


        public Collection<TmxLayerTile> Tiles { get; private set;}
        public PropertyDict Properties { get; private set;}

        public TmxLayer(TmxMap map,XElement xLayer, int width, int height)
        {
            Map = map;
            Name = (string) xLayer.Attribute("name");
            Opacity = (double?) xLayer.Attribute("opacity") ?? 1.0;
            Visible = (bool?) xLayer.Attribute("visible") ?? true;
            OffsetX = (double?) xLayer.Attribute("offsetx") ?? 0.0;
            OffsetY = (double?) xLayer.Attribute("offsety") ?? 0.0;
            ParallaxFactorX = (double?) xLayer.Attribute("parallaxx") ?? 1.0;
            ParallaxFactorY = (double?) xLayer.Attribute("parallaxy") ?? 1.0;

            var colorAttr = xLayer.Attribute("tintcolor");
            Color = colorAttr != null ? new TmxColor(colorAttr) : TmxColor.Default;

            var xData = xLayer.Element("data");
            var encoding = (string)xData.Attribute("encoding");

            Tiles = new Collection<TmxLayerTile>();
            IEnumerable<XElement> xChunks = xData.Elements("chunk").ToList();
            if (xChunks.Any())
            {
                foreach (XElement xChunk in xChunks)
                {
                    int chunkWidth = (int)xChunk.Attribute("width");
                    int chunkHeight = (int)xChunk.Attribute("height");
                    int chunkX = (int)xChunk.Attribute("x");
                    int chunkY = (int)xChunk.Attribute("y");
                    ReadChunk(chunkWidth, chunkHeight, chunkX, chunkY, encoding, xChunk);
                }
            }
            else
            {
                ReadChunk(width, height, 0, 0, encoding, xData);
            }

            Properties = new PropertyDict(xLayer.Element("properties"));
        }

        private void ReadChunk(int width, int height, int startX, int startY, string encoding, XElement xData)
        {
            if (encoding == "base64")
            {
                var decodedStream = new TmxBase64Data(xData);
                var stream = decodedStream.Data;

                using (var br = new BinaryReader(stream))
                    for (int j = 0; j < height; j++)
                    for (int i = 0; i < width; i++)
                        Tiles.Add(new TmxLayerTile(Map,br.ReadUInt32(), i + startX, j + startY));
            }
            else if (encoding == "csv")
            {
                var csvData = (string) xData.Value;
                int k = 0;
                foreach (var s in csvData.Split(new[] {',', '\n'}, StringSplitOptions.RemoveEmptyEntries))
                {
                    var gid = uint.Parse(s.Trim());
                    var x = k % width;
                    var y = k / width;
                    Tiles.Add(new TmxLayerTile(Map,gid, x + startX, y + startY));
                    k++;
                }
            }
            else if (encoding == null)
            {
                int k = 0;
                foreach (var e in xData.Elements("tile"))
                {
                    var gid = (uint?) e.Attribute("gid") ?? 0;

                    var x = k % width;
                    var y = k / width;

                    Tiles.Add(new TmxLayerTile(Map, gid, x + startX, y + startY));
                    k++;
                }
            }
            else throw new Exception("TmxLayer: Unknown encoding.");
        }
    }

    public class TmxLayerTile
    {
        // Tile flip bit flags
        const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
        const uint FLIPPED_VERTICALLY_FLAG   = 0x40000000;
        const uint FLIPPED_DIAGONALLY_FLAG   = 0x20000000;

        public TmxTileset Tileset;
        public int Gid;

        /// <summary>
        /// Horizontal Location
        /// </summary>
        public int X;

        /// <summary>
        /// Verticle Location
        /// </summary>
        public int Y;

        public bool HorizontalFlip;
        public bool VerticalFlip;

        /// <summary>
        /// Horizontal Flip and then Rotate 90 Counter-ClockWise. IN ORDER
        /// </summary>
        public bool DiagonalFlip;

        int? _tilesetTileIndex;

        /// <summary>
        /// Return Tile in <see cref="TmxTileset"/>, If Tile is has nothing but source image draw, then it return null
        /// </summary>
        public TmxTilesetTile GetTilesetTile()
        {
            System.Diagnostics.Debug.Assert(Gid > 0, "invalid Gid, must be larger than zero");
            if (!_tilesetTileIndex.HasValue)
            {
                _tilesetTileIndex = -1;
                if(Tileset.FirstGid <= Gid)
                {
                    var index = this.Gid - Tileset.FirstGid;
                    if(Tileset.Tiles.ContainsKey(index))
                    {
                        _tilesetTileIndex = index;
                    }
                }
            }
            if (_tilesetTileIndex.Value < 0) return null; 
            return Tileset.Tiles[_tilesetTileIndex.Value];
        }
        public TmxLayerTile(TmxMap map,uint id, int x, int y)
        {
            var rawGid = id;
            X = x;
            Y = y;

            // Scan for tile flip bit flags
            bool flip;

            flip = (rawGid & FLIPPED_HORIZONTALLY_FLAG) != 0;
            HorizontalFlip = flip;

            flip = (rawGid & FLIPPED_VERTICALLY_FLAG) != 0;
            VerticalFlip = flip;

            flip = (rawGid & FLIPPED_DIAGONALLY_FLAG) != 0;
            DiagonalFlip = flip;

            // Zero the bit flags
            rawGid &= ~(FLIPPED_HORIZONTALLY_FLAG |
                        FLIPPED_VERTICALLY_FLAG |
                        FLIPPED_DIAGONALLY_FLAG);

            // Save GID remainder to int
            Gid = (int)rawGid;
            Tileset = map.GetTilesetFromGid(Gid);
        }
    }
}
