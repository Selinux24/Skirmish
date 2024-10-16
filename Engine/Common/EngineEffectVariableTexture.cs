﻿
namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Effect texture variable
    /// </summary>
    public class EngineEffectVariableTexture
    {
        /// <summary>
        /// Effect variable
        /// </summary>
        private readonly EffectShaderResourceVariable variable = null;
        /// <summary>
        /// Shader resource
        /// </summary>
        private EngineShaderResourceView resource = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="variable">Variable</param>
        public EngineEffectVariableTexture(EffectShaderResourceVariable variable)
        {
            this.variable = variable;
        }

        /// <summary>
        /// Sets the resource to the shader
        /// </summary>
        /// <param name="resource">Resource</param>
        public void SetResource(EngineShaderResourceView resource)
        {
            variable.SetResource(resource?.GetResource());
        }

        /// <summary>
        /// Gets the resource from the shader
        /// </summary>
        /// <returns>Returns the resource from the shader</returns>
        public EngineShaderResourceView GetResource()
        {
            var srv = variable.GetResource()?.QueryInterface<ShaderResourceView1>();
            if (srv == null)
            {
                return null;
            }

            if (resource == null)
            {
                resource = new EngineShaderResourceView(variable.Description.Name ?? nameof(EngineEffectVariableTexture), srv);
            }
            else
            {
                resource.SetResource(srv);
            }

            return resource;
        }
    }
}
