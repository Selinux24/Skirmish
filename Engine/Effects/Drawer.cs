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
        private Dictionary<EffectTechnique, InputLayout> layouts = new Dictionary<EffectTechnique, InputLayout>();

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
        /// <param name="effect">Effect file</param>
        /// <param name="compile">Compile effect</param>
        public Drawer(Device device, byte[] effect, bool compile)
        {
            this.Device = device;
            if (compile)
            {
                this.Effect = device.CompileEffect(effect);
            }
            else
            {
                this.Effect = device.LoadEffect(effect);
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
        /// Get technique by vertex type
        /// </summary>
        /// <param name="vertexType">VertexType</param>
        /// <param name="instanced">Use instancing data</param>
        /// <param name="stage">Stage</param>
        /// <param name="mode">Mode</param>
        /// <returns>Returns the technique to process the specified vertex type in the specified pipeline stage</returns>
        public abstract EffectTechnique GetTechnique(VertexTypes vertexType, bool instanced, DrawingStages stage, DrawerModesEnum mode);
        /// <summary>
        /// Gets input layout by technique name
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <returns>Returns input layout for technique</returns>
        public InputLayout GetInputLayout(EffectTechnique technique)
        {
            return this.layouts[technique];
        }
        /// <summary>
        /// Add input layout to dictionary
        /// </summary>
        /// <param name="technique">Technique name</param>
        /// <param name="input">Input elements</param>
        public void AddInputLayout(EffectTechnique technique, InputElement[] input)
        {
            EffectPassDescription desc = technique.GetPassByIndex(0).Description;

            InputLayout layout = new InputLayout(
                this.Device,
                desc.Signature,
                input);

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
