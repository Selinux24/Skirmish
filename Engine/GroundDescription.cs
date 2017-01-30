using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.PathFinding;
    using Engine.Common;

    /// <summary>
    /// Ground description
    /// </summary>
    public class GroundDescription : DrawableDescription
    {
        /// <summary>
        /// Vegetation
        /// </summary>
        public class VegetationDescription
        {
            /// <summary>
            /// Vegetation channel
            /// </summary>
            public class Channel
            {
                /// <summary>
                /// Texture names array for vegetation
                /// </summary>
                public string[] VegetarionTextures = null;
                /// <summary>
                /// Vegetation sprite minimum size
                /// </summary>
                public Vector2 MinSize = Vector2.One;
                /// <summary>
                /// Vegetation sprite maximum size
                /// </summary>
                public Vector2 MaxSize = Vector2.One * 2f;
                /// <summary>
                /// Drawing radius for vegetation
                /// </summary>
                public float StartRadius = 0f;
                /// <summary>
                /// Drawing radius for vegetation
                /// </summary>
                public float EndRadius = 0f;
                /// <summary>
                /// Seed for random position generation
                /// </summary>
                public int Seed = 0;
                /// <summary>
                /// Vegetation saturation per triangle
                /// </summary>
                public float Saturation = 0.1f;
                /// <summary>
                /// Toggles UV in shader by instance
                /// </summary>
                public bool ToggleUV = true;
                /// <summary>
                /// Wind effect
                /// </summary>
                public float WindEffect = 1f;
            }

            /// <summary>
            /// Content path
            /// </summary>
            public string ContentPath = "Resources";
            /// <summary>
            /// Casts shadow
            /// </summary>
            public bool CastShadow = true;
            /// <summary>
            /// Can be renderer by the deferred renderer
            /// </summary>
            public bool DeferredEnabled = true;

            /// <summary>
            /// Vegetation map
            /// </summary>
            public string VegetationMap = null;
            /// <summary>
            /// Red vegetation channel from map
            /// </summary>
            public Channel ChannelRed = new Channel();
            /// <summary>
            /// Green vegetation channel from map
            /// </summary>
            public Channel ChannelGreen = new Channel();
            /// <summary>
            /// Blue vegetation channel from map
            /// </summary>
            public Channel ChannelBlue = new Channel();
            /// <summary>
            /// Gets the active channel list
            /// </summary>
            public Channel[] Channels
            {
                get
                {
                    List<Channel> channels = new List<Channel>();

                    channels.Add(this.ChannelRed);
                    channels.Add(this.ChannelGreen);
                    channels.Add(this.ChannelBlue);

                    return channels.ToArray();
                }
            }
        }
        /// <summary>
        /// Quadtree description
        /// </summary>
        public class QuadtreeDescription
        {
            /// <summary>
            /// Maximum depth
            /// </summary>
            public int MaximumDepth { get; set; }

            /// <summary>
            /// Constructor
            /// </summary>
            public QuadtreeDescription()
            {
                this.MaximumDepth = 3;
            }
        }
        /// <summary>
        /// Path finder grid description
        /// </summary>
        public class PathFinderDescription
        {
            /// <summary>
            /// Graph type
            /// </summary>
            public PathFinderSettings Settings = null;
        }

        /// <summary>
        /// Vegetation collection
        /// </summary>
        public VegetationDescription Vegetation = null;
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree = new QuadtreeDescription();
        /// <summary>
        /// Path finder
        /// </summary>
        public PathFinderDescription PathFinder = null;

        /// <summary>
        /// Delay internal generation
        /// </summary>
        public bool DelayGeneration = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundDescription()
            : base()
        {
            this.Static = true;
            this.AlwaysVisible = false;
            this.CastShadow = true;
            this.DeferredEnabled = true;
            this.EnableDepthStencil = true;
            this.EnableAlphaBlending = false;
        }
    }
}
