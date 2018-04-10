using System;

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
        public AgentType()
        {
            Name = "Player";
            Height = 2.0f;
        }

        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode();
        }
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
    }
}
