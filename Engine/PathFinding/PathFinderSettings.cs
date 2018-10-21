using SharpDX;
using System;
using System.Xml.Serialization;

namespace Engine.PathFinding
{
    [Serializable]
    public abstract class PathFinderSettings
    {
        /// <summary>
        /// Path Finder bounds
        /// </summary>
        [XmlIgnore]
        public BoundingBox? Bounds { get; set; } = null;
        /// <summary>
        /// Serialization property
        /// </summary>
        internal float[] InternalBounds
        {
            get
            {
                if (Bounds.HasValue)
                {
                    return new float[]
                    {
                        Bounds.Value.Minimum.X,
                        Bounds.Value.Minimum.Y,
                        Bounds.Value.Minimum.Z,

                        Bounds.Value.Maximum.X,
                        Bounds.Value.Maximum.Y,
                        Bounds.Value.Maximum.Z,
                    };
                }
                else
                {
                    return new float[] { };
                }
            }
            set
            {
                if (value != null && value.Length == 6)
                {
                    Bounds = new BoundingBox(
                        new Vector3(value[0], value[1], value[2]),
                        new Vector3(value[3], value[4], value[5]));
                }
                else
                {
                    Bounds = null;
                }
            }
        }
    }
}
