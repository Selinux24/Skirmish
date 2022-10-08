
namespace Engine.Common
{
    using DXComparison = SharpDX.Direct3D11.Comparison;
    using DXDepthStencilOperationDescription = SharpDX.Direct3D11.DepthStencilOperationDescription;
    using DXDepthStencilStateDescription = SharpDX.Direct3D11.DepthStencilStateDescription;
    using DXDepthWriteMask = SharpDX.Direct3D11.DepthWriteMask;
    using DXStencilOperation = SharpDX.Direct3D11.StencilOperation;

    /// <summary>
    /// Describes depth-stencil state.
    /// </summary>
    public struct EngineDepthStencilStateDescription
    {
        public static explicit operator DXDepthStencilStateDescription(EngineDepthStencilStateDescription obj)
        {
            return new DXDepthStencilStateDescription
            {
                IsDepthEnabled = obj.IsDepthEnabled,
                DepthWriteMask = (DXDepthWriteMask)obj.DepthWriteMask,
                DepthComparison = (DXComparison)obj.DepthComparison,
                IsStencilEnabled = obj.IsStencilEnabled,
                StencilReadMask = obj.StencilReadMask,
                StencilWriteMask = obj.StencilWriteMask,
                FrontFace = (DXDepthStencilOperationDescription)obj.FrontFace,
                BackFace = (DXDepthStencilOperationDescription)obj.BackFace,
            };
        }
        public static explicit operator EngineDepthStencilStateDescription(DXDepthStencilStateDescription obj)
        {
            return new EngineDepthStencilStateDescription
            {
                IsDepthEnabled = obj.IsDepthEnabled,
                DepthWriteMask = (DepthWriteMask)obj.DepthWriteMask,
                DepthComparison = (Comparison)obj.DepthComparison,
                IsStencilEnabled = obj.IsStencilEnabled,
                StencilReadMask = obj.StencilReadMask,
                StencilWriteMask = obj.StencilWriteMask,
                FrontFace = (DepthStencilOperationDescription)obj.FrontFace,
                BackFace = (DepthStencilOperationDescription)obj.BackFace,
            };
        }

        /// <summary>
        /// Returns default values for DepthStencilStateDescription.
        /// </summary>
        public static EngineDepthStencilStateDescription Default()
        {
            return new EngineDepthStencilStateDescription();
        }

        /// <summary>
        /// Enable depth testing.
        /// </summary>
        public bool IsDepthEnabled { get; set; }
        /// <summary>
        /// Identify a portion of the depth-stencil buffer that can be modified by depth data (see DepthWriteMask).
        /// </summary>
        public DepthWriteMask DepthWriteMask { get; set; }
        /// <summary>
        /// A function that compares depth data against existing depth data. The function options are listed in Comparison.
        /// </summary>
        public Comparison DepthComparison { get; set; }
        /// <summary>
        /// Enable stencil testing.
        /// </summary>
        public bool IsStencilEnabled { get; set; }
        /// <summary>
        /// Identify a portion of the depth-stencil buffer for reading stencil data.
        /// </summary>
        public byte StencilReadMask { get; set; }
        /// <summary>
        /// Identify a portion of the depth-stencil buffer for writing stencil data.
        /// </summary>
        public byte StencilWriteMask { get; set; }
        /// <summary>
        /// Identify how to use the results of the depth test and the stencil test for pixels whose surface normal is facing towards the camera (see DepthStencilOperationDescription).
        /// </summary>
        public DepthStencilOperationDescription FrontFace { get; set; }
        /// <summary>
        /// Identify how to use the results of the depth test and the stencil test for pixels whose surface normal is facing away from the camera (see DepthStencilOperationDescription).
        /// </summary>
        public DepthStencilOperationDescription BackFace { get; set; }
    }

    /// <summary>
    /// Identify the portion of a depth-stencil buffer for writing depth data.
    /// </summary>
    public enum DepthWriteMask
    {
        /// <summary>
        /// Turn off writes to the depth-stencil buffer.
        /// </summary>
        Zero = DXDepthWriteMask.Zero,
        /// <summary>
        /// Turn on writes to the depth-stencil buffer.
        /// </summary>
        All = DXDepthWriteMask.All
    }

    /// <summary>
    /// Stencil operations that can be performed based on the results of stencil test.
    /// </summary>
    public struct DepthStencilOperationDescription
    {
        public static explicit operator DXDepthStencilOperationDescription(DepthStencilOperationDescription obj)
        {
            return new DXDepthStencilOperationDescription
            {
                FailOperation = (DXStencilOperation)obj.FailOperation,
                DepthFailOperation = (DXStencilOperation)obj.FailOperation,
                PassOperation = (DXStencilOperation)obj.PassOperation,
                Comparison = (DXComparison)obj.Comparison,
            };
        }
        public static explicit operator DepthStencilOperationDescription(DXDepthStencilOperationDescription obj)
        {
            return new DepthStencilOperationDescription
            {
                FailOperation = (StencilOperation)obj.FailOperation,
                DepthFailOperation = (StencilOperation)obj.FailOperation,
                PassOperation = (StencilOperation)obj.PassOperation,
                Comparison = (Comparison)obj.Comparison,
            };
        }

        /// <summary>
        /// The stencil operation to perform when stencil testing fails.
        /// </summary>
        public StencilOperation FailOperation { get; set; }
        /// <summary>
        /// The stencil operation to perform when stencil testing passes and depth testing fails.
        /// </summary>
        public StencilOperation DepthFailOperation { get; set; }
        /// <summary>
        /// The stencil operation to perform when stencil testing and depth testing both pass.
        /// </summary>
        public StencilOperation PassOperation { get; set; }
        /// <summary>
        /// A function that compares stencil data against existing stencil data. 
        /// The function options are listed in Comparison.
        /// </summary>
        public Comparison Comparison { get; set; }
    }

    /// <summary>
    /// The stencil operations that can be performed during depth-stencil testing.
    /// </summary>
    public enum StencilOperation
    {
        /// <summary>
        /// Keep the existing stencil data.
        /// </summary>
        Keep = DXStencilOperation.Keep,
        /// <summary>
        /// Set the stencil data to 0.
        /// </summary>
        Zero = DXStencilOperation.Zero,
        /// <summary>
        /// Set the stencil data to the reference value set by calling ID3D11DeviceContext::OMSetDepthStencilState.
        /// </summary>
        Replace = DXStencilOperation.Replace,
        /// <summary>
        /// Increment the stencil value by 1, and clamp the result.
        /// </summary>
        IncrementAndClamp = DXStencilOperation.IncrementAndClamp,
        /// <summary>
        /// Decrement the stencil value by 1, and clamp the result.
        /// </summary>
        DecrementAndClamp = DXStencilOperation.DecrementAndClamp,
        /// <summary>
        /// Invert the stencil data.
        /// </summary>
        Invert = DXStencilOperation.Invert,
        /// <summary>
        /// Increment the stencil value by 1, and wrap the result if necessary.
        /// </summary>
        Increment = DXStencilOperation.Increment,
        /// <summary>
        /// Decrement the stencil value by 1, and wrap the result if necessary.
        /// </summary>
        Decrement = DXStencilOperation.Decrement
    }
}
