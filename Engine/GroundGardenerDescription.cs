using SharpDX;
using System;
using System.Collections.Generic;

namespace Engine
{
    using Engine.BuiltIn.Foliage;

    /// <summary>
    /// Ground gardener description
    /// </summary>
    public class GroundGardenerDescription : SceneObjectDescription
    {
        /// <summary>
        /// Vegetation channel
        /// </summary>
        public class Channel
        {
            /// <summary>
            /// Texture names array for vegetation
            /// </summary>
            public string[] VegetationTextures { get; set; } = null;
            /// <summary>
            /// Normal maps names array for vegetation
            /// </summary>
            public string[] VegetationNormalMaps { get; set; } = null;
            /// <summary>
            /// Vegetation sprite minimum size
            /// </summary>
            public Vector2 MinSize { get; set; } = Vector2.One;
            /// <summary>
            /// Vegetation sprite maximum size
            /// </summary>
            public Vector2 MaxSize { get; set; } = Vector2.One * 2f;
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float StartRadius { get; set; } = 0f;
            /// <summary>
            /// Drawing radius for vegetation
            /// </summary>
            public float EndRadius { get; set; } = 0f;
            /// <summary>
            /// Seed for random position generation
            /// </summary>
            public int Seed { get; set; } = 0;
            /// <summary>
            /// Vegetation saturation per triangle
            /// </summary>
            public float Saturation { get; set; } = 0.1f;
            /// <summary>
            /// Wind effect
            /// </summary>
            public float WindEffect { get; set; } = 1f;
            /// <summary>
            /// Channel enabled
            /// </summary>
            public bool Enabled { get; set; } = true;
            /// <summary>
            /// Geometry output instances
            /// </summary>
            public GroundGardenerPatchInstances Instances { get; set; } = GroundGardenerPatchInstances.Default;
            /// <summary>
            /// Delta added to instance position
            /// </summary>
            /// <remarks>
            /// Y value applies to all of the instances
            /// X and Z values only applies to additional instances
            /// </remarks>
            public Vector3 Delta { get; set; } = new Vector3(0.5f, 0.0f, 0.5f);
        }

        /// <summary>
        /// Content path
        /// </summary>
        public string ContentPath { get; set; } = "Resources";

        /// <summary>
        /// Vegetation map
        /// </summary>
        public string VegetationMap { get; set; } = null;
        /// <summary>
        /// Visible radius
        /// </summary>
        public float VisibleRadius
        {
            get
            {
                float vRadius = 0;

                for (int i = 0; i < Channels.Length; i++)
                {
                    vRadius = Math.Max(vRadius, Channels[i].EndRadius);
                }

                return vRadius;
            }
        }
        /// <summary>
        /// Quadtree maximum node size
        /// </summary>
        public float NodeSize { get; set; } = 128f;
        /// <summary>
        /// Red vegetation channel from map
        /// </summary>
        public Channel ChannelRed { get; set; } = new Channel() { Enabled = false };
        /// <summary>
        /// Green vegetation channel from map
        /// </summary>
        public Channel ChannelGreen { get; set; } = new Channel() { Enabled = false };
        /// <summary>
        /// Blue vegetation channel from map
        /// </summary>
        public Channel ChannelBlue { get; set; } = new Channel() { Enabled = false };
        /// <summary>
        /// Gets the active channel list
        /// </summary>
        public Channel[] Channels
        {
            get
            {
                List<Channel> channels = new List<Channel>
                {
                    ChannelRed,
                    ChannelGreen,
                    ChannelBlue
                };

                return channels.ToArray();
            }
        }
        /// <summary>
        /// Planting area
        /// </summary>
        public BoundingBox? PlantingArea { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundGardenerDescription()
            : base()
        {
            CastShadow = ShadowCastingAlgorihtms.Directional;
            DeferredEnabled = false;
            BlendMode = BlendModes.Transparent;
        }
    }

    /// <summary>
    /// Additional instances enumeration
    /// </summary>
    public enum GroundGardenerPatchInstances : uint
    {
        /// <summary>
        /// One instance
        /// </summary>
        Default = BuiltInFoliageInstances.Default,
        /// <summary>
        /// Two instances
        /// </summary>
        Two = BuiltInFoliageInstances.Two,
        /// <summary>
        /// Four instances
        /// </summary>
        Four = BuiltInFoliageInstances.Four,
    }
}
