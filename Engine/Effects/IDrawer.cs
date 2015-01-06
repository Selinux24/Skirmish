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
        /// Finds technique and input layout for vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns technique name for specified vertex type</returns>
        string AddVertexType(VertexTypes vertexType);
        /// <summary>
        /// Gest technique by name
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <returns>Returns technique description</returns>
        EffectTechnique GetTechnique(string technique);
        /// <summary>
        /// Gets input layout by technique name
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <returns>Returns input layout for technique</returns>
        InputLayout GetInputLayout(string technique);
    }
}
