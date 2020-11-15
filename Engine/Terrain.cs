using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Engine
{
    using Engine.Collections.Generic;
    using Engine.Common;
    using Engine.Content;
    using Engine.Effects;

    /// <summary>
    /// Terrain class
    /// </summary>
    public class Terrain : Ground, IUseMaterials
    {
        #region Helper classes

        /// <summary>
        /// Map grid
        /// </summary>
        class MapGrid : IDisposable
        {
            /// <summary>
            /// Level of detail leves in map
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
            private Dictionary<int, BufferDescriptor> dictVB = new Dictionary<int, BufferDescriptor>();
            /// <summary>
            /// Index buffer description dictionary
            /// </summary>
            private Dictionary<MapGridShapeId, BufferDescriptor> dictIB = new Dictionary<MapGridShapeId, BufferDescriptor>();
            /// <summary>
            /// Tree
            /// </summary>
            private QuadTree<VertexData> drawingQuadTree = null;
            /// <summary>
            /// Last mapped node
            /// </summary>
            private QuadTreeNode<VertexData> lastNode = null;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game instance</param>
            /// <param name="mapName">Map name</param>
            /// <param name="vertices">Vertices to map</param>
            /// <param name="trianglesPerNode">Triangles per terrain node</param>
            public MapGrid(Game game, string mapName, IEnumerable<VertexData> vertices, int trianglesPerNode)
            {
                Game = game;

                //Populate collections
                for (int i = 0; i < NodesHigh.Length; i++)
                {
                    NodesHigh[i] = new MapGridNode();
                }
                for (int i = 0; i < NodesMedium.Length; i++)
                {
                    NodesMedium[i] = new MapGridNode();
                }
                for (int i = 0; i < NodesLow.Length; i++)
                {
                    NodesLow[i] = new MapGridNode();
                }
                for (int i = 0; i < NodesMinimum.Length; i++)
                {
                    NodesMinimum[i] = new MapGridNode();
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

                //Populate shapes dictionaty
                foreach (var lod in lodList)
                {
                    foreach (var shape in shapeList)
                    {
                        var id = new MapGridShapeId() { LevelOfDetail = lod, Shape = shape };

                        dictIB.Add(id, CreateDescriptor(id, trianglesPerNode, game.BufferManager));
                    }
                }

                drawingQuadTree = new QuadTree<VertexData>(vertices, LODLevels);

                //Populate nodes dictionary
                var nodes = drawingQuadTree.GetLeafNodes();
                foreach (var node in nodes)
                {
                    var data = VertexData.Convert(VertexTypes.Terrain, node.Items, null, null);

                    dictVB.Add(node.Id, game.BufferManager.AddVertexData(mapName, false, data));
                }
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
            /// Draw shadows 
            /// </summary>
            /// <param name="context">Draw context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="terrainTechnique">Technique for drawing</param>
            public void DrawShadows(DrawContextShadows context, BufferManager bufferManager, EngineEffectTechnique terrainTechnique)
            {
                var visibleNodesHigh = Array.FindAll(NodesHigh, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMedium = Array.FindAll(NodesMedium, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesLow = Array.FindAll(NodesLow, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMinimum = Array.FindAll(NodesMinimum, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

                DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesHigh);
                DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesMedium);
                DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesLow);
                DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesMinimum);
            }
            /// <summary>
            /// Draw
            /// </summary>
            /// <param name="context">Draw context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="terrainTechnique">Technique for drawing</param>
            public void Draw(DrawContext context, BufferManager bufferManager, EngineEffectTechnique terrainTechnique)
            {
                var visibleNodesHigh = Array.FindAll(NodesHigh, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMedium = Array.FindAll(NodesMedium, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesLow = Array.FindAll(NodesLow, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMinimum = Array.FindAll(NodesMinimum, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

                var mode = context.DrawerMode;
                DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesHigh);
                DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesMedium);
                DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesLow);
                DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesMinimum);
            }
            /// <summary>
            /// Draws the visible node list
            /// </summary>
            /// <param name="mode">Drawer mode</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="terrainTechnique">Technique for drawing</param>
            /// <param name="nodeList">Node list</param>
            private void DrawNodeList(DrawerModes mode, BufferManager bufferManager, EngineEffectTechnique terrainTechnique, MapGridNode[] nodeList)
            {
                var graphics = Game.Graphics;

                for (int i = 0; i < nodeList.Length; i++)
                {
                    var gNode = nodeList[i];
                    if (gNode.IBDesc.Count > 0)
                    {
                        bufferManager.SetInputAssembler(terrainTechnique, gNode.VBDesc, Topology.TriangleList);
                        bufferManager.SetIndexBuffer(gNode.IBDesc);

                        if (!mode.HasFlag(DrawerModes.ShadowMap))
                        {
                            Counters.InstancesPerFrame++;
                            Counters.PrimitivesPerFrame += gNode.IBDesc.Count / 3;
                        }

                        for (int p = 0; p < terrainTechnique.PassCount; p++)
                        {
                            graphics.EffectPassApply(terrainTechnique, p, 0);

                            graphics.DrawIndexed(
                                gNode.IBDesc.Count,
                                gNode.IBDesc.BufferOffset,
                                gNode.VBDesc.BufferOffset);
                        }
                    }
                }
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
            public bool Equals(MapGridShapeId other)
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
            public override bool Equals(object obj)
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
            public override int GetHashCode()
            {
                return LevelOfDetail.GetHashCode() ^ Shape.GetHashCode();
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
        /// Heightmap texture resolution
        /// </summary>
        private readonly float textureResolution;
        /// <summary>
        /// Terrain material
        /// </summary>
        private readonly IMeshMaterial terrainMaterial;

        /// <summary>
        /// Gets or sets whether use alpha mapping or not
        /// </summary>
        private readonly bool useAlphaMap;
        /// <summary>
        /// Gets or sets whether use slope texturing or not
        /// </summary>
        private readonly bool useSlopes;
        /// <summary>
        /// Lerping proportion between alhpa mapping and slope texturing
        /// </summary>
        private readonly float proportion;
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
        private readonly bool useAnisotropic = false;
        /// <summary>
        /// Slope ranges
        /// </summary>
        private readonly Vector2 slopeRanges = Vector2.Zero;

        /// <summary>
        /// Gets the used material list
        /// </summary>
        public virtual IEnumerable<IMeshMaterial> Materials
        {
            get
            {
                return new[] { terrainMaterial };
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="scene">Scene</param>
        /// <param name="description">Terrain description</param>
        public Terrain(string name, Scene scene, GroundDescription description)
            : base(name, scene, description)
        {
            useAnisotropic = description.UseAnisotropic;

            if (description.Heightmap == null)
            {
                throw new EngineException($"Terrain initialization error. Heightmap description not found.");
            }

            // Read heightmap
            heightMap = HeightMap.FromDescription(description.Heightmap);
            float heightMapCellSize = description.Heightmap.CellSize;
            float heightMapHeight = description.Heightmap.MaximumHeight;
            Curve heightMapCurve = description.Heightmap.HeightCurve;
            float uvScale = 1;
            Vector2 uvDisplacement = Vector2.Zero;

            if (description.Heightmap.Textures != null)
            {
                // Read texture data
                uvScale = description.Heightmap.Textures.Scale;
                uvDisplacement = description.Heightmap.Textures.Displacement;
                useAlphaMap = description.Heightmap.Textures.UseAlphaMapping;
                useSlopes = description.Heightmap.Textures.UseSlopes;
                proportion = description.Heightmap.Textures.Proportion;
                textureResolution = description.Heightmap.Textures.Resolution;
                slopeRanges = description.Heightmap.Textures.SlopeRanges;

                ReadHeightmapTextures(description.Heightmap.ContentPath, description.Heightmap.Textures);
            }

            // Read material
            terrainMaterial = MeshMaterial.DefaultBlinnPhong;

            // Get vertices and indices from heightmap
            heightMap.BuildGeometry(
                heightMapCellSize,
                heightMapHeight,
                heightMapCurve,
                uvScale,
                uvDisplacement,
                out var vertices, out var indices);

            // Compute triangles for ray - mesh picking
            var tris = Triangle.ComputeTriangleList(
                Topology.TriangleList,
                vertices.Select(v => v.Position.Value).ToArray(),
                indices.ToArray());

            // Initialize quadtree for ray picking
            groundPickingQuadtree = description.ReadQuadTree(tris);

            //Initialize map
            int trianglesPerNode = heightMap.CalcTrianglesPerNode(MapGrid.LODLevels);
            mapGrid = new MapGrid(Game, $"Terrain.{Name}", vertices, trianglesPerNode);
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
        public override void Update(UpdateContext context)
        {
            if (!Active)
            {
                return;
            }

            mapGrid?.Update(context.EyePosition);
        }
        /// <inheritdoc/>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return;
            }

            var terrainTechnique = SetTechniqueTerrainShadowMap(context);
            if (terrainTechnique != null)
            {
                mapGrid?.DrawShadows(context, BufferManager, terrainTechnique);
            }
        }
        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            var terrainTechnique = SetTechniqueTerrain(context);
            if (terrainTechnique != null)
            {
                mapGrid?.Draw(context, BufferManager, terrainTechnique);
            }
        }
        /// <summary>
        /// Sets thecnique for terrain drawing
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueTerrain(DrawContext context)
        {
            var mode = context.DrawerMode;

            EngineEffectTechnique terrainTechnique = null;
            if (mode.HasFlag(DrawerModes.Forward)) terrainTechnique = SetTechniqueTerrainDefault(context);
            if (mode.HasFlag(DrawerModes.Deferred)) terrainTechnique = SetTechniqueTerrainDeferred(context);

            return terrainTechnique;
        }
        /// <summary>
        /// Sets thecnique for terrain drawing with forward renderer
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueTerrainDefault(DrawContext context)
        {
            var effect = DrawerPool.EffectDefaultTerrain;

            effect.UpdatePerFrame(
                textureResolution,
                context);

            var state = new EffectTerrainState
            {
                UseAnisotropic = useAnisotropic,
                NormalMap = terrainNormalMaps,
                UseAlphaMap = useAlphaMap,
                AlphaMap = alphaMap,
                ColorTextures = colorTextures,
                UseSlopes = useSlopes,
                SlopeRanges = slopeRanges,
                DiffuseMapLR = terrainTexturesLR,
                DiffuseMapHR = terrainTexturesHR,
                Proportion = proportion,
                MaterialIndex = terrainMaterial.ResourceIndex,
            };

            effect.UpdatePerObject(state);

            if (useAlphaMap && useSlopes) { return effect.TerrainFullForward; }
            if (useAlphaMap) { return effect.TerrainAlphaMapForward; }
            if (useSlopes) { return effect.TerrainSlopesForward; }

            return null;
        }
        /// <summary>
        /// Sets thecnique for terrain drawing with deferred renderer
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueTerrainDeferred(DrawContext context)
        {
            var effect = DrawerPool.EffectDeferredTerrain;

            effect.UpdatePerFrame(
                context.ViewProjection,
                textureResolution);

            var state = new EffectTerrainState
            {
                UseAnisotropic = useAnisotropic,
                NormalMap = terrainNormalMaps,
                UseAlphaMap = useAlphaMap,
                AlphaMap = alphaMap,
                ColorTextures = colorTextures,
                UseSlopes = useSlopes,
                SlopeRanges = slopeRanges,
                DiffuseMapLR = terrainTexturesLR,
                DiffuseMapHR = terrainTexturesHR,
                Proportion = proportion,
                MaterialIndex = terrainMaterial.ResourceIndex,
            };

            effect.UpdatePerObject(state);

            if (useAlphaMap && useSlopes) { return effect.TerrainFullDeferred; }
            if (useAlphaMap) { return effect.TerrainAlphaMapDeferred; }
            if (useSlopes) { return effect.TerrainSlopesDeferred; }

            return null;
        }
        /// <summary>
        /// Sets thecnique for terrain drawing in shadow mapping
        /// </summary>
        /// <param name="context">Drawing context</param>
        /// <returns>Returns the selected technique</returns>
        private EngineEffectTechnique SetTechniqueTerrainShadowMap(DrawContextShadows context)
        {
            var effect = DrawerPool.EffectShadowTerrain;

            effect.UpdatePerFrame(context.ViewProjection);

            return effect.TerrainShadowMap;
        }

        /// <summary>
        /// Reads texture data
        /// </summary>
        /// <param name="baseContentPath">Base content path</param>
        /// <param name="description">Textures description</param>
        private void ReadHeightmapTextures(string baseContentPath, HeightmapTexturesDescription description)
        {
            string tContentPath = Path.Combine(baseContentPath, description.ContentPath);

            var normalMapTextures = ImageContent.Array(tContentPath, description.NormalMaps);
            terrainNormalMaps = Game.ResourceManager.RequestResource(normalMapTextures);

            if (description.UseSlopes)
            {
                var texturesLR = ImageContent.Array(tContentPath, description.TexturesLR);
                var texturesHR = ImageContent.Array(tContentPath, description.TexturesHR);

                terrainTexturesLR = Game.ResourceManager.RequestResource(texturesLR);
                terrainTexturesHR = Game.ResourceManager.RequestResource(texturesHR);
            }

            if (description.UseAlphaMapping)
            {
                var colors = ImageContent.Array(tContentPath, description.ColorTextures);
                var aMap = ImageContent.Texture(tContentPath, description.AlphaMap);

                colorTextures = Game.ResourceManager.RequestResource(colors);
                alphaMap = Game.ResourceManager.RequestResource(aMap);
            }
        }
    }

    /// <summary>
    /// Terrain extensions
    /// </summary>
    public static class TerrainExtensions
    {
        /// <summary>
        /// Adds a component to the scene
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="layer">Processing layer</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Terrain> AddComponentTerrain(this Scene scene, string name, GroundDescription description, SceneObjectUsages usage = SceneObjectUsages.Ground, int layer = Scene.LayerDefault)
        {
            Terrain component = null;

            await Task.Run(() =>
            {
                component = new Terrain(name, scene, description);

                scene.AddComponent(component, usage, layer);
            });

            return component;
        }
    }
}
