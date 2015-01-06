using System.Collections.Generic;
using SharpDX.Direct3D11;

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
        /// Layout dictionary
        /// </summary>
        private Dictionary<string, InputLayout> layouts = new Dictionary<string, InputLayout>();

        /// <summary>
        /// Graphics device
        /// </summary>
        protected Device Device = null;
        /// <summary>
        /// Effect
        /// </summary>
        protected Effect Effect = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Graphics device</param>
        /// <param name="effect">Effect</param>
        public Drawer(Device device, byte[] effect)
        {
            this.Device = device;
            this.Effect = device.LoadEffect(effect);
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
        /// Finds technique and input layout for vertex type
        /// </summary>
        /// <param name="vertexType">Vertex type</param>
        /// <returns>Returns technique name for specified vertex type</returns>
        public abstract string AddVertexType(VertexTypes vertexType);
        /// <summary>
        /// Gest technique by name
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <returns>Returns technique description</returns>
        public EffectTechnique GetTechnique(string technique)
        {
            return this.Effect.GetTechniqueByName(technique);
        }
        /// <summary>
        /// Gets input layout by technique name
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <returns>Returns input layout for technique</returns>
        public InputLayout GetInputLayout(string technique)
        {
            return this.layouts[technique];
        }
        /// <summary>
        /// Add input layout to dictionary
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <param name="layout">Input layout</param>
        public void AddInputLayout(string technique, InputLayout layout)
        {
            if (!this.layouts.ContainsKey(technique))
            {
                this.layouts.Add(technique, layout);
            }
            else
            {
                this.layouts[technique] = layout;
            }
        }
    }
}
