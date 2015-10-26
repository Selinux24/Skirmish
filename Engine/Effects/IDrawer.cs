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
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        EffectTechnique GetTechnique(VertexTypes vertexType, DrawingStages stage, DrawerModesEnum mode);
        /// <summary>
        /// Gets input layout by technique name
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <returns>Returns input layout for technique</returns>
        InputLayout GetInputLayout(EffectTechnique technique);
        /// <summary>
        /// Add input layout to dictionary
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <param name="input">Input elements</param>
        void AddInputLayout(EffectTechnique technique, InputElement[] input);
    }
}
