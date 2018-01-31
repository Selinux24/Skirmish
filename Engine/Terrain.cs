using SharpDX;
using SharpDX.Direct3D;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class Terrain : Ground, UseMaterials
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

                return bufferManager.Add(string.Format("{0}.{1}", shapeId.LevelOfDetail, shapeId.Shape), indexList, false);
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
            /// Visible nodes of last draw call
            /// </summary>
            private MapGridNode[] visibleNodesHigh = new MapGridNode[] { };
            /// <summary>
            /// Visible nodes of last draw call
            /// </summary>
            private MapGridNode[] visibleNodesMedium = new MapGridNode[] { };
            /// <summary>
            /// Visible nodes of last draw call
            /// </summary>
            private MapGridNode[] visibleNodesLow = new MapGridNode[] { };
            /// <summary>
            /// Visible nodes of last draw call
            /// </summary>
            private MapGridNode[] visibleNodesMinimum = new MapGridNode[] { };

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="game">Game instance</param>
            /// <param name="vertices">Vertices to map</param>
            /// <param name="trianglesPerNode">Triangles per terrain node</param>
            /// <param name="bufferManager">Buffer manager</param>
            public MapGrid(Game game, VertexData[] vertices, int trianglesPerNode, BufferManager bufferManager)
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
                    LevelOfDetailEnum.High,
                    LevelOfDetailEnum.Medium,
                    LevelOfDetailEnum.Low,
                    LevelOfDetailEnum.Minimum,
                };
                var shapeList = new[]
                {
                    IndexBufferShapeEnum.Full,

                    IndexBufferShapeEnum.SideTop,
                    IndexBufferShapeEnum.SideBottom,
                    IndexBufferShapeEnum.SideLeft,
                    IndexBufferShapeEnum.SideRight,

                    IndexBufferShapeEnum.CornerTopLeft,
                    IndexBufferShapeEnum.CornerTopRight,
                    IndexBufferShapeEnum.CornerBottomLeft,
                    IndexBufferShapeEnum.CornerBottomRight,
                };

                //Populate shapes dictionaty
                foreach (var lod in lodList)
                {
                    foreach (var shape in shapeList)
                    {
                        var id = new MapGridShapeId() { LevelOfDetail = lod, Shape = shape };

                        this.dictIB.Add(id, CreateDescriptor(id, trianglesPerNode, bufferManager));
                    }
                }

                this.drawingQuadTree = new QuadTree<VertexData>(vertices, LODLevels);

                //Populate nodes dictionary
                var nodes = this.drawingQuadTree.GetLeafNodes();
                foreach (var node in nodes)
                {
                    var data = VertexData.Convert(VertexTypes.Terrain, node.Items, null, null, Matrix.Identity);

                    this.dictVB.Add(node.Id, bufferManager.Add("", data, false, 1));
                }
            }
            /// <summary>
            /// Dispose
            /// </summary>
            public void Dispose()
            {
                Helper.Dispose(this.dictVB);
                Helper.Dispose(this.dictIB);

                this.NodesHigh = null;
                this.NodesMedium = null;
                this.NodesLow = null;
                this.NodesMinimum = null;
            }

            /// <summary>
            /// Updates map from quad-tree and position
            /// </summary>
            /// <param name="eyePosition">Eye position</param>
            public void Update(Vector3 eyePosition)
            {
                if (this.updatingNodes == false)
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

                    this.NodesHigh[0].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideTop, node, this.dictVB, this.dictIB);
                    this.NodesHigh[1].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideBottom, node, this.dictVB, this.dictIB);
                    this.NodesHigh[2].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideLeft, node, this.dictVB, this.dictIB);
                    this.NodesHigh[3].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideRight, node, this.dictVB, this.dictIB);
                    this.NodesHigh[4].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.CornerTopLeft, IndexBufferShapeEnum.CornerTopLeft, node, this.dictVB, this.dictIB);
                    this.NodesHigh[5].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.CornerTopRight, IndexBufferShapeEnum.CornerTopRight, node, this.dictVB, this.dictIB);
                    this.NodesHigh[6].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.CornerBottomLeft, IndexBufferShapeEnum.CornerBottomLeft, node, this.dictVB, this.dictIB);
                    this.NodesHigh[7].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.CornerBottomRight, IndexBufferShapeEnum.CornerBottomRight, node, this.dictVB, this.dictIB);
                    this.NodesHigh[8].Set(LevelOfDetailEnum.High, IndexBufferShapeEnum.Full, IndexBufferShapeEnum.Full, node, this.dictVB, this.dictIB);

                    this.NodesMedium[0].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideTop, this.NodesHigh[0].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[1].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideLeft, this.NodesMedium[0].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[2].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.CornerTopLeft, IndexBufferShapeEnum.SideLeft, this.NodesMedium[1].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[3].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideRight, this.NodesMedium[0].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[4].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.CornerTopRight, IndexBufferShapeEnum.SideRight, this.NodesMedium[3].Node, this.dictVB, this.dictIB);

                    this.NodesMedium[5].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideBottom, this.NodesHigh[1].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[6].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideLeft, this.NodesMedium[5].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[7].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.CornerBottomLeft, IndexBufferShapeEnum.SideLeft, this.NodesMedium[6].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[8].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideRight, this.NodesMedium[5].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[9].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.CornerBottomRight, IndexBufferShapeEnum.SideRight, this.NodesMedium[8].Node, this.dictVB, this.dictIB);

                    this.NodesMedium[10].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideLeft, this.NodesHigh[2].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[11].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideTop, this.NodesMedium[10].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[12].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideBottom, this.NodesMedium[10].Node, this.dictVB, this.dictIB);

                    this.NodesMedium[13].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideRight, this.NodesHigh[3].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[14].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideTop, this.NodesMedium[13].Node, this.dictVB, this.dictIB);
                    this.NodesMedium[15].Set(LevelOfDetailEnum.Medium, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideBottom, this.NodesMedium[13].Node, this.dictVB, this.dictIB);

                    this.NodesLow[0].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideTop, this.NodesMedium[0].Node, this.dictVB, this.dictIB);
                    this.NodesLow[1].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideLeft, this.NodesLow[0].Node, this.dictVB, this.dictIB);
                    this.NodesLow[2].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideLeft, this.NodesLow[1].Node, this.dictVB, this.dictIB);
                    this.NodesLow[3].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.CornerTopLeft, IndexBufferShapeEnum.SideLeft, this.NodesLow[2].Node, this.dictVB, this.dictIB);
                    this.NodesLow[4].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideRight, this.NodesLow[0].Node, this.dictVB, this.dictIB);
                    this.NodesLow[5].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideRight, this.NodesLow[4].Node, this.dictVB, this.dictIB);
                    this.NodesLow[6].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.CornerTopRight, IndexBufferShapeEnum.SideRight, this.NodesLow[5].Node, this.dictVB, this.dictIB);

                    this.NodesLow[7].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideBottom, this.NodesMedium[5].Node, this.dictVB, this.dictIB);
                    this.NodesLow[8].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideLeft, this.NodesLow[7].Node, this.dictVB, this.dictIB);
                    this.NodesLow[9].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideLeft, this.NodesLow[8].Node, this.dictVB, this.dictIB);
                    this.NodesLow[10].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.CornerBottomLeft, IndexBufferShapeEnum.SideLeft, this.NodesLow[9].Node, this.dictVB, this.dictIB);
                    this.NodesLow[11].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideRight, this.NodesLow[7].Node, this.dictVB, this.dictIB);
                    this.NodesLow[12].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideRight, this.NodesLow[11].Node, this.dictVB, this.dictIB);
                    this.NodesLow[13].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.CornerBottomRight, IndexBufferShapeEnum.SideRight, this.NodesLow[12].Node, this.dictVB, this.dictIB);

                    this.NodesLow[14].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideLeft, this.NodesMedium[10].Node, this.dictVB, this.dictIB);
                    this.NodesLow[15].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideTop, this.NodesLow[14].Node, this.dictVB, this.dictIB);
                    this.NodesLow[16].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideTop, this.NodesLow[15].Node, this.dictVB, this.dictIB);
                    this.NodesLow[17].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideBottom, this.NodesLow[14].Node, this.dictVB, this.dictIB);
                    this.NodesLow[18].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideBottom, this.NodesLow[17].Node, this.dictVB, this.dictIB);

                    this.NodesLow[19].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideRight, this.NodesMedium[13].Node, this.dictVB, this.dictIB);
                    this.NodesLow[20].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideTop, this.NodesLow[19].Node, this.dictVB, this.dictIB);
                    this.NodesLow[21].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideTop, this.NodesLow[20].Node, this.dictVB, this.dictIB);
                    this.NodesLow[22].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideBottom, this.NodesLow[19].Node, this.dictVB, this.dictIB);
                    this.NodesLow[23].Set(LevelOfDetailEnum.Low, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideBottom, this.NodesLow[22].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[0].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideTop, this.NodesLow[0].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[1].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[0].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[2].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[1].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[3].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[2].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[4].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.CornerTopLeft, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[3].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[5].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideRight, this.NodesMinimum[0].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[6].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideRight, this.NodesMinimum[5].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[7].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideTop, IndexBufferShapeEnum.SideRight, this.NodesMinimum[6].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[8].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.CornerTopRight, IndexBufferShapeEnum.SideRight, this.NodesMinimum[7].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[9].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideBottom, this.NodesLow[7].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[10].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[9].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[11].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[10].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[12].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[11].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[13].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.CornerBottomLeft, IndexBufferShapeEnum.SideLeft, this.NodesMinimum[12].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[14].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideRight, this.NodesMinimum[9].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[15].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideRight, this.NodesMinimum[14].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[16].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideBottom, IndexBufferShapeEnum.SideRight, this.NodesMinimum[15].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[17].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.CornerBottomRight, IndexBufferShapeEnum.SideRight, this.NodesMinimum[16].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[18].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideLeft, this.NodesLow[14].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[19].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideTop, this.NodesMinimum[18].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[20].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideTop, this.NodesMinimum[19].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[21].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideTop, this.NodesMinimum[20].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[22].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideBottom, this.NodesMinimum[18].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[23].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideBottom, this.NodesMinimum[22].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[24].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideLeft, IndexBufferShapeEnum.SideBottom, this.NodesMinimum[23].Node, this.dictVB, this.dictIB);

                    this.NodesMinimum[25].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideRight, this.NodesLow[19].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[26].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideTop, this.NodesMinimum[25].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[27].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideTop, this.NodesMinimum[26].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[28].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideTop, this.NodesMinimum[27].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[29].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideBottom, this.NodesMinimum[25].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[30].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideBottom, this.NodesMinimum[29].Node, this.dictVB, this.dictIB);
                    this.NodesMinimum[31].Set(LevelOfDetailEnum.Minimum, IndexBufferShapeEnum.SideRight, IndexBufferShapeEnum.SideBottom, this.NodesMinimum[30].Node, this.dictVB, this.dictIB);
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
                this.visibleNodesHigh = Array.FindAll(this.NodesHigh, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                this.visibleNodesMedium = Array.FindAll(this.NodesMedium, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                this.visibleNodesLow = Array.FindAll(this.NodesLow, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                this.visibleNodesMinimum = Array.FindAll(this.NodesMinimum, n => n.Node != null && context.Frustum.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

                this.DrawNodeList(DrawerModesEnum.ShadowMap, bufferManager, terrainTechnique, this.visibleNodesHigh);
                this.DrawNodeList(DrawerModesEnum.ShadowMap, bufferManager, terrainTechnique, this.visibleNodesMedium);
                this.DrawNodeList(DrawerModesEnum.ShadowMap, bufferManager, terrainTechnique, this.visibleNodesLow);
                this.DrawNodeList(DrawerModesEnum.ShadowMap, bufferManager, terrainTechnique, this.visibleNodesMinimum);
            }
            /// <summary>
            /// Draw
            /// </summary>
            /// <param name="context">Draw context</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="terrainTechnique">Technique for drawing</param>
            public void Draw(DrawContext context, BufferManager bufferManager, EngineEffectTechnique terrainTechnique)
            {
                this.visibleNodesHigh = Array.FindAll(this.NodesHigh, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                this.visibleNodesMedium = Array.FindAll(this.NodesMedium, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                this.visibleNodesLow = Array.FindAll(this.NodesLow, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);
                this.visibleNodesMinimum = Array.FindAll(this.NodesMinimum, n => n.Node != null && context.CameraVolume.Contains(n.Node.BoundingBox) != ContainmentType.Disjoint);

                var mode = context.DrawerMode;
                this.DrawNodeList(mode, bufferManager, terrainTechnique, this.visibleNodesHigh);
                this.DrawNodeList(mode, bufferManager, terrainTechnique, this.visibleNodesMedium);
                this.DrawNodeList(mode, bufferManager, terrainTechnique, this.visibleNodesLow);
                this.DrawNodeList(mode, bufferManager, terrainTechnique, this.visibleNodesMinimum);
            }
            /// <summary>
            /// Draws the visible node list
            /// </summary>
            /// <param name="mode">Drawer mode</param>
            /// <param name="bufferManager">Buffer manager</param>
            /// <param name="terrainTechnique">Technique for drawing</param>
            /// <param name="nodeList">Node list</param>
            private void DrawNodeList(DrawerModesEnum mode, BufferManager bufferManager, EngineEffectTechnique terrainTechnique, MapGridNode[] nodeList)
            {
                var graphics = this.Game.Graphics;

                for (int i = 0; i < nodeList.Length; i++)
                {
                    var gNode = nodeList[i];
                    if (gNode.IBDesc.Count > 0)
                    {
                        bufferManager.SetInputAssembler(terrainTechnique, gNode.VBDesc.Slot, PrimitiveTopology.TriangleList);
                        bufferManager.SetIndexBuffer(gNode.IBDesc.Slot);

                        if (!mode.HasFlag(DrawerModesEnum.ShadowMap))
                        {
                            Counters.InstancesPerFrame++;
                            Counters.PrimitivesPerFrame += gNode.IBDesc.Count / 3;
                        }

                        for (int p = 0; p < terrainTechnique.PassCount; p++)
                        {
                            graphics.EffectPassApply(terrainTechnique, p, 0);

                            graphics.DrawIndexed(
                                gNode.IBDesc.Count,
                                gNode.IBDesc.Offset,
                                gNode.VBDesc.Offset);
                        }
                    }
                }
            }
            /// <summary>
            /// Gets the visible nodes list gattered in last draw call
            /// </summary>
            /// <returns>Returns the visible nodes list gattered in last draw call</returns>
            public MapGridNode[] GetVisibleNodesHigh()
            {
                return Array.FindAll(this.visibleNodesHigh, n => n.Node != null);
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
            public LevelOfDetailEnum LevelOfDetail;
            /// <summary>
            /// Shape
            /// </summary>
            public IndexBufferShapeEnum Shape;
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
                LevelOfDetailEnum lod,
                IndexBufferShapeEnum shape,
                IndexBufferShapeEnum direction,
                QuadTreeNode<VertexData> node,
                Dictionary<int, BufferDescriptor> dictVB,
                Dictionary<MapGridShapeId, BufferDescriptor> dictIB)
            {
                QuadTreeNode<VertexData> nNode = null;

                if (node != null)
                {
                    if (direction == IndexBufferShapeEnum.Full) nNode = node;

                    else if (direction == IndexBufferShapeEnum.CornerTopLeft) nNode = node.TopLeftNeighbor;
                    else if (direction == IndexBufferShapeEnum.CornerTopRight) nNode = node.TopRightNeighbor;
                    else if (direction == IndexBufferShapeEnum.CornerBottomLeft) nNode = node.BottomLeftNeighbor;
                    else if (direction == IndexBufferShapeEnum.CornerBottomRight) nNode = node.BottomRightNeighbor;

                    else if (direction == IndexBufferShapeEnum.SideTop) nNode = node.TopNeighbor;
                    else if (direction == IndexBufferShapeEnum.SideBottom) nNode = node.BottomNeighbor;
                    else if (direction == IndexBufferShapeEnum.SideLeft) nNode = node.LeftNeighbor;
                    else if (direction == IndexBufferShapeEnum.SideRight) nNode = node.RightNeighbor;
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
        }
        /// <summary>
        /// Map grid shape Id
        /// </summary>
        struct MapGridShapeId : IEquatable<MapGridShapeId>
        {
            /// <summary>
            /// Level of detail
            /// </summary>
            public LevelOfDetailEnum LevelOfDetail;
            /// <summary>
            /// Shape
            /// </summary>
            public IndexBufferShapeEnum Shape;

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
                if (obj == null) return false;

                if (obj is MapGridShapeId)
                {
                    return this.Equals((MapGridShapeId)obj);
                }
                else
                {
                    return false;
                }
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
        private float heightMapCellSize;
        /// <summary>
        /// Heightmap maximum height
        /// </summary>
        private float heightMapHeight;

        /// <summary>
        /// Heightmap texture resolution
        /// </summary>
        private float textureResolution;
        /// <summary>
        /// Terrain material
        /// </summary>
        private MeshMaterial terrainMaterial;

        /// <summary>
        /// Gets or sets whether use alpha mapping or not
        /// </summary>
        private bool useAlphaMap;
        /// <summary>
        /// Gets or sets whether use slope texturing or not
        /// </summary>
        private bool useSlopes;
        /// <summary>
        /// Lerping proportion between alhpa mapping and slope texturing
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
        /// Slope ranges
        /// </summary>
        private Vector2 slopeRanges = Vector2.Zero;
        /// <summary>
        /// Use anisotropic
        /// </summary>
        private bool useAnisotropic = false;

        /// <summary>
        /// Gets the used material list
        /// </summary>
        public virtual MeshMaterial[] Materials
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
            #region Read heightmap

            {
                string contentPath = description.Content.HeightmapDescription.ContentPath;

                ImageContent heightMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Content.HeightmapDescription.HeightmapFileName),
                };
                ImageContent colorMapImage = new ImageContent()
                {
                    Streams = ContentManager.FindContent(contentPath, description.Content.HeightmapDescription.ColormapFileName),
                };

                this.heightMap = HeightMap.FromStream(heightMapImage.Stream, colorMapImage.Stream);
                this.heightMapCellSize = description.Content.HeightmapDescription.CellSize;
                this.heightMapHeight = description.Content.HeightmapDescription.MaximumHeight;
                this.textureResolution = description.Content.HeightmapDescription.TextureResolution;
            }

            #endregion

            #region Read terrain data

            {
                string contentPath = Path.Combine(description.Content.HeightmapDescription.ContentPath, description.Content.HeightmapDescription.Textures.ContentPath);

                this.terrainMaterial = new MeshMaterial()
                {
                    Material = description.Content.HeightmapDescription.Material != null ? description.Content.HeightmapDescription.Material.GetMaterial() : Material.Default
                };

                ImageContent normalMapTextures = new ImageContent()
                {
                    Paths = ContentManager.FindPaths(contentPath, description.Content.HeightmapDescription.Textures.NormalMaps),
                };
                this.terrainNormalMaps = this.Game.ResourceManager.CreateResource(normalMapTextures);

                ImageContent specularMapTextures = new ImageContent()
                {
                    Paths = ContentManager.FindPaths(contentPath, description.Content.HeightmapDescription.Textures.SpecularMaps),
                };
                this.terrainSpecularMaps = this.Game.ResourceManager.CreateResource(specularMapTextures);

                if (description.Content.HeightmapDescription.Textures.UseSlopes)
                {
                    ImageContent terrainTexturesLR = new ImageContent()
                    {
                        Paths = ContentManager.FindPaths(contentPath, description.Content.HeightmapDescription.Textures.TexturesLR),
                    };
                    ImageContent terrainTexturesHR = new ImageContent()
                    {
                        Paths = ContentManager.FindPaths(contentPath, description.Content.HeightmapDescription.Textures.TexturesHR),
                    };

                    this.terrainTexturesLR = this.Game.ResourceManager.CreateResource(terrainTexturesLR);
                    this.terrainTexturesHR = this.Game.ResourceManager.CreateResource(terrainTexturesHR);
                    this.slopeRanges = description.Content.HeightmapDescription.Textures.SlopeRanges;
                }

                if (description.Content.HeightmapDescription.Textures.UseAlphaMapping)
                {
                    ImageContent colors = new ImageContent()
                    {
                        Paths = ContentManager.FindPaths(contentPath, description.Content.HeightmapDescription.Textures.ColorTextures),
                    };
                    ImageContent alphaMap = new ImageContent()
                    {
                        Paths = ContentManager.FindPaths(contentPath, description.Content.HeightmapDescription.Textures.AlphaMap),
                    };

                    this.colorTextures = this.Game.ResourceManager.CreateResource(colors);
                    this.alphaMap = this.Game.ResourceManager.CreateResource(alphaMap);
                }

                this.useAlphaMap = description.Content.HeightmapDescription.Textures.UseAlphaMapping;
                this.useSlopes = description.Content.HeightmapDescription.Textures.UseSlopes;
                this.proportion = description.Content.HeightmapDescription.Textures.Proportion;
            }

            #endregion

            this.useAlphaMap = description.UseAnisotropic;
            this.useAnisotropic = description.UseAnisotropic;

            //Get vertices and indices from heightmap
            VertexData[] vertices;
            uint[] indices;
            this.BuildGeometry(out vertices, out indices);

            List<Triangle> tris = new List<Triangle>();

            for (int i = 0; i < indices.Length; i += 3)
            {
                tris.Add(new Triangle(
                    vertices[indices[i]].Position.Value,
                    vertices[indices[i + 2]].Position.Value,
                    vertices[indices[i + 1]].Position.Value));
            }

            //Initialize quadtree for ray picking
            this.groundPickingQuadtree = new PickingQuadTree<Triangle>(
                tris.ToArray(),
                description.Quadtree.MaximumDepth);

            if (this.Map == null)
            {
                //Initialize map
                int trianglesPerNode = this.heightMap.CalcTrianglesPerNode(MapGrid.LODLevels);

                this.Map = new MapGrid(this.Game, vertices, trianglesPerNode, this.BufferManager);
            }
        }
        /// <summary>
        /// Release of resources
        /// </summary>
        public override void Dispose()
        {
            Helper.Dispose(this.Map);

            Helper.Dispose(this.heightMap);

            Helper.Dispose(this.terrainTexturesLR);
            Helper.Dispose(this.terrainTexturesHR);
            Helper.Dispose(this.terrainNormalMaps);
            Helper.Dispose(this.terrainSpecularMaps);
            Helper.Dispose(this.colorTextures);
            Helper.Dispose(this.alphaMap);

            Helper.Dispose(this.terrainMaterial);
        }

        /// <summary>
        /// Updates the state of the terrain components
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {
            this.Map.Update(context.EyePosition);
        }
        /// <summary>
        /// Draws the terrain components shadows
        /// </summary>
        /// <param name="context">Draw context</param>
        public override void DrawShadows(DrawContextShadows context)
        {
            var terrainTechnique = this.SetTechniqueTerrainShadowMap(context);
            if (terrainTechnique != null)
            {
                this.Map.DrawShadows(context, this.BufferManager, terrainTechnique);
            }
        }
        /// <summary>
        /// Draws the terrain components
        /// </summary>
        /// <param name="context">Draw context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            EngineEffectTechnique terrainTechnique = null;
            if (mode.HasFlag(DrawerModesEnum.Forward)) terrainTechnique = this.SetTechniqueTerrainDefault(context as DrawContext);
            if (mode.HasFlag(DrawerModesEnum.Deferred)) terrainTechnique = this.SetTechniqueTerrainDeferred(context as DrawContext);
            if (terrainTechnique != null)
            {
                this.Map.Draw(context, this.BufferManager, terrainTechnique);
            }
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

            effect.UpdatePerObject(
                this.useAnisotropic,
                this.terrainNormalMaps,
                this.terrainSpecularMaps,
                this.useAlphaMap,
                this.alphaMap,
                this.colorTextures,
                this.useSlopes,
                this.slopeRanges,
                this.terrainTexturesLR,
                this.terrainTexturesHR,
                this.proportion,
                this.terrainMaterial.ResourceIndex);

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

            effect.UpdatePerObject(
                this.terrainMaterial.ResourceIndex,
                this.useAnisotropic,
                this.terrainNormalMaps,
                this.terrainSpecularMaps,
                this.useAlphaMap,
                this.alphaMap,
                this.colorTextures,
                this.useSlopes,
                this.slopeRanges,
                this.terrainTexturesLR,
                this.terrainTexturesHR,
                this.proportion);

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
        /// Build geometry
        /// </summary>
        /// <param name="vertices">Geometry vertices</param>
        /// <param name="indices">Geometry indices</param>
        private void BuildGeometry(out VertexData[] vertices, out uint[] indices)
        {
            this.heightMap.BuildGeometry(
                this.heightMapCellSize,
                this.heightMapHeight,
                out vertices, out indices);
        }

        /// <summary>
        /// Gets terrain bounding boxes at specified level
        /// </summary>
        /// <param name="level">Level</param>
        /// <returns>Returns terrain bounding boxes</returns>
        public BoundingBox[] GetBoundingBoxes(int level = 0)
        {
            return this.groundPickingQuadtree.GetBoundingBoxes(level);
        }
    }
}
