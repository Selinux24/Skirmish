using System;

namespace Engine.Common
{
    using SharpDX.Direct3D11;

    /// <summary>
    /// Input layout
    /// </summary>
    public class EngineInputLayout : IDisposable
    {
        /// <summary>
        /// Internal input layout
        /// </summary>
        private readonly InputLayout inputLayout;

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        internal EngineInputLayout(string name, InputLayout inputLayout)
        {
            Name = name;
            this.inputLayout = inputLayout ?? throw new ArgumentNullException(nameof(inputLayout));

            this.inputLayout.DebugName = name;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~EngineInputLayout()
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
                inputLayout?.Dispose();
            }
        }

        /// <summary>
        /// Gets the internal input layout
        /// </summary>
        internal InputLayout GetInputLayout()
        {
            return inputLayout;
        }
    }
}
