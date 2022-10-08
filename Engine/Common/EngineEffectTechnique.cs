﻿using SharpDX.D3DCompiler;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Technique
    /// </summary>
    public class EngineEffectTechnique
    {
        /// <summary>
        /// Effect technique
        /// </summary>
        private readonly EffectTechnique techinque = null;

        /// <summary>
        /// Gets the effect pass count
        /// </summary>
        public int PassCount
        {
            get
            {
                return techinque.Description.PassCount;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="techinque">Internal technique</param>
        public EngineEffectTechnique(EffectTechnique techinque)
        {
            this.techinque = techinque;
        }

        /// <summary>
        /// Gets the pass in the specified index
        /// </summary>
        /// <param name="index">Pass index</param>
        /// <returns>Returns the effect pass</returns>
        internal EffectPass GetPass(int index)
        {
            return techinque.GetPassByIndex(index);
        }
        /// <summary>
        /// Gets the shader byte code
        /// </summary>
        /// <param name="pass">Pass index</param>
        /// <returns>Returns the shader byte code</returns>
        internal ShaderBytecode GetSignature(int pass = 0)
        {
            return techinque.GetPassByIndex(pass).Description.Signature;
        }
    }
}
