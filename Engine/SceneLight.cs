﻿using SharpDX;

namespace Engine
{
    /// <summary>
    /// Scene ligth
    /// </summary>
    public abstract class SceneLight
    {
        /// <summary>
        /// Light name
        /// </summary>
        public string Name = null;
        /// <summary>
        /// Enables or disables the light
        /// </summary>
        public bool Enabled = false;
        /// <summary>
        /// Gets or stes wheter the light casts shadow
        /// </summary>
        public bool CastShadow = false;
        /// <summary>
        /// Diffuse color
        /// </summary>
        public Color4 DiffuseColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Specular color
        /// </summary>
        public Color4 SpecularColor = new Color4(0.0f, 0.0f, 0.0f, 0.0f);
        /// <summary>
        /// Free use variable
        /// </summary>
        public object State = null;

        /// <summary>
        /// Gets the text representation of the light
        /// </summary>
        /// <returns>Returns the text representation of the light</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                return string.Format("{0}; Enabled: {1}", this.Name, this.Enabled);
            }
            else
            {
                return string.Format("{0}; Enabled {1}", this.GetType(), this.Enabled);
            }
        }
    }
}
