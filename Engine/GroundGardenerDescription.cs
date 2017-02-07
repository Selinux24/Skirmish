﻿using SharpDX;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Ground gardener description
    /// </summary>
    public class GroundGardenerDescription : DrawableDescription
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

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundGardenerDescription()
            : base()
        {
            this.Static = false;
            this.CastShadow = true;
            this.DeferredEnabled = false;
            this.DepthEnabled = true;
            this.AlphaEnabled = true;
        }
    }
}