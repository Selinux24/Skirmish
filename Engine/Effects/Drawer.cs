
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
        public Drawer(Graphics graphics, byte[] effect, bool compile)
        {
            this.Graphics = graphics;

            if (compile)
            {
                this.Effect = graphics.CompileEffect(effect, HelperShaders.FXProfile);
            }
            else
            {
                this.Effect = graphics.LoadEffect(effect);
            }
        }
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (this.Effect != null)
            {
                this.Effect.Dispose();
                this.Effect = null;
            }
        }

        /// <summary>
        /// Optimize effect
        /// </summary>
        public void Optimize()
        {
            this.Effect.Optimize();
        }
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public abstract EngineEffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode);
    }
}
