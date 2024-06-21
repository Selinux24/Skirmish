using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Content;

    /// <summary>
    /// Ground description
    /// </summary>
    public abstract class GroundDescription : SceneObjectDescription
    {
        /// <summary>
        /// Content
        /// </summary>
        public ContentDescription Content { get; set; }
        /// <summary>
        /// Content list
        /// </summary>
        public IEnumerable<ContentDescription> ContentList { get; set; }
        /// <summary>
        /// Quadtree
        /// </summary>
        public QuadtreeDescription Quadtree { get; set; } = QuadtreeDescription.Default(4);
        /// <summary>
        /// Use anisotropic filtering
        /// </summary>
        public bool UseAnisotropic { get; set; } = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public GroundDescription()
            : base()
        {
            BlendMode = BlendModes.Opaque;
            CastShadow = ShadowCastingAlgorihtms.All;
        }

        /// <summary>
        /// Reads the content data from description
        /// </summary>
        public virtual async Task<IEnumerable<ContentData>> ReadContentData()
        {
            // Read model content
            if (Content != null)
            {
                return await Content.ReadContentData();
            }
            else if (ContentList?.Any() == true)
            {
                var tasks = ContentList.Select(c => c.ReadContentData());

                var res = await Task.WhenAll(tasks);

                return res.SelectMany(r => r);
            }
            else
            {
                throw new EngineException("No geometry found in description.");
            }
        }
        /// <summary>
        /// Reads the content library from description
        /// </summary>
        public virtual async Task<ContentLibrary> ReadContentLibrary()
        {
            return new ContentLibrary(await ReadContentData());
        }
        /// <summary>
        /// Reads a quadtree from description
        /// </summary>
        /// <typeparam name="T">Quadtree item type</typeparam>
        /// <param name="items">Quadtree items</param>
        public virtual PickingQuadTree<T> ReadQuadTree<T>(IEnumerable<T> items) where T : IVertexList, IRayIntersectable
        {
            if (Quadtree != null)
            {
                return new PickingQuadTree<T>(items, Quadtree);
            }

            return null;
        }
    }
}
