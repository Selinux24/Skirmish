
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
            if (resource != null)
            {
                this.variable.SetResource(resource.GetResource());
            }
            else
            {
                this.variable.SetResource(null);
            }
        }

        /// <summary>
        /// Gets the resource from the shader
        /// </summary>
        /// <returns>Returns the resource from the shader</returns>
        public EngineShaderResourceView GetResource()
        {
            var srv = this.variable.GetResource().QueryInterface<ShaderResourceView1>();

            return new EngineShaderResourceView(srv);
        }
    }
}
