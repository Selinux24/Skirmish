using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.BuiltIn;
    using Engine.BuiltIn.Common;
    using Engine.Collections.Generic;
    using Engine.Common;
    using Engine.Content;

    /// <summary>
    /// Terrain class
    /// </summary>
    public sealed class Terrain : Ground<GroundDescription>, IUseMaterials
    {
        #region Helper classes

        /// <summary>
        /// Map grid
        /// </summary>
        class MapGrid : IDisposable
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
            private static BufferDescriptor CreateDescriptor(MapGridShapeId shapeId, int trianglesPerNode, BufferManager bufferManager)
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
            public MapGridNode[] NodesHigh = new MapGridNode[9];
            /// <summary>
            /// Medium resolution nodes
            /// </summary>
            public MapGridNode[] NodesMedium = new MapGridNode[16];
            /// <summary>
            /// Low resolution nodes
            /// </summary>
            public MapGridNode[] NodesLow = new MapGridNode[24];
            /// <summary>
            /// Minimum resolution nodes
            /// </summary>
            public MapGridNode[] NodesMinimum = new MapGridNode[32];
            /// <summary>
            /// Vertex buffer description dictionary
            /// </summary>
            private Dictionary<int, BufferDescriptor> dictVB = new();
            /// <summary>
            /// Index buffer description dictionary
            /// </summary>
            private Dictionary<MapGridShapeId, BufferDescriptor> dictIB = new();
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
            public static async Task<MapGrid> Create(Game game, string mapName, IEnumerable<VertexData> vertices, int trianglesPerNode)
            {
                var res = new MapGrid
                {
                    Game = game,
                };

                //Populate collections
                for (int i = 0; i < res.NodesHigh.Length; i++)
                {
                    res.NodesHigh[i] = new MapGridNode();
                }
                for (int i = 0; i < res.NodesMedium.Length; i++)
                {
                    res.NodesMedium[i] = new MapGridNode();
                }
                for (int i = 0; i < res.NodesLow.Length; i++)
                {
                    res.NodesLow[i] = new MapGridNode();
                }
                for (int i = 0; i < res.NodesMinimum.Length; i++)
                {
                    res.NodesMinimum[i] = new MapGridNode();
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
                        var id = new MapGridShapeId() { LevelOfDetail = lod, Shape = shape };

                        res.dictIB.Add(id, CreateDescriptor(id, trianglesPerNode, game.BufferManager));
                    }
                }

                var bbox = SharpDXExtensions.BoundingBoxFromPoints(vertices.SelectMany(v => v.GetVertices()).Distinct().ToArray());
                var items = vertices.Select(v => (SharpDXExtensions.BoundingBoxFromPoints(v.GetVertices().ToArray()), v));
                res.drawingQuadTree = new QuadTree<VertexData>(bbox, items, LODLevels);

                //Populate nodes dictionary
                var nodes = res.drawingQuadTree.GetLeafNodes();
                foreach (var node in nodes)
                {
                    var data = await VertexData.Convert(VertexTypes.Terrain, node.Items, null, null);

                    res.dictVB.Add(node.Id, game.BufferManager.AddVertexData(mapName, false, data));
                }

                return res;
            }
            /// <summary>
            /// Destructor
            /// </summary>
            ~MapGrid()
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
                if (!updatingNodes)
                {
                    var task = Task.Run(() =>
                    {
                        updatingNodes = true;
                        UpdateNodes(eyePosition);
                    });

                    task.ContinueWith((t) =>
                    {
                        updatingNodes = false;
                    });
                }
            }
            /// <summary>
            /// Update terrain nodes
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            private void UpdateNodes(Vector3 eyePosition)
            {
                var node = drawingQuadTree.FindNode(eyePosition);

                if (node != null && lastNode != node)
                {
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
            }
            /// <summary>
            /// Cull non contained nodes
            /// </summary>
            /// <param name="volume">Volume</param>
            private (MapGridNode[] visibleNodesHigh, MapGridNode[] visibleNodesMedium, MapGridNode[] visibleNodesLow, MapGridNode[] visibleNodesMinimum) Cull(ICullingVolume volume)
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
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="drawer">Drawer</param>
            public bool DrawShadows(DrawContextShadows context, BufferManager bufferManager, IBuiltInDrawer drawer)
            {
                var (visibleNodesHigh, visibleNodesMedium, visibleNodesLow, visibleNodesMinimum) = Cull((IntersectionVolumeFrustum)context.Camera.Frustum);

                var dc = context.DeviceContext;
                var r0 = DrawNodeList(dc, bufferManager, drawer, visibleNodesHigh);
                var r1 = DrawNodeList(dc, bufferManager, drawer, visibleNodesMedium);
                var r2 = DrawNodeList(dc, bufferManager, drawer, visibleNodesLow);
                var r3 = DrawNodeList(dc, bufferManager, drawer, visibleNodesMinimum);

                return r0 || r1 || r2 || r3;
            }
            /// <summary>
            /// Draws
            /// </summary>
            /// <param name="context">Draw context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="drawer">Drawer</param>
            public bool Draw(DrawContext context, BufferManager bufferManager, IBuiltInDrawer drawer)
            {
                var (visibleNodesHigh, visibleNodesMedium, visibleNodesLow, visibleNodesMinimum) = Cull((IntersectionVolumeFrustum)context.Camera.Frustum);

                var dc = context.DeviceContext;
                var r0 = DrawNodeList(dc, bufferManager, drawer, visibleNodesHigh);
                var r1 = DrawNodeList(dc, bufferManager, drawer, visibleNodesMedium);
                var r2 = DrawNodeList(dc, bufferManager, drawer, visibleNodesLow);
                var r3 = DrawNodeList(dc, bufferManager, drawer, visibleNodesMinimum);

                return r0 || r1 || r2 || r3;
            }
            /// <summary>
            /// Draws the visible node list
            /// </summary>
            /// <param name="dc">Device context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="drawer">Drawer</param>
            /// <param name="nodeList">Node list</param>
            private static bool DrawNodeList(IEngineDeviceContext dc, BufferManager bufferManager, IBuiltInDrawer drawer, MapGridNode[] nodeList)
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
                    if (!drawer.Draw(dc, bufferManager, options))
                    {
                        continue;
                    }

                    instanceCount++;
                    primitiveCount += gNode.IBDesc.Count / 3;
                }

                return primitiveCount > 0;
            }
        }
        /// <summary>
        /// Map grid node
        /// </summary>
        class MapGridNode
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
            public MapGridNode()
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
                Dictionary<MapGridShapeId, BufferDescriptor> dictIB)
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
                    IBDesc = dictIB[new MapGridShapeId() { LevelOfDetail = lod, Shape = shape }];
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
        /// <summary>
        /// Map grid shape Id
        /// </summary>
        struct MapGridShapeId : IEquatable<MapGridShapeId>
        {
            /// <summary>
            /// Level of detail
            /// </summary>
            public LevelOfDetail LevelOfDetail { get; set; }
            /// <summary>
            /// Shape
            /// </summary>
            public IndexBufferShapes Shape { get; set; }

            /// <summary>
            /// Equal to operator
            /// </summary>
            /// <param name="mgShape1">Shape 1</param>
            /// <param name="mgShape2">Shape 2</param>
            /// <returns>Returns true if both instances are equal</returns>
            public static bool operator ==(MapGridShapeId mgShape1, MapGridShapeId mgShape2)
            {
                return mgShape1.Equals(mgShape2);
            }
            /// <summary>
            /// Not equal operator
            /// </summary>
            /// <param name="mgShape1">Shape 1</param>
            /// <param name="mgShape2">Shape 2</param>
            /// <returns>Returns true if both instances are different</returns>
            public static bool operator !=(MapGridShapeId mgShape1, MapGridShapeId mgShape2)
            {
                return !(mgShape1.Equals(mgShape2));
            }
            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type
            /// </summary>
            /// <param name="other">An object to compare with this object</param>
            /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
            public readonly bool Equals(MapGridShapeId other)
            {
                if (LevelOfDetail == other.LevelOfDetail && Shape == other.Shape)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type
            /// </summary>
            /// <param name="other">An object to compare with this object</param>
            /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
            public override readonly bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                if (obj is MapGridShapeId shape)
                {
                    return Equals(shape);
                }

                return false;
            }
            /// <summary>
            /// Serves as the default hash function
            /// </summary>
            /// <returns>A hash code for the current object</returns>
            public override readonly int GetHashCode()
            {
                return HashCode.Combine(LevelOfDetail, Shape);
            }
        }

        #endregion

        /// <summary>
        /// Grid
        /// </summary>
        private MapGrid mapGrid = null;
        /// <summary>
        /// Height map
        /// </summary>
        private HeightMap heightMap = null;

        /// <summary>
        /// Height-map texture resolution
        /// </summary>
        private float textureResolution;
        /// <summary>
        /// Terrain material
        /// </summary>
        private IMeshMaterial terrainMaterial;
        /// <summary>
        /// Gets or sets the terrain drawing mode
        /// </summary>
        private BuiltInTerrainModes terrainMode;
        /// <summary>
        /// Lerp proportion between alpha mapping and slope texturing
        /// </summary>
        private float proportion;
        /// <summary>
        /// Terrain low res textures
        /// </summary>
        private EngineShaderResourceView terrainTexturesLR = null;
        /// <summary>
        /// Terrain high res textures
        /// </summary>
        private EngineShaderResourceView terrainTexturesHR = null;
        /// <summary>
        /// Terrain normal maps
        /// </summary>
        private EngineShaderResourceView terrainNormalMaps = null;
        /// <summary>
        /// Color textures for alpha map
        /// </summary>
        private EngineShaderResourceView colorTextures = null;
        /// <summary>
        /// Alpha map
        /// </summary>
        private EngineShaderResourceView alphaMap = null;
        /// <summary>
        /// Use anisotropic
        /// </summary>
        private bool useAnisotropic = false;
        /// <summary>
        /// Slope ranges
        /// </summary>
        private Vector2 slopeRanges = Vector2.Zero;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="id">Id</param>
        /// <param name="name">Name</param>
        public Terrain(Scene scene, string id, string name)
            : base(scene, id, name)
        {

        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~Terrain()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mapGrid?.Dispose();
                mapGrid = null;

                heightMap?.Dispose();
                heightMap = null;

                terrainTexturesLR?.Dispose();
                terrainTexturesLR = null;
                terrainTexturesHR?.Dispose();
                terrainTexturesHR = null;
                terrainNormalMaps?.Dispose();
                terrainNormalMaps = null;
                colorTextures?.Dispose();
                colorTextures = null;
                alphaMap?.Dispose();
                alphaMap = null;
            }
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(GroundDescription description)
        {
            await base.ReadAssets(description);

            if (Description.Heightmap == null)
            {
                throw new EngineException($"Terrain initialization error. Height-map description not found.");
            }

            useAnisotropic = Description.UseAnisotropic;

            // Read height-map
            heightMap = HeightMap.FromDescription(Description.Heightmap);
            float heightMapCellSize = Description.Heightmap.CellSize;
            float heightMapHeight = Description.Heightmap.MaximumHeight;
            Curve heightMapCurve = Description.Heightmap.HeightCurve;
            float uvScale = 1;
            Vector2 uvDisplacement = Vector2.Zero;

            if (Description.Heightmap.Textures != null)
            {
                // Read texture data
                uvScale = Description.Heightmap.Textures.Scale;
                uvDisplacement = Description.Heightmap.Textures.Displacement;
                proportion = Description.Heightmap.Textures.Proportion;
                textureResolution = Description.Heightmap.Textures.Resolution;
                slopeRanges = Description.Heightmap.Textures.SlopeRanges;

                bool useAlphaMap = Description.Heightmap.Textures.UseAlphaMapping;
                bool useSlopes = Description.Heightmap.Textures.UseSlopes;
                terrainMode = BuiltInTerrainModes.AlphaMap;
                if (useAlphaMap && useSlopes) { terrainMode = BuiltInTerrainModes.Full; }
                if (useSlopes) { terrainMode = BuiltInTerrainModes.Slopes; }

                await ReadHeightmapTextures(Description.Heightmap.ContentPath, Description.Heightmap.Textures);
            }

            // Read material
            terrainMaterial = MeshMaterial.FromMaterial(MaterialBlinnPhong.Default);

            // Get vertices and indices from height-map
            var (Vertices, Indices) = await heightMap.BuildGeometry(
                heightMapCellSize,
                heightMapHeight,
                heightMapCurve,
                uvScale,
                uvDisplacement);

            // Compute triangles for ray - mesh picking
            var tris = Triangle.ComputeTriangleList(
                Topology.TriangleList,
                Vertices.Select(v => v.Position.Value).ToArray(),
                Indices.ToArray());

            // Initialize quad-tree for ray picking
            GroundPickingQuadtree = Description.ReadQuadTree(tris);

            //Initialize map
            int trianglesPerNode = heightMap.CalcTrianglesPerNode(MapGrid.LODLevels);
            mapGrid = await MapGrid.Create(Game, $"Terrain.{Name}", Vertices, trianglesPerNode);
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            mapGrid?.Update(Scene.Camera.Position);
        }
        /// <inheritdoc/>
        public override bool DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return false;
            }

            if (mapGrid == null)
            {
                return false;
            }

            var shadowDrawer = context.ShadowMap?.GetDrawer(VertexTypes.Terrain, false, false);
            if (shadowDrawer == null)
            {
                return false;
            }

            shadowDrawer.UpdateCastingLight(context);

            var meshState = new BuiltInDrawerMeshState
            {
                Local = Matrix.Identity,
            };
            shadowDrawer.UpdateMesh(context.DeviceContext, meshState);

            return mapGrid.DrawShadows(context, BufferManager, shadowDrawer);
        }
        /// <inheritdoc/>
        public override bool Draw(DrawContext context)
        {
            if (!Visible)
            {
                return false;
            }

            if (mapGrid == null)
            {
                return false;
            }

            var terrainDrawer = GetDrawer(context);
            if (terrainDrawer == null)
            {
                return false;
            }

            return mapGrid.Draw(context, BufferManager, terrainDrawer);
        }
        /// <summary>
        /// Gets the terrain drawer, based on the drawing context
        /// </summary>
        /// <param name="context">Drawing context</param>
        private IBuiltInDrawer GetDrawer(DrawContext context)
        {
            if (context.DrawerMode.HasFlag(DrawerModes.Forward))
            {
                var dr = BuiltInShaders.GetDrawer<BuiltIn.Forward.BuiltInTerrain>();
                dr.Update(context.DeviceContext, GetTerrainState());
                return dr;
            }

            if (context.DrawerMode.HasFlag(DrawerModes.Deferred))
            {
                var dr = BuiltInShaders.GetDrawer<BuiltIn.Deferred.BuiltInTerrain>();
                dr.Update(context.DeviceContext, GetTerrainState());
                return dr;
            }

            return null;
        }
        /// <summary>
        /// Gets the terrain state
        /// </summary>
        private BuiltInTerrainState GetTerrainState()
        {
            return new BuiltInTerrainState
            {
                TintColor = Color.White,
                MaterialIndex = terrainMaterial.ResourceIndex,
                Mode = terrainMode,
                TextureResolution = textureResolution,
                Proportion = proportion,
                SlopeRanges = slopeRanges,
                AlphaMap = alphaMap,
                MormalMap = terrainNormalMaps,
                ColorTexture = colorTextures,
                LowResolutionTexture = terrainTexturesLR,
                HighResolutionTexture = terrainTexturesHR,
                UseAnisotropic = useAnisotropic,
            };
        }

        /// <summary>
        /// Reads texture data
        /// </summary>
        /// <param name="baseContentPath">Base content path</param>
        /// <param name="description">Textures description</param>
        private async Task ReadHeightmapTextures(string baseContentPath, HeightmapTexturesDescription description)
        {
            string tContentPath = Path.Combine(baseContentPath, description.ContentPath);

            var normalMapTextures = new FileArrayImageContent(tContentPath, description.NormalMaps);
            terrainNormalMaps = await Game.ResourceManager.RequestResource(normalMapTextures);

            if (description.UseSlopes)
            {
                var texturesLR = new FileArrayImageContent(tContentPath, description.TexturesLR);
                var texturesHR = new FileArrayImageContent(tContentPath, description.TexturesHR);

                terrainTexturesLR = await Game.ResourceManager.RequestResource(texturesLR);
                terrainTexturesHR = await Game.ResourceManager.RequestResource(texturesHR);
            }

            if (description.UseAlphaMapping)
            {
                var colors = new FileArrayImageContent(tContentPath, description.ColorTextures);
                var aMap = new FileArrayImageContent(tContentPath, description.AlphaMap);

                colorTextures = await Game.ResourceManager.RequestResource(colors);
                alphaMap = await Game.ResourceManager.RequestResource(aMap);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<IMeshMaterial> GetMaterials()
        {
            return terrainMaterial != null ? new[] { terrainMaterial } : Enumerable.Empty<IMeshMaterial>();
        }
        /// <inheritdoc/>
        public IMeshMaterial GetMaterial(string meshMaterialName)
        {
            return terrainMaterial;
        }
        /// <inheritdoc/>
        public bool ReplaceMaterial(string meshMaterialName, IMeshMaterial material)
        {
            if (terrainMaterial == material)
            {
                return false;
            }

            terrainMaterial = material;

            return true;
        }
    }
}
