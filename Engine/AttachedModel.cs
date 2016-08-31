using System;

namespace Engine
{
    using Engine.Common;

    /// <summary>
    /// Attached object
    /// </summary>
    public class AttachedModel
    {
        /// <summary>
        /// Model
        /// </summary>
        public ModelBase Model = null;
        /// <summary>
        /// Model use
        /// </summary>
        public AttachedModelUsesEnum Use = AttachedModelUsesEnum.None;
    }
}
