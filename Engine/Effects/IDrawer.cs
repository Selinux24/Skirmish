using System;
using SharpDX.Direct3D11;

namespace Engine.Effects
{
    using Engine.Common;

    /// <summary>
    /// Drawer class
    /// </summary>
    public interface IDrawer : IDisposable
    {
        /// <summary>
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode);
    }
}
