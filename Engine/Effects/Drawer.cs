using System;

namespace Engine.Effects
{
    using Engine.Common;
    using Engine.Helpers;

    /// <summary>
    /// Drawer
    /// </summary>
    public abstract class Drawer : IDrawer
    {
        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;
        /// <summary>
        /// Effect
        /// </summary>
        protected EngineEffect Effect = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        /// <param name="effect">Effect file</param>
        /// <param name="compile">Compile effect</param>
        protected Drawer(Graphics graphics, byte[] effect, bool compile)
        {
            Graphics = graphics;

            if (compile)
            {
                Effect = graphics.CompileEffect(effect, HelperShaders.FXProfile);
            }
            else
            {
                Effect = graphics.LoadEffect(effect);
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Drawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">Free managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Effect?.Dispose();
                Effect = null;
            }
        }

        /// <summary>
        /// Optimize effect
        /// </summary>
        public void Optimize()
        {
            Effect?.Optimize();
        }
    }
}
