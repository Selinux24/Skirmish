using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Common
{
    using Engine.Animation;

    /// <summary>
    /// The instance use skinning data for render
    /// </summary>
    public interface UseSkinningData
    {
        /// <summary>
        /// Gets the skinning list used by the current drawing data
        /// </summary>
        SkinningData[] SkinningData { get; }
    }
}
