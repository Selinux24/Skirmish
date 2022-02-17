using SharpDX;

namespace Tanks
{
    /// <summary>
    /// Player status class
    /// </summary>
    public class PlayerStatus
    {
        /// <summary>
        /// Player name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Player points
        /// </summary>
        public int Points { get; set; }
        /// <summary>
        /// Maximum life
        /// </summary>
        public int MaxLife { get; set; }
        /// <summary>
        /// Current life
        /// </summary>
        public int CurrentLife { get; set; }
        /// <summary>
        /// Max movement
        /// </summary>
        public int MaxMove { get; set; }
        /// <summary>
        /// Current movement
        /// </summary>
        public float CurrentMove { get; set; }
        /// <summary>
        /// Player color
        /// </summary>
        public Color Color { get; set; }
        /// <summary>
        /// Tint color
        /// </summary>
        public Color TintColor { get; set; }
        /// <summary>
        /// Health
        /// </summary>
        public float Health
        {
            get
            {
                return (float)CurrentLife / MaxLife;
            }
        }
        /// <summary>
        /// Texture index
        /// </summary>
        /// <remarks>Based on health</remarks>
        public uint TextureIndex
        {
            get
            {
                if (Health > 0.6666f)
                {
                    return 0;
                }
                else if (Health > 0)
                {
                    return 1;
                }

                return 2;
            }
        }

        /// <summary>
        /// Updates internal state when a new turn occured
        /// </summary>
        public void NewTurn()
        {
            CurrentMove = MaxMove;
        }
    }
}
