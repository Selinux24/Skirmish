using AISamples.Common;
using AISamples.Common.Primitives;
using Engine;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AISamples.SceneCWRVirtualWorld
{
    /// <summary>
    /// Open street map data struct
    /// </summary>
    struct OsmData
    {
        /// <summary>
        /// Element list
        /// </summary>
        public OsmNode[] Elements { get; set; }
    }

    /// <summary>
    /// Open street map node struct
    /// </summary>
    struct OsmNode
    {
        /// <summary>
        /// Node type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Node id
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Latitude value
        /// </summary>
        public float Lat { get; set; }
        /// <summary>
        /// Longitud value
        /// </summary>
        public float Lon { get; set; }
        /// <summary>
        /// Node id list
        /// </summary>
        public long[] Nodes { get; set; }
        /// <summary>
        /// Tags
        /// </summary>
        public OsmTags Tags { get; set; }
    }

    /// <summary>
    /// Open Street Map tags struct
    /// </summary>
    struct OsmTags
    {
        /// <summary>
        /// Number of lanes
        /// </summary>
        public string Lanes { get; set; }
        /// <summary>
        /// One way street
        /// </summary>
        public string OneWay { get; set; }
    }

    /// <summary>
    /// Open street map parser
    /// </summary>
    static class Osm
    {
        public static Graph ParseRoads(string fileName, float scale = 1)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(File.ReadAllText(fileName));

            OsmData osm = SerializationHelper.DeserializeJson<OsmData>(byteArray);

            var elements = Array.FindAll(osm.Elements, e => e.Type == "node").ToArray();

            var maxLat = elements.Max(e => e.Lat);
            var minLat = elements.Min(e => e.Lat);
            var maxLon = elements.Max(e => e.Lon);
            var minLon = elements.Min(e => e.Lon);

            var deltaLon = maxLon - minLon;
            var deltaLat = maxLat - minLat;
            var ar = deltaLon / deltaLat * MathF.Cos(MathUtil.DegreesToRadians(maxLat));
            float height = deltaLat * 111000;
            float width = height * ar;
            height *= scale;
            width *= scale;

            Dictionary<long, Vector2> points = [];
            foreach (var node in elements)
            {
                float lat = Utils.InvLerp(minLat, maxLat, node.Lat) * height;
                float lon = Utils.InvLerp(minLon, maxLon, node.Lon) * width;

                lat -= height * 0.5f;
                lon -= width * 0.5f;

                points.Add(node.Id, new(lon, lat));
            }

            var ways = Array.FindAll(osm.Elements, e => e.Type == "way").ToArray();

            List<Segment2> segments = [];
            foreach (var way in ways)
            {
                for (int i = 1; i < way.Nodes.Length; i++)
                {
                    var i1 = way.Nodes[i - 1];
                    var i2 = way.Nodes[i];
                    bool oneWay = way.Tags.OneWay == "yes" || way.Tags.Lanes == "1";

                    segments.Add(new(points[i1], points[i2], oneWay));
                }
            }

            return new([.. points.Values], [.. segments]);
        }
    }
}
