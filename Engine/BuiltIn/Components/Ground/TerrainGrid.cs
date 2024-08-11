using Engine.BuiltIn.Primitives;
using Engine.Collections.Generic;
using Engine.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.BuiltIn.Components.Ground
{
    /// <summary>
    /// Map grid
    /// </summary>
    class TerrainGrid : IDisposable
    {
        /// <summary>
        /// Level of detail levels in map
        /// </summary>
        public const int LODLevels = 3;

        /// <summary>
        /// Creates a new descriptor based on triangles per node count and shape Id parameters
        /// </summary>
        /// <param name="shapeId">Shape Id</param>
        /// <param name="trianglesPerNode">Triangles per node</param>
        /// <param name="bufferManager">Buffer manager to allocate buffers</param>
        /// <returns>Returns the new buffer descriptor</returns>
        private static BufferDescriptor CreateDescriptor(TerrainGridShapeId shapeId, int trianglesPerNode, BufferManager bufferManager)
        {
            var indexList = GeometryUtil.GenerateIndices(shapeId.LevelOfDetail, shapeId.Shape, trianglesPerNode);

            return bufferManager.AddIndexData(string.Format("{0}.{1}", shapeId.LevelOfDetail, shapeId.Shape), false, indexList);
        }

        /// <summary>
        /// Updating nodes flag for asynchronous task
        /// </summary>
        private bool updatingNodes = false;

        /// <summary>
        /// Game
        /// </summary>
        public Game Game = null;
        /// <summary>
        /// High resolution nodes
        /// </summary>
        public TerrainGridNode[] NodesHigh = new TerrainGridNode[9];
        /// <summary>
        /// Medium resolution nodes
        /// </summary>
        public TerrainGridNode[] NodesMedium = new TerrainGridNode[16];
        /// <summary>
        /// Low resolution nodes
        /// </summary>
        public TerrainGridNode[] NodesLow = new TerrainGridNode[24];
        /// <summary>
        /// Minimum resolution nodes
        /// </summary>
        public TerrainGridNode[] NodesMinimum = new TerrainGridNode[32];
        /// <summary>
        /// Vertex buffer description dictionary
        /// </summary>
        private Dictionary<int, BufferDescriptor> dictVB = [];
        /// <summary>
        /// Index buffer description dictionary
        /// </summary>
        private Dictionary<TerrainGridShapeId, BufferDescriptor> dictIB = [];
        /// <summary>
        /// Tree
        /// </summary>
        private QuadTree<VertexData> drawingQuadTree = null;
        /// <summary>
        /// Last mapped node
        /// </summary>
        private QuadTreeNode<VertexData> lastNode = null;

        /// <summary>
        /// Creates a new grid
        /// </summary>
        /// <param name="game">Game instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="vertices">Vertices to map</param>
        /// <param name="trianglesPerNode">Triangles per terrain node</param>
        public static async Task<TerrainGrid> Create(Game game, string mapName, IEnumerable<VertexData> vertices, int trianglesPerNode)
        {
            var res = new TerrainGrid
            {
                Game = game,
            };

            //Populate collections
            for (int i = 0; i < res.NodesHigh.Length; i++)
            {
                res.NodesHigh[i] = new TerrainGridNode();
            }
            for (int i = 0; i < res.NodesMedium.Length; i++)
            {
                res.NodesMedium[i] = new TerrainGridNode();
            }
            for (int i = 0; i < res.NodesLow.Length; i++)
            {
                res.NodesLow[i] = new TerrainGridNode();
            }
            for (int i = 0; i < res.NodesMinimum.Length; i++)
            {
                res.NodesMinimum[i] = new TerrainGridNode();
            }

            var lodList = new[]
            {
                LevelOfDetail.High,
                LevelOfDetail.Medium,
                LevelOfDetail.Low,
                LevelOfDetail.Minimum,
            };
            var shapeList = new[]
            {
                IndexBufferShapes.Full,

                IndexBufferShapes.SideTop,
                IndexBufferShapes.SideBottom,
                IndexBufferShapes.SideLeft,
                IndexBufferShapes.SideRight,

                IndexBufferShapes.CornerTopLeft,
                IndexBufferShapes.CornerTopRight,
                IndexBufferShapes.CornerBottomLeft,
                IndexBufferShapes.CornerBottomRight,
            };

            //Populate shapes dictionary
            foreach (var lod in lodList)
            {
                foreach (var shape in shapeList)
                {
                    var sid = new TerrainGridShapeId() { LevelOfDetail = lod, Shape = shape };

                    res.dictIB.Add(sid, CreateDescriptor(sid, trianglesPerNode, game.BufferManager));
                }
            }

            var bbox = SharpDXExtensions.BoundingBoxFromPoints(vertices.SelectMany(v => v.GetVertices()).Distinct().ToArray());
            var items = vertices.Select(v => (SharpDXExtensions.BoundingBoxFromPoints(v.GetVertices().ToArray()), v));
            res.drawingQuadTree = new QuadTree<VertexData>(bbox, items, LODLevels);

            //Populate nodes dictionary
            var nodes = res.drawingQuadTree.GetLeafNodes();
            foreach (var node in nodes)
            {
                var data = await VertexTerrain.Convert(node.Items);

                res.dictVB.Add(node.Id, game.BufferManager.AddVertexData(mapName, false, data));
            }

            return res;
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TerrainGrid()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
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
                if (Game.BufferManager != null)
                {
                    //Remove data from buffer manager
                    foreach (var vb in dictVB.Values)
                    {
                        Game.BufferManager.RemoveVertexData(vb);
                    }
                    foreach (var ib in dictIB.Values)
                    {
                        //Remove data from buffer manager
                        Game.BufferManager.RemoveIndexData(ib);
                    }
                }

                if (dictVB != null)
                {
                    dictVB.Clear();
                    dictVB = null;
                }

                if (dictIB != null)
                {
                    dictIB.Clear();
                    dictIB = null;
                }

                drawingQuadTree = null;

                NodesHigh = null;
                NodesMedium = null;
                NodesLow = null;
                NodesMinimum = null;
            }
        }

        /// <summary>
        /// Updates map from quad-tree and position
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        public void Update(Vector3 eyePosition)
        {
            if (updatingNodes)
            {
                return;
            }

            updatingNodes = true;

            Task.Run(() =>
            {
                UpdateNodes(eyePosition);

                updatingNodes = false;
            });
        }
        /// <summary>
        /// Update terrain nodes
        /// </summary>
        /// <param name="eyePosition">Eye position</param>
        private void UpdateNodes(Vector3 eyePosition)
        {
            var node = drawingQuadTree.FindClosestNode(eyePosition);

            if (node == null || lastNode == node)
            {
                return;
            }

            lastNode = node;

            NodesHigh[0].Set(LevelOfDetail.High, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, node, dictVB, dictIB);
            NodesHigh[1].Set(LevelOfDetail.High, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, node, dictVB, dictIB);
            NodesHigh[2].Set(LevelOfDetail.High, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, node, dictVB, dictIB);
            NodesHigh[3].Set(LevelOfDetail.High, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, node, dictVB, dictIB);
            NodesHigh[4].Set(LevelOfDetail.High, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.CornerTopLeft, node, dictVB, dictIB);
            NodesHigh[5].Set(LevelOfDetail.High, IndexBufferShapes.CornerTopRight, IndexBufferShapes.CornerTopRight, node, dictVB, dictIB);
            NodesHigh[6].Set(LevelOfDetail.High, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.CornerBottomLeft, node, dictVB, dictIB);
            NodesHigh[7].Set(LevelOfDetail.High, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.CornerBottomRight, node, dictVB, dictIB);
            NodesHigh[8].Set(LevelOfDetail.High, IndexBufferShapes.Full, IndexBufferShapes.Full, node, dictVB, dictIB);

            NodesMedium[0].Set(LevelOfDetail.Medium, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, NodesHigh[0].Node, dictVB, dictIB);
            NodesMedium[1].Set(LevelOfDetail.Medium, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, NodesMedium[0].Node, dictVB, dictIB);
            NodesMedium[2].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.SideLeft, NodesMedium[1].Node, dictVB, dictIB);
            NodesMedium[3].Set(LevelOfDetail.Medium, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, NodesMedium[0].Node, dictVB, dictIB);
            NodesMedium[4].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerTopRight, IndexBufferShapes.SideRight, NodesMedium[3].Node, dictVB, dictIB);

            NodesMedium[5].Set(LevelOfDetail.Medium, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, NodesHigh[1].Node, dictVB, dictIB);
            NodesMedium[6].Set(LevelOfDetail.Medium, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, NodesMedium[5].Node, dictVB, dictIB);
            NodesMedium[7].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.SideLeft, NodesMedium[6].Node, dictVB, dictIB);
            NodesMedium[8].Set(LevelOfDetail.Medium, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, NodesMedium[5].Node, dictVB, dictIB);
            NodesMedium[9].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.SideRight, NodesMedium[8].Node, dictVB, dictIB);

            NodesMedium[10].Set(LevelOfDetail.Medium, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, NodesHigh[2].Node, dictVB, dictIB);
            NodesMedium[11].Set(LevelOfDetail.Medium, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, NodesMedium[10].Node, dictVB, dictIB);
            NodesMedium[12].Set(LevelOfDetail.Medium, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, NodesMedium[10].Node, dictVB, dictIB);

            NodesMedium[13].Set(LevelOfDetail.Medium, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, NodesHigh[3].Node, dictVB, dictIB);
            NodesMedium[14].Set(LevelOfDetail.Medium, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, NodesMedium[13].Node, dictVB, dictIB);
            NodesMedium[15].Set(LevelOfDetail.Medium, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, NodesMedium[13].Node, dictVB, dictIB);

            NodesLow[0].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, NodesMedium[0].Node, dictVB, dictIB);
            NodesLow[1].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, NodesLow[0].Node, dictVB, dictIB);
            NodesLow[2].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, NodesLow[1].Node, dictVB, dictIB);
            NodesLow[3].Set(LevelOfDetail.Low, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.SideLeft, NodesLow[2].Node, dictVB, dictIB);
            NodesLow[4].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, NodesLow[0].Node, dictVB, dictIB);
            NodesLow[5].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, NodesLow[4].Node, dictVB, dictIB);
            NodesLow[6].Set(LevelOfDetail.Low, IndexBufferShapes.CornerTopRight, IndexBufferShapes.SideRight, NodesLow[5].Node, dictVB, dictIB);

            NodesLow[7].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, NodesMedium[5].Node, dictVB, dictIB);
            NodesLow[8].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, NodesLow[7].Node, dictVB, dictIB);
            NodesLow[9].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, NodesLow[8].Node, dictVB, dictIB);
            NodesLow[10].Set(LevelOfDetail.Low, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.SideLeft, NodesLow[9].Node, dictVB, dictIB);
            NodesLow[11].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, NodesLow[7].Node, dictVB, dictIB);
            NodesLow[12].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, NodesLow[11].Node, dictVB, dictIB);
            NodesLow[13].Set(LevelOfDetail.Low, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.SideRight, NodesLow[12].Node, dictVB, dictIB);

            NodesLow[14].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, NodesMedium[10].Node, dictVB, dictIB);
            NodesLow[15].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, NodesLow[14].Node, dictVB, dictIB);
            NodesLow[16].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, NodesLow[15].Node, dictVB, dictIB);
            NodesLow[17].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, NodesLow[14].Node, dictVB, dictIB);
            NodesLow[18].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, NodesLow[17].Node, dictVB, dictIB);

            NodesLow[19].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, NodesMedium[13].Node, dictVB, dictIB);
            NodesLow[20].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, NodesLow[19].Node, dictVB, dictIB);
            NodesLow[21].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, NodesLow[20].Node, dictVB, dictIB);
            NodesLow[22].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, NodesLow[19].Node, dictVB, dictIB);
            NodesLow[23].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, NodesLow[22].Node, dictVB, dictIB);

            NodesMinimum[0].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, NodesLow[0].Node, dictVB, dictIB);
            NodesMinimum[1].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, NodesMinimum[0].Node, dictVB, dictIB);
            NodesMinimum[2].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, NodesMinimum[1].Node, dictVB, dictIB);
            NodesMinimum[3].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, NodesMinimum[2].Node, dictVB, dictIB);
            NodesMinimum[4].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.SideLeft, NodesMinimum[3].Node, dictVB, dictIB);
            NodesMinimum[5].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, NodesMinimum[0].Node, dictVB, dictIB);
            NodesMinimum[6].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, NodesMinimum[5].Node, dictVB, dictIB);
            NodesMinimum[7].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, NodesMinimum[6].Node, dictVB, dictIB);
            NodesMinimum[8].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerTopRight, IndexBufferShapes.SideRight, NodesMinimum[7].Node, dictVB, dictIB);

            NodesMinimum[9].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, NodesLow[7].Node, dictVB, dictIB);
            NodesMinimum[10].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, NodesMinimum[9].Node, dictVB, dictIB);
            NodesMinimum[11].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, NodesMinimum[10].Node, dictVB, dictIB);
            NodesMinimum[12].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, NodesMinimum[11].Node, dictVB, dictIB);
            NodesMinimum[13].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.SideLeft, NodesMinimum[12].Node, dictVB, dictIB);
            NodesMinimum[14].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, NodesMinimum[9].Node, dictVB, dictIB);
            NodesMinimum[15].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, NodesMinimum[14].Node, dictVB, dictIB);
            NodesMinimum[16].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, NodesMinimum[15].Node, dictVB, dictIB);
            NodesMinimum[17].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.SideRight, NodesMinimum[16].Node, dictVB, dictIB);

            NodesMinimum[18].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, NodesLow[14].Node, dictVB, dictIB);
            NodesMinimum[19].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, NodesMinimum[18].Node, dictVB, dictIB);
            NodesMinimum[20].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, NodesMinimum[19].Node, dictVB, dictIB);
            NodesMinimum[21].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, NodesMinimum[20].Node, dictVB, dictIB);
            NodesMinimum[22].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, NodesMinimum[18].Node, dictVB, dictIB);
            NodesMinimum[23].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, NodesMinimum[22].Node, dictVB, dictIB);
            NodesMinimum[24].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, NodesMinimum[23].Node, dictVB, dictIB);

            NodesMinimum[25].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, NodesLow[19].Node, dictVB, dictIB);
            NodesMinimum[26].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, NodesMinimum[25].Node, dictVB, dictIB);
            NodesMinimum[27].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, NodesMinimum[26].Node, dictVB, dictIB);
            NodesMinimum[28].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, NodesMinimum[27].Node, dictVB, dictIB);
            NodesMinimum[29].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, NodesMinimum[25].Node, dictVB, dictIB);
            NodesMinimum[30].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, NodesMinimum[29].Node, dictVB, dictIB);
            NodesMinimum[31].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, NodesMinimum[30].Node, dictVB, dictIB);
        }
        /// <summary>
        /// Cull non contained nodes
        /// </summary>
        /// <param name="volume">Volume</param>
        private (TerrainGridNode[] visibleNodesHigh, TerrainGridNode[] visibleNodesMedium, TerrainGridNode[] visibleNodesLow, TerrainGridNode[] visibleNodesMinimum) Cull(ICullingVolume volume)
        {
            var visibleNodesHigh = Array.FindAll(NodesHigh, n => n.Node != null && volume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
            var visibleNodesMedium = Array.FindAll(NodesMedium, n => n.Node != null && volume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
            var visibleNodesLow = Array.FindAll(NodesLow, n => n.Node != null && volume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
            var visibleNodesMinimum = Array.FindAll(NodesMinimum, n => n.Node != null && volume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

            return (visibleNodesHigh, visibleNodesMedium, visibleNodesLow, visibleNodesMinimum);
        }

        /// <summary>
        /// Draws shadows
        /// </summary>
        /// <param name="context">Draw context</param>
        /// <param name="drawer">Drawer</param>
        public bool DrawShadows(DrawContextShadows context, IDrawer drawer)
        {
            var (visibleNodesHigh, visibleNodesMedium, visibleNodesLow, visibleNodesMinimum) = Cull((IntersectionVolumeFrustum)context.Camera.Frustum);

            var dc = context.DeviceContext;
            var r0 = DrawNodeList(dc, drawer, visibleNodesHigh);
            var r1 = DrawNodeList(dc, drawer, visibleNodesMedium);
            var r2 = DrawNodeList(dc, drawer, visibleNodesLow);
            var r3 = DrawNodeList(dc, drawer, visibleNodesMinimum);

            return r0 || r1 || r2 || r3;
        }
        /// <summary>
        /// Draws
        /// </summary>
        /// <param name="context">Draw context</param>
        /// <param name="drawer">Drawer</param>
        public bool Draw(DrawContext context, IDrawer drawer)
        {
            var (visibleNodesHigh, visibleNodesMedium, visibleNodesLow, visibleNodesMinimum) = Cull((IntersectionVolumeFrustum)context.Camera.Frustum);

            var dc = context.DeviceContext;
            var r0 = DrawNodeList(dc, drawer, visibleNodesHigh);
            var r1 = DrawNodeList(dc, drawer, visibleNodesMedium);
            var r2 = DrawNodeList(dc, drawer, visibleNodesLow);
            var r3 = DrawNodeList(dc, drawer, visibleNodesMinimum);

            return r0 || r1 || r2 || r3;
        }
        /// <summary>
        /// Draws the visible node list
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="drawer">Drawer</param>
        /// <param name="nodeList">Node list</param>
        private static bool DrawNodeList(IEngineDeviceContext dc, IDrawer drawer, TerrainGridNode[] nodeList)
        {
            int instanceCount = 0;
            int primitiveCount = 0;

            for (int i = 0; i < nodeList.Length; i++)
            {
                var gNode = nodeList[i];
                if (gNode.IBDesc.Count <= 0)
                {
                    continue;
                }

                var options = new DrawOptions
                {
                    IndexBuffer = gNode.IBDesc,
                    VertexBuffer = gNode.VBDesc,
                    Topology = Topology.TriangleList,
                };
                if (!drawer.Draw(dc, options))
                {
                    continue;
                }

                instanceCount++;
                primitiveCount += gNode.IBDesc.Count / 3;
            }

            return primitiveCount > 0;
        }
    }
}
