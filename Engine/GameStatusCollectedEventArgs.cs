using System;
using System.Collections.Generic;

namespace Engine
{
    /// <summary>
    /// Game status collected event
    /// </summary>
    public class GameStatusCollectedEventArgs : EventArgs
    {
        /// <summary>
        /// Time trace dictionary
        /// </summary>
        public Dictionary<string, double> Trace { get; set; } = new Dictionary<string, double>();
    }
}
