using System;
using System.Xml.Serialization;

namespace Engine
{
    /// <summary>
    /// Object reference
    /// </summary>
    [Serializable]
    public class ModularSceneryObjectReference : ModularSceneryAssetReference
    {
        /// <summary>
        /// Particle
        /// </summary>
        [XmlElement("particleLight", Type = typeof(ParticleEmitterDescription))]
        public ParticleEmitterDescription ParticleLight { get; set; }
    }
}
