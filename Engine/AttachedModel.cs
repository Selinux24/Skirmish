using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Gets the attached object pickable items
        /// </summary>
        /// <returns>Returns the attached object pickable items</returns>
        public IRayPickable<Triangle>[] GetObjects()
        {
            if (this.Model is Model)
            {
                return new IRayPickable<Triangle>[] { (Model)this.Model };
            }
            else if (this.Model is ModelInstanced)
            {
                List<IRayPickable<Triangle>> list = new List<IRayPickable<Triangle>>();

                foreach (var instance in ((ModelInstanced)this.Model).GetInstances())
                {
                    list.Add(instance);
                }

                return list.ToArray();
            }

            return null;
        }
    }
}
