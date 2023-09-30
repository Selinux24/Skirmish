﻿using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Agent type
    /// </summary>
    [Serializable]
    public abstract class AgentType
    {
        /// <summary>
        /// Agent name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the height of the agent
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected AgentType()
        {
            Name = "Player";
            Height = 2.0f;
        }

        /// <summary>
        /// Compares another object with this instance for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating whether the object is equal to this instance.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            if (obj is AgentType other)
            {
                return
                    other.Name == this.Name &&
                    other.Height == this.Height;
            }

            return false;
        }
        /// <summary>
        /// Calculates a hash code unique to the contents of this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode();
        }
    }
}
