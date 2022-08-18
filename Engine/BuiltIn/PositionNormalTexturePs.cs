using System;
using System.Runtime.InteropServices;

namespace Engine.BuiltIn
{
    using Engine.Common;
    using Engine.Helpers;
    using Engine.Properties;

    /// <summary>
    /// Position normal texture pixel shader
    /// </summary>
    public class PositionNormalTexturePs : IDisposable
    {
        /// <summary>
        /// Per object data structure
        /// </summary>
        [StructLayout(LayoutKind.Explicit, Size = 16)]
        struct PerObject : IBufferData
        {
            public static PerObject Build(bool useColorDiffuse)
            {
                return new PerObject
                {
                    UseColorDiffuse = useColorDiffuse,
                };
            }

            /// <summary>
            /// Use color diffuse value
            /// </summary>
            [FieldOffset(0)]
            public bool UseColorDiffuse;

            /// <inheritdoc/>
            public int GetStride()
            {
                return Marshal.SizeOf(typeof(PerObject));
            }
        }

        /// <summary>
        /// Shader
        /// </summary>
        public readonly EnginePixelShader Shader;

        /// <summary>
        /// Per object constant buffer
        /// </summary>
        private readonly EngineConstantBuffer<PerObject> cbPerObject;
        /// <summary>
        /// Diffuse map resource view
        /// </summary>
        private EngineShaderResourceView diffuseMapArray;

        /// <summary>
        /// Graphics instance
        /// </summary>
        protected Graphics Graphics = null;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphics">Graphics device</param>
        public PositionNormalTexturePs(Graphics graphics)
        {
            Graphics = graphics;

            bool compile = Resources.Ps_PositionNormalTexture_Cso == null;
            var bytes = Resources.Ps_PositionNormalTexture_Cso ?? Resources.Ps_PositionNormalTexture;
            if (compile)
            {
                Shader = graphics.CompilePixelShader(nameof(PositionNormalTexturePs), "main", bytes, HelperShaders.PSProfile);
            }
            else
            {
                Shader = graphics.LoadPixelShader(nameof(PositionNormalTexturePs), bytes);
            }

            cbPerObject = new EngineConstantBuffer<PerObject>(graphics, nameof(PositionNormalTexturePs) + "." + nameof(PerObject));
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~PositionNormalTexturePs()
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
                Shader?.Dispose();

                cbPerObject?.Dispose();
            }
        }

        /// <summary>
        /// Writes per object data
        /// </summary>
        /// <param name="useColorDiffuse">Use color diffuse value</param>
        public void WriteCBPerObject(bool useColorDiffuse)
        {
            cbPerObject.WriteData(PerObject.Build(useColorDiffuse));
        }
        /// <summary>
        /// Sets the diffuse map array
        /// </summary>
        /// <param name="diffuseMapArray">Diffuse map array</param>
        public void SetDiffuseMap(EngineShaderResourceView diffuseMapArray)
        {
            this.diffuseMapArray = diffuseMapArray;
        }

        /// <summary>
        /// Sets the pixel shader constant buffers
        /// </summary>
        public void SetConstantBuffers()
        {
            var cb = new[]
            {
                 BuiltInShaders.GetPSPerFrameLit(),
                 cbPerObject,
            };

            Graphics.SetPixelShaderConstantBuffers(0, cb);

            var rv = new[]
            {
                BuiltInShaders.GetPSPerFrameLitShadowMapDir(),
                BuiltInShaders.GetPSPerFrameLitShadowMapSpot(),
                BuiltInShaders.GetPSPerFrameLitShadowMapPoint(),
                diffuseMapArray,
            };

            Graphics.SetPixelShaderResourceViews(0, rv);
        }
    }
}
