using System.Collections.Generic;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Common;

    /// <summary>
    /// Map grid node
    /// </summary>
    class TerrainGridNode
    {
        /// <summary>
        /// Level of detail
        /// </summary>
        public LevelOfDetail LevelOfDetail;
        /// <summary>
        /// Shape
        /// </summary>
        public IndexBufferShapes Shape;
        /// <summary>
        /// Node
        /// </summary>
        public QuadTreeNode<VertexData> Node;
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        public BufferDescriptor VBDesc;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        public BufferDescriptor IBDesc;

        /// <summary>
        /// Constructor
        /// </summary>
        public TerrainGridNode()
        {

        }

        /// <summary>
        /// Sets the node information
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <param name="shape">Index buffer shape</param>
        /// <param name="direction">Direction to move</param>
        /// <param name="node">Node</param>
        /// <param name="dictVB">Vertex buffer description dictionary</param>
        /// <param name="dictIB">Index buffer description dictionary</param>
        public void Set(
            LevelOfDetail lod,
            IndexBufferShapes shape,
            IndexBufferShapes direction,
            QuadTreeNode<VertexData> node,
            Dictionary<int, BufferDescriptor> dictVB,
            Dictionary<TerrainGridShapeId, BufferDescriptor> dictIB)
        {
            QuadTreeNode<VertexData> nNode = null;

            if (node != null)
            {
                var dir = GetNodeDirection(direction, node);
                if (dir != null)
                {
                    nNode = dir;
                }
            }

            if (Node != nNode)
            {
                //Set buffer (VX)
                if (nNode != null)
                {
                    VBDesc = dictVB[nNode.Id];
                }
                Node = nNode;
            }

            bool assignIB = false;

            if (LevelOfDetail != lod)
            {
                //Set buffer (IX)
                LevelOfDetail = lod;

                assignIB = true;
            }

            if (Shape != shape)
            {
                //Set buffer (IX)
                Shape = shape;

                assignIB = true;
            }

            if (assignIB)
            {
                IBDesc = dictIB[new TerrainGridShapeId() { LevelOfDetail = lod, Shape = shape }];
            }
        }
        /// <summary>
        /// Gets the node direction
        /// </summary>
        /// <param name="direction">Shape direction</param>
        /// <param name="node">Current node</param>
        /// <returns>Returns the node in the direction</returns>
        private static QuadTreeNode<VertexData> GetNodeDirection(IndexBufferShapes direction, QuadTreeNode<VertexData> node)
        {
            if (direction == IndexBufferShapes.Full) return node;

            else if (direction == IndexBufferShapes.CornerTopLeft) return node.TopLeftNeighbor;
            else if (direction == IndexBufferShapes.CornerTopRight) return node.TopRightNeighbor;
            else if (direction == IndexBufferShapes.CornerBottomLeft) return node.BottomLeftNeighbor;
            else if (direction == IndexBufferShapes.CornerBottomRight) return node.BottomRightNeighbor;

            else if (direction == IndexBufferShapes.SideTop) return node.TopNeighbor;
            else if (direction == IndexBufferShapes.SideBottom) return node.BottomNeighbor;
            else if (direction == IndexBufferShapes.SideLeft) return node.LeftNeighbor;
            else if (direction == IndexBufferShapes.SideRight) return node.RightNeighbor;

            return null;
        }
    }
}
