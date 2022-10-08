using System;

namespace Engine
{
    /// <summary>
    /// Game load resources event arguments
    /// </summary>
    public class GameLoadResourcesEventArgs : EventArgs
    {
        /// <summary>
        /// Load identifier
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Scene
        /// </summary>
        public Scene Scene { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameLoadResourcesEventArgs() : base()
        {

        }
    }
}
