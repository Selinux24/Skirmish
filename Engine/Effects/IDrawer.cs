using System;

namespace Engine.Effects
{
    /// <summary>
    /// Drawer class
    /// </summary>
    public interface IDrawer : IDisposable
    {
        /// <summary>
        /// Optimize effect
        /// </summary>
        void Optimize();
    }
}
