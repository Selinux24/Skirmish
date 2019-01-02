using System.Collections.Generic;

namespace Engine.Content
{
    using Engine.Animation;

    /// <summary>
    /// Skinning content
    /// </summary>
    public class SkinningContent
    {
        /// <summary>
        /// Controller collection
        /// </summary>
        private readonly List<string> controllers = new List<string>();

        /// <summary>
        /// Controller names
        /// </summary>
        public string[] Controllers
        {
            get
            {
                return this.controllers.ToArray();
            }
            set
            {
                this.controllers.Clear();

                if (value != null && value.Length > 0)
                {
                    this.controllers.AddRange(value);
                }
            }
        }
        /// <summary>
        /// Skeleton information
        /// </summary>
        public Skeleton Skeleton { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SkinningContent()
        {

        }
        /// <summary>
        /// Adds a controller reference to skinning content
        /// </summary>
        /// <param name="controller">Controller name</param>
        public void Add(string controller)
        {
            if (!this.controllers.Contains(controller))
            {
                this.controllers.Add(controller);
            }
        }
        /// <summary>
        /// Gets text representation of instance
        /// </summary>
        /// <returns>Returns text representation of instance</returns>
        public override string ToString()
        {
            if (this.Controllers != null && this.Controllers.Length == 1)
            {
                return string.Format("{0}", this.Controllers[0]);
            }
            else if (this.Controllers != null && this.Controllers.Length > 1)
            {
                return string.Format("{0}", string.Join(", ", this.Controllers));
            }
            else
            {
                return "Empty Controller;";
            }
        }
    }
}
