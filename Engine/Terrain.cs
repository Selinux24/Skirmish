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
                this.Game = game;

                //Populate collections
                for (int i = 0; i < this.NodesHigh.Length; i++)
                {
                    this.NodesHigh[i] = new MapGridNode();
                }
                for (int i = 0; i < this.NodesMedium.Length; i++)
                {
                    this.NodesMedium[i] = new MapGridNode();
                }
                for (int i = 0; i < this.NodesLow.Length; i++)
                {
                    this.NodesLow[i] = new MapGridNode();
                }
                for (int i = 0; i < this.NodesMinimum.Length; i++)
                {
                    this.NodesMinimum[i] = new MapGridNode();
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

                        this.dictIB.Add(id, CreateDescriptor(id, trianglesPerNode, game.BufferManager));
                    }
                }

                this.drawingQuadTree = new QuadTree<VertexData>(vertices, LODLevels);

                //Populate nodes dictionary
                var nodes = this.drawingQuadTree.GetLeafNodes();
                foreach (var node in nodes)
                {
                    var data = VertexData.Convert(VertexTypes.Terrain, node.Items, null, null);

                    this.dictVB.Add(node.Id, game.BufferManager.AddVertexData(mapName, false, data));
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
                    if (this.Game.BufferManager != null)
                    {
                        //Remove data from buffer manager
                        foreach (var vb in this.dictVB.Values)
                        {
                            this.Game.BufferManager.RemoveVertexData(vb);
                        }
                        foreach (var ib in this.dictIB.Values)
                        {
                            //Remove data from buffer manager
                            this.Game.BufferManager.RemoveIndexData(ib);
                        }
                    }

                    if (this.dictVB != null)
                    {
                        this.dictVB.Clear();
                        this.dictVB = null;
                    }

                    if (this.dictIB != null)
                    {
                        this.dictIB.Clear();
                        this.dictIB = null;
                    }

                    this.drawingQuadTree = null;

                    this.NodesHigh = null;
                    this.NodesMedium = null;
                    this.NodesLow = null;
                    this.NodesMinimum = null;
                }
            }

            /// <summary>
            /// Updates map from quad-tree and position
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            public void Update(Vector3 eyePosition)
            {
                if (!this.updatingNodes)
                {
                    var task = Task.Run(() =>
                    {
                        this.updatingNodes = true;
                        this.UpdateNodes(eyePosition);
                    });

                    task.ContinueWith((t) =>
                    {
                        this.updatingNodes = false;
                    });
                }
            }
            /// <summary>
            /// Update terrain nodes
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            private void UpdateNodes(Vector3 eyePosition)
            {
                var node = this.drawingQuadTree.FindNode(eyePosition);

                if (node != null && this.lastNode != node)
                {
                    this.lastNode = node;

                    this.NodesHigh[0].Set(LevelOfDetail.High, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, node, this.dictVB, this.dictIB);
                    this.NodesHigh[1].Set(LevelOfDetail.High, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, node, this.dictVB, this.dictIB);
                    this.NodesHigh[2].Set(LevelOfDetail.High, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, node, this.dictVB, this.dictIB);
                    this.NodesHigh[3].Set(LevelOfDetail.High, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, node, this.dictVB, this.dictIB);
                    this.NodesHigh[4].Set(LevelOfDetail.High, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.CornerTopLeft, node, this.dictVB, this.dictIB);
                    this.NodesHigh[5].Set(LevelOfDetail.High, IndexBufferShapes.CornerTopRight, IndexBufferShapes.CornerTopRight, node, this.dictVB, this.dictIB);
                    this.NodesHigh[6].Set(LevelOfDetail.High, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.CornerBottomLeft, node, this.dictVB, this.dictIB);
                    this.NodesHigh[7].Set(LevelOfDetail.High, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.CornerBottomRight, node, this.dictVB, this.dictIB);
                    this.NodesHigh[8].Set(LevelOfDetail.High, IndexBufferShapes.Full, IndexBufferShapes.Full, node, this.dictVB, this.dictIB);

                    this.NodesMedium[0].Set(LevelOfDetail.Medium, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, this.NodesHigh[0].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[1].Set(LevelOfDetail.Medium, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, this.NodesMedium[0].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[2].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.SideLeft, this.NodesMedium[1].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[3].Set(LevelOfDetail.Medium, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, this.NodesMedium[0].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[4].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerTopRight, IndexBufferShapes.SideRight, this.NodesMedium[3].Node, this.dictVB, this.dictIB);

                    this.NodesMedium[5].Set(LevelOfDetail.Medium, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, this.NodesHigh[1].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[6].Set(LevelOfDetail.Medium, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, this.NodesMedium[5].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[7].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.SideLeft, this.NodesMedium[6].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[8].Set(LevelOfDetail.Medium, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, this.NodesMedium[5].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[9].Set(LevelOfDetail.Medium, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.SideRight, this.NodesMedium[8].Node, this.dictVB, this.dictIB);

                    this.NodesMedium[10].Set(LevelOfDetail.Medium, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, this.NodesHigh[2].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[11].Set(LevelOfDetail.Medium, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, this.NodesMedium[10].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[12].Set(LevelOfDetail.Medium, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, this.NodesMedium[10].Node, this.dictVB, this.dictIB);

                    this.NodesMedium[13].Set(LevelOfDetail.Medium, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, this.NodesHigh[3].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[14].Set(LevelOfDetail.Medium, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, this.NodesMedium[13].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[15].Set(LevelOfDetail.Medium, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, this.NodesMedium[13].Node, this.dictVB, this.dictIB);

                    this.NodesLow[0].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, this.NodesMedium[0].Node, this.dictVB, this.dictIB);
                    this.NodesLow[1].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, this.NodesLow[0].Node, this.dictVB, this.dictIB);
                    this.NodesLow[2].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, this.NodesLow[1].Node, this.dictVB, this.dictIB);
                    this.NodesLow[3].Set(LevelOfDetail.Low, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.SideLeft, this.NodesLow[2].Node, this.dictVB, this.dictIB);
                    this.NodesLow[4].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, this.NodesLow[0].Node, this.dictVB, this.dictIB);
                    this.NodesLow[5].Set(LevelOfDetail.Low, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, this.NodesLow[4].Node, this.dictVB, this.dictIB);
                    this.NodesLow[6].Set(LevelOfDetail.Low, IndexBufferShapes.CornerTopRight, IndexBufferShapes.SideRight, this.NodesLow[5].Node, this.dictVB, this.dictIB);

                    this.NodesLow[7].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, this.NodesMedium[5].Node, this.dictVB, this.dictIB);
                    this.NodesLow[8].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, this.NodesLow[7].Node, this.dictVB, this.dictIB);
                    this.NodesLow[9].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, this.NodesLow[8].Node, this.dictVB, this.dictIB);
                    this.NodesLow[10].Set(LevelOfDetail.Low, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.SideLeft, this.NodesLow[9].Node, this.dictVB, this.dictIB);
                    this.NodesLow[11].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, this.NodesLow[7].Node, this.dictVB, this.dictIB);
                    this.NodesLow[12].Set(LevelOfDetail.Low, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, this.NodesLow[11].Node, this.dictVB, this.dictIB);
                    this.NodesLow[13].Set(LevelOfDetail.Low, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.SideRight, this.NodesLow[12].Node, this.dictVB, this.dictIB);

                    this.NodesLow[14].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, this.NodesMedium[10].Node, this.dictVB, this.dictIB);
                    this.NodesLow[15].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, this.NodesLow[14].Node, this.dictVB, this.dictIB);
                    this.NodesLow[16].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, this.NodesLow[15].Node, this.dictVB, this.dictIB);
                    this.NodesLow[17].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, this.NodesLow[14].Node, this.dictVB, this.dictIB);
                    this.NodesLow[18].Set(LevelOfDetail.Low, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, this.NodesLow[17].Node, this.dictVB, this.dictIB);

                    this.NodesLow[19].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, this.NodesMedium[13].Node, this.dictVB, this.dictIB);
                    this.NodesLow[20].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, this.NodesLow[19].Node, this.dictVB, this.dictIB);
                    this.NodesLow[21].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, this.NodesLow[20].Node, this.dictVB, this.dictIB);
                    this.NodesLow[22].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, this.NodesLow[19].Node, this.dictVB, this.dictIB);
                    this.NodesLow[23].Set(LevelOfDetail.Low, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, this.NodesLow[22].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[0].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideTop, this.NodesLow[0].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[1].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, this.NodesMinimum[0].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[2].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, this.NodesMinimum[1].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[3].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideLeft, this.NodesMinimum[2].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[4].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerTopLeft, IndexBufferShapes.SideLeft, this.NodesMinimum[3].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[5].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, this.NodesMinimum[0].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[6].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, this.NodesMinimum[5].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[7].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideTop, IndexBufferShapes.SideRight, this.NodesMinimum[6].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[8].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerTopRight, IndexBufferShapes.SideRight, this.NodesMinimum[7].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[9].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideBottom, this.NodesLow[7].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[10].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, this.NodesMinimum[9].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[11].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, this.NodesMinimum[10].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[12].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideLeft, this.NodesMinimum[11].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[13].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerBottomLeft, IndexBufferShapes.SideLeft, this.NodesMinimum[12].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[14].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, this.NodesMinimum[9].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[15].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, this.NodesMinimum[14].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[16].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideBottom, IndexBufferShapes.SideRight, this.NodesMinimum[15].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[17].Set(LevelOfDetail.Minimum, IndexBufferShapes.CornerBottomRight, IndexBufferShapes.SideRight, this.NodesMinimum[16].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[18].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideLeft, this.NodesLow[14].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[19].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, this.NodesMinimum[18].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[20].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, this.NodesMinimum[19].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[21].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideTop, this.NodesMinimum[20].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[22].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, this.NodesMinimum[18].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[23].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, this.NodesMinimum[22].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[24].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideLeft, IndexBufferShapes.SideBottom, this.NodesMinimum[23].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[25].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideRight, this.NodesLow[19].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[26].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, this.NodesMinimum[25].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[27].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, this.NodesMinimum[26].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[28].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideTop, this.NodesMinimum[27].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[29].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, this.NodesMinimum[25].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[30].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, this.NodesMinimum[29].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[31].Set(LevelOfDetail.Minimum, IndexBufferShapes.SideRight, IndexBufferShapes.SideBottom, this.NodesMinimum[30].Node, this.dictVB, this.dictIB);
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
                var visibleNodesHigh = Array.FindAll(this.NodesHigh, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMedium = Array.FindAll(this.NodesMedium, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesLow = Array.FindAll(this.NodesLow, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMinimum = Array.FindAll(this.NodesMinimum, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

                this.DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesHigh);
                this.DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesMedium);
                this.DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesLow);
                this.DrawNodeList(DrawerModes.ShadowMap, bufferManager, terrainTechnique, visibleNodesMinimum);
            }
            /// <summary>
            /// Draw
            /// </summary>
            /// <param name="context">Draw context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="terrainTechnique">Technique for drawing</param>
            public void Draw(DrawContext context, BufferManager bufferManager, EngineEffectTechnique terrainTechnique)
            {
                var visibleNodesHigh = Array.FindAll(this.NodesHigh, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMedium = Array.FindAll(this.NodesMedium, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesLow = Array.FindAll(this.NodesLow, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                var visibleNodesMinimum = Array.FindAll(this.NodesMinimum, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

                var mode = context.DrawerMode;
                this.DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesHigh);
                this.DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesMedium);
                this.DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesLow);
                this.DrawNodeList(mode, bufferManager, terrainTechnique, visibleNodesMinimum);
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
                var graphics = this.Game.Graphics;

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

                if (this.Node != nNode)
                {
                    //Set buffer (VX)
                    if (nNode != null)
                    {
                        this.VBDesc = dictVB[nNode.Id];
                    }
                    this.Node = nNode;
                }

                bool assignIB = false;

                if (this.LevelOfDetail != lod)
                {
                    //Set buffer (IX)
                    this.LevelOfDetail = lod;

                    assignIB = true;
                }

                if (this.Shape != shape)
                {
                    //Set buffer (IX)
                    this.Shape = shape;

                    assignIB = true;
                }

                if (assignIB)
                {
                    this.IBDesc = dictIB[new MapGridShapeId() { LevelOfDetail = lod, Shape = shape }];
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
                if (this.LevelOfDetail == other.LevelOfDetail && this.Shape == other.Shape)
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
                    return this.Equals(shape);
                }

                return false;
            }
            /// <summary>
            /// Serves as the default hash function
            /// </summary>
            /// <returns>A hash code for the current object</returns>
            public override int GetHashCode()
            {
                return this.LevelOfDetail.GetHashCode() ^ this.Shape.GetHashCode();
            }
        }

        #endregion

        /// <summary>
        /// Grid
        /// </summary>
        private MapGrid Map = null;
        /// <summary>
        /// Height map
        /// </summary>
        private HeightMap heightMap = null;
        /// <summary>
        /// Heightmap cell size
        /// </summary>
        private readonly float heightMapCellSize;
        /// <summary>
        /// Heightmap maximum height
        /// </summary>
        private readonly float heightMapHeight;
        /// <summary>
        /// Heightmap height curve
        /// </summary>
        private readonly Curve heightMapCurve;
        /// <summary>
        /// UV map scale
        /// </summary>
        private readonly float uvScale;
        /// <summary>
        /// UV map displacement
        /// </summary>
        private readonly Vector2 uvDisplacement;

        /// <summary>
        /// Heightmap texture resolution
        /// </summary>
        private readonly float textureResolution;
        /// <summary>
        /// Terrain material
        /// </summary>
        private readonly MeshMaterial terrainMaterial;

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
        /// Terrain specular maps
        /// </summary>
        private EngineShaderResourceView terrainSpecularMaps = null;
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
        public virtual IEnumerable<MeshMaterial> Materials
        {
            get
            {
                return new[] { this.terrainMaterial };
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Terrain description</param>
        public Terrain(Scene scene, GroundDescription description)
            : base(scene, description)
        {
            this.useAnisotropic = description.UseAnisotropic;

            if (description.HeightmapDescription == null)
            {
                throw new EngineException($"Terrain initialization error. Heightmap description not found.");
            }

            // Read heightmap
            this.heightMap = HeightMap.FromDescription(description.HeightmapDescription);
            this.heightMapCellSize = description.HeightmapDescription.CellSize;
            this.heightMapHeight = description.HeightmapDescription.MaximumHeight;
            this.heightMapCurve = description.HeightmapDescription.HeightCurve;

            // Read material
            this.terrainMaterial = new MeshMaterial()
            {
                Material = description.HeightmapDescription.Material?.GetMaterial() ?? Material.Default
            };

            if (description.HeightmapDescription.Textures != null)
            {
                // Read texture data
                this.useAlphaMap = description.HeightmapDescription.Textures.UseAlphaMapping;
                this.useSlopes = description.HeightmapDescription.Textures.UseSlopes;
                this.proportion = description.HeightmapDescription.Textures.Proportion;
                this.uvScale = description.HeightmapDescription.Textures.Scale;
                this.uvDisplacement = description.HeightmapDescription.Textures.Displacement;
                this.textureResolution = description.HeightmapDescription.Textures.Resolution;
                this.slopeRanges = description.HeightmapDescription.Textures.SlopeRanges;

                this.ReadHeightmapTextures(description.HeightmapDescription.ContentPath, description.HeightmapDescription.Textures);
            }

            // Get vertices and indices from heightmap
            this.heightMap.BuildGeometry(
                this.heightMapCellSize,
                this.heightMapHeight,
                this.heightMapCurve,
                this.uvScale,
                this.uvDisplacement,
                out var vertices, out var indices);

            // Compute triangles for ray - mesh picking
            var tris = Triangle.ComputeTriangleList(
                Topology.TriangleList,
                vertices.Select(v => v.Position.Value).ToArray(),
                indices.ToArray());

            // Initialize quadtree for ray picking
            this.groundPickingQuadtree = description.ReadQuadTree(tris);

            if (this.Map == null)
            {
                //Initialize map
                int trianglesPerNode = this.heightMap.CalcTrianglesPerNode(MapGrid.LODLevels);

                this.Map = new MapGrid(this.Game, $"Terrain.{this.Name}", vertices, trianglesPerNode);
            }
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
                Map?.Dispose();
                Map = null;

                heightMap?.Dispose();
                heightMap = null;

                terrainTexturesLR?.Dispose();
                terrainTexturesLR = null;
                terrainTexturesHR?.Dispose();
                terrainTexturesHR = null;
                terrainNormalMaps?.Dispose();
                terrainNormalMaps = null;
                terrainSpecularMaps?.Dispose();
                terrainSpecularMaps = null;
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

            this.Map?.Update(context.EyePosition);
        }
        /// <inheritdoc/>
        public override void DrawShadows(DrawContextShadows context)
        {
            if (!Visible)
            {
                return;
            }

            var terrainTechnique = this.SetTechniqueTerrainShadowMap(context);
            if (terrainTechnique != null)
            {
                this.Map?.DrawShadows(context, this.BufferManager, terrainTechnique);
            }
        }
        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            var terrainTechnique = this.SetTechniqueTerrain(context);
            if (terrainTechnique != null)
            {
                this.Map?.Draw(context, this.BufferManager, terrainTechnique);
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
            if (mode.HasFlag(DrawerModes.Forward)) terrainTechnique = this.SetTechniqueTerrainDefault(context);
            if (mode.HasFlag(DrawerModes.Deferred)) terrainTechnique = this.SetTechniqueTerrainDeferred(context);

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
                this.textureResolution,
                context);

            var state = new EffectTerrainState
            {
                UseAnisotropic = this.useAnisotropic,
                NormalMap = this.terrainNormalMaps,
                SpecularMap = this.terrainSpecularMaps,
                UseAlphaMap = this.useAlphaMap,
                AlphaMap = this.alphaMap,
                ColorTextures = this.colorTextures,
                UseSlopes = this.useSlopes,
                SlopeRanges = this.slopeRanges,
                DiffuseMapLR = this.terrainTexturesLR,
                DiffuseMapHR = this.terrainTexturesHR,
                Proportion = this.proportion,
                MaterialIndex = this.terrainMaterial.ResourceIndex,
            };

            effect.UpdatePerObject(state);

            if (this.useAlphaMap && this.useSlopes) { return effect.TerrainFullForward; }
            if (this.useAlphaMap) { return effect.TerrainAlphaMapForward; }
            if (this.useSlopes) { return effect.TerrainSlopesForward; }

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
                this.textureResolution);

            var state = new EffectTerrainState
            {
                UseAnisotropic = this.useAnisotropic,
                NormalMap = this.terrainNormalMaps,
                SpecularMap = this.terrainSpecularMaps,
                UseAlphaMap = this.useAlphaMap,
                AlphaMap = this.alphaMap,
                ColorTextures = this.colorTextures,
                UseSlopes = this.useSlopes,
                SlopeRanges = this.slopeRanges,
                DiffuseMapLR = this.terrainTexturesLR,
                DiffuseMapHR = this.terrainTexturesHR,
                Proportion = this.proportion,
                MaterialIndex = this.terrainMaterial.ResourceIndex,
            };

            effect.UpdatePerObject(state);

            if (this.useAlphaMap && this.useSlopes) { return effect.TerrainFullDeferred; }
            if (this.useAlphaMap) { return effect.TerrainAlphaMapDeferred; }
            if (this.useSlopes) { return effect.TerrainSlopesDeferred; }

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
            this.terrainNormalMaps = this.Game.ResourceManager.RequestResource(normalMapTextures);

            var specularMapTextures = ImageContent.Array(tContentPath, description.SpecularMaps);
            this.terrainSpecularMaps = this.Game.ResourceManager.RequestResource(specularMapTextures);

            if (description.UseSlopes)
            {
                var texturesLR = ImageContent.Array(tContentPath, description.TexturesLR);
                var texturesHR = ImageContent.Array(tContentPath, description.TexturesHR);

                this.terrainTexturesLR = this.Game.ResourceManager.RequestResource(texturesLR);
                this.terrainTexturesHR = this.Game.ResourceManager.RequestResource(texturesHR);
            }

            if (description.UseAlphaMapping)
            {
                var colors = ImageContent.Array(tContentPath, description.ColorTextures);
                var aMap = ImageContent.Texture(tContentPath, description.AlphaMap);

                this.colorTextures = this.Game.ResourceManager.RequestResource(colors);
                this.alphaMap = this.Game.ResourceManager.RequestResource(aMap);
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
        /// <param name="description">Description</param>
        /// <param name="usage">Component usage</param>
        /// <param name="order">Processing order</param>
        /// <returns>Returns the created component</returns>
        public static async Task<Terrain> AddComponentTerrain(this Scene scene, GroundDescription description, SceneObjectUsages usage = SceneObjectUsages.None, int order = 0)
        {
            Terrain component = null;

            await Task.Run(() =>
            {
                component = new Terrain(scene, description);

                scene.AddComponent(component, usage, order);
            });

            return component;
        }
    }
}
