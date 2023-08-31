using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engine.Common
{
    /// <summary>
    /// Geometry utilities
    /// </summary>
    public static class GeometryUtil
    {
        /// <summary>
        /// Golden ratio value
        /// </summary>
        /// <seealso cref="https://en.wikipedia.org/wiki/Golden_ratio"/>
        public const float GoldenRatio = 1.6180f;
        /// <summary>
        /// Inverse golden ratio value
        /// </summary>
        public const float InverseGoldenRatio = 1f / GoldenRatio;
        /// <summary>
        /// Quad inverse golden ratio value
        /// </summary>
        public const float QuadInverseGoldenRatio = InverseGoldenRatio * InverseGoldenRatio;

        /// <summary>
        /// Generates a index for a triangle soup quad with the specified shape
        /// </summary>
        /// <param name="bufferShape">Buffer shape</param>
        /// <param name="triangles">Triangle count</param>
        /// <returns>Returns the generated index list</returns>
        public static IEnumerable<uint> GenerateIndices(IndexBufferShapes bufferShape, int triangles)
        {
            return GenerateIndices(LevelOfDetail.High, bufferShape, triangles);
        }
        /// <summary>
        /// Generates a index for a triangle soup quad with the specified shape
        /// </summary>
        /// <param name="lod">Level of detail</param>
        /// <param name="bufferShape">Buffer shape</param>
        /// <param name="triangles">Triangle count</param>
        /// <returns>Returns the generated index list</returns>
        public static IEnumerable<uint> GenerateIndices(LevelOfDetail lod, IndexBufferShapes bufferShape, int triangles)
        {
            uint offset = (uint)lod;
            uint fullSide = (uint)Math.Sqrt(triangles / 2f);

            int tris = triangles / (int)Math.Pow(offset, 2);

            int nodes = tris / 2;
            uint side = (uint)Math.Sqrt(nodes);
            uint sideLoss = side / 2;

            bool topSide =
                bufferShape == IndexBufferShapes.CornerTopLeft ||
                bufferShape == IndexBufferShapes.CornerTopRight ||
                bufferShape == IndexBufferShapes.SideTop;

            bool bottomSide =
                bufferShape == IndexBufferShapes.CornerBottomLeft ||
                bufferShape == IndexBufferShapes.CornerBottomRight ||
                bufferShape == IndexBufferShapes.SideBottom;

            bool leftSide =
                bufferShape == IndexBufferShapes.CornerBottomLeft ||
                bufferShape == IndexBufferShapes.CornerTopLeft ||
                bufferShape == IndexBufferShapes.SideLeft;

            bool rightSide =
                bufferShape == IndexBufferShapes.CornerBottomRight ||
                bufferShape == IndexBufferShapes.CornerTopRight ||
                bufferShape == IndexBufferShapes.SideRight;

            uint totalTriangles = (uint)tris;
            if (topSide) totalTriangles -= sideLoss;
            if (bottomSide) totalTriangles -= sideLoss;
            if (leftSide) totalTriangles -= sideLoss;
            if (rightSide) totalTriangles -= sideLoss;

            var indices = new List<uint>((int)totalTriangles * 3);

            for (uint y = 1; y < side; y += 2)
            {
                for (uint x = 1; x < side; x += 2)
                {
                    uint indexPRow = (((y - 1) * offset) * (fullSide + 1)) + (x * offset);
                    uint indexCRow = (((y + 0) * offset) * (fullSide + 1)) + (x * offset);
                    uint indexNRow = (((y + 1) * offset) * (fullSide + 1)) + (x * offset);

                    bool firstRow = y == 1;
                    bool lastRow = y == side - 1;
                    bool firstColumn = x == 1;
                    bool lastColumn = x == side - 1;

                    //Top side
                    var top = ComputeTopSide(firstRow, topSide, offset, indexPRow, indexCRow);

                    //Bottom side
                    var bottom = ComputeBottomSide(lastRow, bottomSide, offset, indexCRow, indexNRow);

                    //Left side
                    var left = ComputeLeftSide(firstColumn, leftSide, offset, indexPRow, indexCRow, indexNRow);

                    //Right side
                    var right = ComputeRightSide(lastColumn, rightSide, offset, indexPRow, indexCRow, indexNRow);

                    indices.AddRange(top);
                    indices.AddRange(bottom);
                    indices.AddRange(left);
                    indices.AddRange(right);
                }
            }

            return indices.ToArray();
        }
        /// <summary>
        /// Computes the top side indexes for triangle soup quad
        /// </summary>
        /// <param name="firstRow">It's the first row</param>
        /// <param name="topSide">It's the top side</param>
        /// <param name="offset">Index offset</param>
        /// <param name="indexPRow">P index</param>
        /// <param name="indexCRow">C index</param>
        /// <returns>Returns the indexes list</returns>
        private static IEnumerable<uint> ComputeTopSide(bool firstRow, bool topSide, uint offset, uint indexPRow, uint indexCRow)
        {
            if (firstRow && topSide)
            {
                return new[]
                {
                    //Top
                    indexCRow,
                    indexPRow - (1 * offset),
                    indexPRow + (1 * offset),
                };
            }
            else
            {
                return new[]
                {
                    //Top left
                    indexCRow,
                    indexPRow - (1 * offset),
                    indexPRow,
                    //Top right
                    indexCRow,
                    indexPRow,
                    indexPRow + (1 * offset),
                };
            }
        }
        /// <summary>
        /// Computes the bottom side indexes for triangle soup quad
        /// </summary>
        /// <param name="lastRow">It's the last row</param>
        /// <param name="bottomSide">It's the bottom side</param>
        /// <param name="offset">Index offset</param>
        /// <param name="indexCRow">C index</param>
        /// <param name="indexNRow">N index</param>
        /// <returns>Returns the indexes list</returns>
        private static IEnumerable<uint> ComputeBottomSide(bool lastRow, bool bottomSide, uint offset, uint indexCRow, uint indexNRow)
        {
            if (lastRow && bottomSide)
            {
                return new[]
                {
                    //Bottom only
                    indexCRow,
                    indexNRow + (1 * offset),
                    indexNRow - (1 * offset),
                };
            }
            else
            {
                return new[]
                {
                    //Bottom left
                    indexCRow,
                    indexNRow,
                    indexNRow - (1 * offset),
                    //Bottom right
                    indexCRow,
                    indexNRow + (1 * offset),
                    indexNRow,
                };
            }
        }
        /// <summary>
        /// Computes the left side indexes for triangle soup quad
        /// </summary>
        /// <param name="firstColumn">It's the first column</param>
        /// <param name="leftSide">It's the left side</param>
        /// <param name="offset">Index offset</param>
        /// <param name="indexPRow">P index</param>
        /// <param name="indexCRow">C index</param>
        /// <param name="indexNRow">N index</param>
        /// <returns>Returns the indexes list</returns>
        private static IEnumerable<uint> ComputeLeftSide(bool firstColumn, bool leftSide, uint offset, uint indexPRow, uint indexCRow, uint indexNRow)
        {
            if (firstColumn && leftSide)
            {
                return new[]
                {
                    //Left only
                    indexCRow,
                    indexNRow - (1 * offset),
                    indexPRow - (1 * offset),
                };
            }
            else
            {
                return new[]
                {
                    //Left top
                    indexCRow,
                    indexCRow - (1 * offset),
                    indexPRow - (1 * offset),
                    //Left bottom
                    indexCRow,
                    indexNRow - (1 * offset),
                    indexCRow - (1 * offset),
                };
            }
        }
        /// <summary>
        /// Computes the right side indexes for triangle soup quad
        /// </summary>
        /// <param name="lastColumn">It's the last column</param>
        /// <param name="rightSide">It's the right side</param>
        /// <param name="offset">Index offset</param>
        /// <param name="indexPRow">P index</param>
        /// <param name="indexCRow">C index</param>
        /// <param name="indexNRow">N index</param>
        /// <returns>Returns the indexes list</returns>
        private static IEnumerable<uint> ComputeRightSide(bool lastColumn, bool rightSide, uint offset, uint indexPRow, uint indexCRow, uint indexNRow)
        {
            if (lastColumn && rightSide)
            {
                return new[]
                {
                    //Right only
                    indexCRow,
                    indexPRow + (1 * offset),
                    indexNRow + (1 * offset),
                };
            }
            else
            {
                return new[]
                {
                    //Right top
                    indexCRow,
                    indexPRow + (1 * offset),
                    indexCRow + (1 * offset),
                    //Right bottom
                    indexCRow,
                    indexCRow + (1 * offset),
                    indexNRow + (1 * offset),
                };
            }
        }
        /// <summary>
        /// Toggle coordinates from left-handed to right-handed and vice versa
        /// </summary>
        /// <typeparam name="T">Index type</typeparam>
        /// <param name="indices">Indices in a triangle list topology</param>
        /// <returns>Returns a new array</returns>
        public static IEnumerable<T> ChangeCoordinate<T>(IEnumerable<T> indices)
        {
            var idx = indices.ToArray();

            T[] res = new T[idx.Length];

            for (int i = 0; i < idx.Length; i += 3)
            {
                res[i + 0] = idx[i + 0];
                res[i + 1] = idx[i + 2];
                res[i + 2] = idx[i + 1];
            }

            return res;
        }
        /// <summary>
        /// Gets the asset transform
        /// </summary>
        /// <param name="position">Position</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="scale">Scale</param>
        /// <returns>Returns a matrix with the reference transform</returns>
        public static Matrix Transformation(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return Matrix.Transformation(
                Vector3.Zero, Quaternion.Identity, scale,
                Vector3.Zero, rotation,
                position);
        }

        /// <summary>
        /// Creates a new UV map from parameters
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="texU">Texture U</param>
        /// <param name="texV">Texture V</param>
        /// <param name="texWidth">Texture total width</param>
        /// <param name="texHeight">Texture total height</param>
        /// <returns>Returns the new UP map for the sprite</returns>
        public static Vector4 CreateUVMap(float width, float height, float texU, float texV, float texWidth, float texHeight)
        {
            //Texture map
            float u0 = texWidth > 0 ? (texU) / texWidth : 0;
            float v0 = texHeight > 0 ? (texV) / texHeight : 0;
            float u1 = texWidth > 0 ? (texU + width) / texWidth : 1;
            float v1 = texHeight > 0 ? (texV + height) / texHeight : 1;

            return new Vector4(u0, v0, u1, v1);
        }

        /// <summary>
        /// Creates a screen
        /// </summary>
        /// <param name="form">Form</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateScreen(IEngineForm form)
        {
            return CreateScreen(form.RenderWidth, form.RenderHeight);
        }
        /// <summary>
        /// Creates a screen
        /// </summary>
        /// <param name="renderWidth">Render area width</param>
        /// <param name="renderHeight">Render area height</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateScreen(int renderWidth, int renderHeight)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uvs = new Vector2[4];
            uint[] indices = new uint[6];

            float width = renderWidth;
            float height = renderHeight;

            float left = ((width / 2) * -1);
            float right = left + width;
            float top = (height / 2);
            float bottom = top - height;

            vertices[0] = new Vector3(left, top, 0.0f);
            uvs[0] = new Vector2(0.0f, 0.0f);

            vertices[1] = new Vector3(right, bottom, 0.0f);
            uvs[1] = new Vector2(1.0f, 1.0f);

            vertices[2] = new Vector3(left, bottom, 0.0f);
            uvs[2] = new Vector2(0.0f, 1.0f);

            vertices[3] = new Vector3(right, top, 0.0f);
            uvs[3] = new Vector2(1.0f, 0.0f);

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 1;

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
                Uvs = uvs,
            };
        }
        /// <summary>
        /// Creates a unit sprite
        /// </summary>
        /// <returns>Returns a geometry descriptor</returns>
        /// <remarks>Unit size with then center in X=0.5;Y=0.5</remarks>
        public static GeometryDescriptor CreateUnitSprite()
        {
            return CreateSprite(new Vector2(-0.5f, 0.5f), 1, 1, 0, 0);
        }
        /// <summary>
        /// Creates a unit sprite
        /// </summary>
        /// <param name="uvMap">UV map</param>
        /// <returns>Returns a geometry descriptor</returns>
        /// <remarks>Unit size with then center in X=0.5;Y=0.5</remarks>
        public static GeometryDescriptor CreateUnitSprite(Vector4 uvMap)
        {
            return CreateSprite(new Vector2(-0.5f, 0.5f), 1, 1, 0, 0, uvMap);
        }
        /// <summary>
        /// Creates a sprite
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSprite(Vector2 position, float width, float height)
        {
            return CreateSprite(position, width, height, 0, 0, new Vector4(0, 0, 1, 1));
        }
        /// <summary>
        /// Creates a sprite
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight)
        {
            return CreateSprite(position, width, height, formWidth, formHeight, new Vector4(0, 0, 1, 1));
        }
        /// <summary>
        /// Creates a sprite
        /// </summary>
        /// <param name="position">Sprite position</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="formWidth">Render form width</param>
        /// <param name="formHeight">Render form height</param>
        /// <param name="uvMap">UV map</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSprite(Vector2 position, float width, float height, float formWidth, float formHeight, Vector4 uvMap)
        {
            Vector3[] vertices = new Vector3[4];
            Vector2[] uvs = new Vector2[4];

            float left = (formWidth * 0.5f * -1f) + position.X;
            float right = left + width;
            float top = (formHeight * 0.5f) + position.Y;
            float bottom = top - height;

            //Texture map
            float u0 = uvMap.X;
            float v0 = uvMap.Y;
            float u1 = uvMap.Z;
            float v1 = uvMap.W;

            vertices[0] = new Vector3(left, top, 0.0f);
            uvs[0] = new Vector2(u0, v0);

            vertices[1] = new Vector3(right, bottom, 0.0f);
            uvs[1] = new Vector2(u1, v1);

            vertices[2] = new Vector3(left, bottom, 0.0f);
            uvs[2] = new Vector2(u0, v1);

            vertices[3] = new Vector3(right, top, 0.0f);
            uvs[3] = new Vector2(u1, v0);

            uint[] indices = new uint[6];

            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;

            indices[3] = 0;
            indices[4] = 3;
            indices[5] = 1;

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
                Uvs = uvs,
            };
        }

        /// <summary>
        /// Creates a sphere
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="sphere">Sphere</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSphere(Topology topology, BoundingSphere sphere, int sliceCount, int stackCount)
        {
            return CreateSphere(topology, sphere.Center, sphere.Radius, sliceCount, stackCount);
        }
        /// <summary>
        /// Creates a sphere
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSphere(Topology topology, float radius, int sliceCount, int stackCount)
        {
            return CreateSphere(topology, Vector3.Zero, radius, sliceCount, stackCount);
        }
        /// <summary>
        /// Creates a sphere
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Sphere center</param>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSphere(Topology topology, Vector3 center, float radius, int sliceCount, int stackCount)
        {
            if (topology == Topology.TriangleList)
            {
                var vertList = new List<Vector3>();
                var normList = new List<Vector3>();
                var tangList = new List<Vector3>();
                var binmList = new List<Vector3>();
                var uvList = new List<Vector2>();

                sliceCount--;
                stackCount++;

                #region Positions

                //North pole
                vertList.Add(new Vector3(0.0f, radius, 0.0f) + center);
                normList.Add(new Vector3(0.0f, 1.0f, 0.0f));
                tangList.Add(new Vector3(0.0f, 0.0f, 1.0f));
                binmList.Add(new Vector3(1.0f, 0.0f, 0.0f));
                uvList.Add(new Vector2(0.0f, 0.0f));

                float phiStep = MathUtil.Pi / stackCount;
                float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

                for (int st = 1; st <= stackCount - 1; ++st)
                {
                    float phi = st * phiStep;

                    for (int sl = 0; sl <= sliceCount; ++sl)
                    {
                        float theta = sl * thetaStep;

                        float x = (float)Math.Sin(phi) * (float)Math.Cos(theta);
                        float y = (float)Math.Cos(phi);
                        float z = (float)Math.Sin(phi) * (float)Math.Sin(theta);

                        float tX = -(float)Math.Sin(phi) * (float)Math.Sin(theta);
                        float tY = 0.0f;
                        float tZ = +(float)Math.Sin(phi) * (float)Math.Cos(theta);

                        var position = radius * new Vector3(x, y, z);
                        var normal = new Vector3(x, y, z);
                        var tangent = Vector3.Normalize(new Vector3(tX, tY, tZ));
                        var binormal = Vector3.Cross(normal, tangent);

                        float u = theta / MathUtil.Pi * 2f;
                        float v = phi / MathUtil.Pi;

                        var texture = new Vector2(u, v);

                        vertList.Add(position + center);
                        normList.Add(normal);
                        tangList.Add(tangent);
                        binmList.Add(binormal);
                        uvList.Add(texture);
                    }
                }

                //South pole
                vertList.Add(new Vector3(0.0f, -radius, 0.0f) + center);
                normList.Add(new Vector3(0.0f, -1.0f, 0.0f));
                tangList.Add(new Vector3(0.0f, 0.0f, -1.0f));
                binmList.Add(new Vector3(-1.0f, 0.0f, 0.0f));
                uvList.Add(new Vector2(0.0f, 1.0f));

                #endregion

                var indexList = new List<int>();

                #region Indexes

                for (int index = 1; index <= sliceCount; ++index)
                {
                    indexList.Add(0);
                    indexList.Add(index + 1);
                    indexList.Add(index);
                }

                int baseIndex = 1;
                int ringVertexCount = sliceCount + 1;
                for (int st = 0; st < stackCount - 2; ++st)
                {
                    for (int sl = 0; sl < sliceCount; ++sl)
                    {
                        indexList.Add(baseIndex + st * ringVertexCount + sl);
                        indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                        indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);

                        indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl);
                        indexList.Add(baseIndex + st * ringVertexCount + sl + 1);
                        indexList.Add(baseIndex + (st + 1) * ringVertexCount + sl + 1);
                    }
                }

                int southPoleIndex = vertList.Count - 1;

                baseIndex = southPoleIndex - ringVertexCount;

                for (int index = 0; index < sliceCount; ++index)
                {
                    indexList.Add(southPoleIndex);
                    indexList.Add(baseIndex + index);
                    indexList.Add(baseIndex + index + 1);
                }

                #endregion

                return new GeometryDescriptor()
                {
                    Vertices = vertList.ToArray(),
                    Normals = normList.ToArray(),
                    Tangents = tangList.ToArray(),
                    Binormals = binmList.ToArray(),
                    Uvs = uvList.ToArray(),
                    Indices = indexList.Select(i => (uint)i).ToArray(),
                };
            }
            else if (topology == Topology.LineList)
            {
                var vertList = new List<Vector3>();
                var indexList = new List<int>();

                //North pole
                vertList.Add(new Vector3(0.0f, +radius, 0.0f) + center);

                float phiStep = MathUtil.Pi / (stackCount + 1);
                float thetaStep = 2.0f * MathUtil.Pi / sliceCount;

                //Compute vertices for each stack ring (do not count the poles as rings).
                for (int st = 1; st < (stackCount + 1); ++st)
                {
                    float phi = st * phiStep;

                    //Vertices of ring.
                    for (int sl = 0; sl <= sliceCount; ++sl)
                    {
                        float theta = sl * thetaStep;

                        //Spherical to Cartesian
                        var position = new Vector3(
                            radius * (float)Math.Sin(phi) * (float)Math.Cos(theta),
                            radius * (float)Math.Cos(phi),
                            radius * (float)Math.Sin(phi) * (float)Math.Sin(theta));

                        indexList.Add(vertList.Count);
                        indexList.Add(sl == sliceCount ? vertList.Count - sliceCount : vertList.Count + 1);

                        vertList.Add(position + center);
                    }
                }

                //South pole
                vertList.Add(new Vector3(0.0f, -radius, 0.0f) + center);

                return new GeometryDescriptor()
                {
                    Vertices = vertList.ToArray(),
                    Indices = indexList.Select(i => (uint)i).ToArray(),
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Creates a sphere list
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="sphereList">Sphere list</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateSpheres(Topology topology, IEnumerable<BoundingSphere> sphereList, int sliceCount, int stackCount)
        {
            var gList = sphereList.Select(s => CreateSphere(topology, s, sliceCount, stackCount));

            return new GeometryDescriptor(gList);
        }

        /// <summary>
        /// Creates a box
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="bbox">Bounding box</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateBox(Topology topology, BoundingBox bbox)
        {
            return CreateBox(topology, bbox.Center, bbox.Width, bbox.Height, bbox.Depth);
        }
        /// <summary>
        /// Creates a box
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="obb">Oriented bounding box</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateBox(Topology topology, OrientedBoundingBox obb)
        {
            var geom = CreateBox(topology, Vector3.Zero, obb.Extents.X * 2, obb.Extents.Y * 2, obb.Extents.Z * 2);

            var trn = obb.Transformation;
            if (!trn.IsIdentity)
            {
                var vertices = geom.Vertices.ToArray();
                Vector3.TransformCoordinate(vertices, ref trn, vertices);
                geom.Vertices = vertices;
            }

            return geom;
        }
        /// <summary>
        /// Creates a box
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateBox(Topology topology, float width, float height, float depth)
        {
            return CreateBox(topology, Vector3.Zero, width, height, depth);
        }
        /// <summary>
        /// Creates a box
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Box center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateBox(Topology topology, Vector3 center, float width, float height, float depth)
        {
            float w2 = 0.5f * width;
            float h2 = 0.5f * height;
            float d2 = 0.5f * depth;

            //Create separate faces to make sharp edges
            var vertices = new[]
            {
                // Fill in the front face vertex data.
                new Vector3(-w2, -h2, -d2),
                new Vector3(-w2, +h2, -d2),
                new Vector3(+w2, +h2, -d2),
                new Vector3(+w2, -h2, -d2),

                // Fill in the back face vertex data.
                new Vector3(-w2, -h2, +d2),
                new Vector3(+w2, -h2, +d2),
                new Vector3(+w2, +h2, +d2),
                new Vector3(-w2, +h2, +d2),

                // Fill in the top face vertex data.
                new Vector3(-w2, +h2, -d2),
                new Vector3(-w2, +h2, +d2),
                new Vector3(+w2, +h2, +d2),
                new Vector3(+w2, +h2, -d2),

                // Fill in the bottom face vertex data.
                new Vector3(-w2, -h2, -d2),
                new Vector3(+w2, -h2, -d2),
                new Vector3(+w2, -h2, +d2),
                new Vector3(-w2, -h2, +d2),

                // Fill in the left face vertex data.
                new Vector3(-w2, -h2, +d2),
                new Vector3(-w2, +h2, +d2),
                new Vector3(-w2, +h2, -d2),
                new Vector3(-w2, -h2, -d2),

                // Fill in the right face vertex data.
                new Vector3(+w2, -h2, -d2),
                new Vector3(+w2, +h2, -d2),
                new Vector3(+w2, +h2, +d2),
                new Vector3(+w2, -h2, +d2),
            };

            if (center != Vector3.Zero)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] += center;
                }
            }

            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[36];

                // Fill in the front face index data
                indices[0] = 0; indices[1] = 1; indices[2] = 2;
                indices[3] = 0; indices[4] = 2; indices[5] = 3;

                // Fill in the back face index data
                indices[6] = 4; indices[7] = 5; indices[8] = 6;
                indices[9] = 4; indices[10] = 6; indices[11] = 7;

                // Fill in the top face index data
                indices[12] = 8; indices[13] = 9; indices[14] = 10;
                indices[15] = 8; indices[16] = 10; indices[17] = 11;

                // Fill in the bottom face index data
                indices[18] = 12; indices[19] = 13; indices[20] = 14;
                indices[21] = 12; indices[22] = 14; indices[23] = 15;

                // Fill in the left face index data
                indices[24] = 16; indices[25] = 17; indices[26] = 18;
                indices[27] = 16; indices[28] = 18; indices[29] = 19;

                // Fill in the right face index data
                indices[30] = 20; indices[31] = 21; indices[32] = 22;
                indices[33] = 20; indices[34] = 22; indices[35] = 23;
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    0, 1,
                    0, 3,
                    1, 2,
                    3, 2,

                    4, 5,
                    4, 7,
                    5, 6,
                    7, 6,

                    0, 4,
                    1, 7,
                    2, 6,
                    3, 5,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a box list
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="bboxList">Bounding box list</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateBoxes(Topology topology, IEnumerable<BoundingBox> bboxList)
        {
            var gList = bboxList.Select(b => CreateBox(topology, b));

            return new GeometryDescriptor(gList);
        }
        /// <summary>
        /// Creates a box list
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="obbList">Oriented bounding box list</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateBoxes(Topology topology, IEnumerable<OrientedBoundingBox> obbList)
        {
            var gList = obbList.Select(b => CreateBox(topology, b));

            return new GeometryDescriptor(gList);
        }

        /// <summary>
        /// Creates a cone
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="baseRadius">The base radius</param>
        /// <param name="height">Cone height</param>
        /// <param name="sliceCount">The base slice count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateConeBaseRadius(Topology topology, float baseRadius, float height, int sliceCount)
        {
            var vertList = new List<Vector3>();
            var indexList = new List<uint>();

            vertList.Add(new Vector3(0.0f, 0.0f, 0.0f));
            vertList.Add(new Vector3(0.0f, -height, 0.0f));

            float thetaStep = MathUtil.TwoPi / sliceCount;

            for (int sl = 0; sl < sliceCount; sl++)
            {
                float theta = sl * thetaStep;

                var position = new Vector3(
                    baseRadius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Cos(theta),
                    -height,
                    baseRadius * (float)Math.Sin(MathUtil.PiOverTwo) * (float)Math.Sin(theta));

                vertList.Add(position);
            }

            if (topology == Topology.TriangleList)
            {
                for (uint index = 0; index < sliceCount; index++)
                {
                    indexList.Add(0);
                    indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
                    indexList.Add(index + 2);

                    indexList.Add(1);
                    indexList.Add(index + 2);
                    indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
                }
            }
            else if (topology == Topology.LineList)
            {
                for (uint index = 0; index < sliceCount; index++)
                {
                    indexList.Add(0);
                    indexList.Add(index + 2);

                    indexList.Add(1);
                    indexList.Add(index + 2);

                    indexList.Add(index + 2);
                    indexList.Add(index == sliceCount - 1 ? 2 : index + 3);
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return new GeometryDescriptor()
            {
                Vertices = vertList.ToArray(),
                Indices = indexList.ToArray(),
            };
        }
        /// <summary>
        /// Creates a cone
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="cupAngle">The cup angle</param>
        /// <param name="height">Cone height</param>
        /// <param name="sliceCount">The base slice count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateConeCupAngle(Topology topology, float cupAngle, float height, int sliceCount)
        {
            float baseRadius = (float)Math.Tan(cupAngle) * height;

            return CreateConeBaseRadius(topology, baseRadius, height, sliceCount);
        }

        /// <summary>
        /// Creates a frustum
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="frustum">Bounding frustum</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateFrustum(Topology topology, BoundingFrustum frustum)
        {
            //Get the 8 corners of the frustum
            var vertices = frustum.GetCorners();

            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[]
                {
                    0,1,2,
                    0,2,3,

                    4,6,5,
                    4,7,6,

                    3,6,2,
                    3,7,6,

                    0,1,5,
                    0,5,4,

                    2,1,5,
                    2,5,6,

                    0,3,7,
                    0,7,4,
                };
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    0, 1,
                    0, 3,
                    1, 2,
                    3, 2,

                    4, 5,
                    4, 7,
                    5, 6,
                    7, 6,

                    0, 4,
                    1, 5,
                    2, 6,
                    3, 7
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a tetrahedron
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateTetrahedron(Topology topology, Vector3 center, float width, float height, float depth)
        {
            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    1, 3, 2,
                    0, 3, 1,
                };
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    0, 1,
                    1, 2,
                    2, 0,
                    0, 3,
                    1, 3,
                    2, 3,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            var vertices = new[]
            {
                new Vector3(1f, 1f, 1f),
                new Vector3(1f, -1f, -1f),
                new Vector3(-1f, 1f, -1f),
                new Vector3(-1f, -1f, 1),
            };

            Matrix trn = Matrix.Scaling(width, height, depth) * Matrix.Translation(center);
            Vector3.TransformCoordinate(vertices, ref trn, vertices);

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a octahedron
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateOctahedron(Topology topology, Vector3 center, float width, float height, float depth)
        {
            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 1,

                    5, 2, 1,
                    5, 3, 2,
                    5, 4, 3,
                    5, 1, 4,
                };
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    0, 1,
                    0, 2,
                    0, 3,
                    0, 4,

                    5, 1,
                    5, 2,
                    5, 3,
                    5, 4,

                    1, 2,
                    2, 3,
                    3, 4,
                    4, 1,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            var vertices = new[]
            {
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, -1f),
                new Vector3(-1f, 0f, 0f),
                new Vector3(0f, 0f, 1f),
                new Vector3(0f, -1f, 0f),
            };

            Matrix trn = Matrix.Scaling(width, height, depth) * Matrix.Translation(center);
            Vector3.TransformCoordinate(vertices, ref trn, vertices);

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a icosahedron
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateIcosahedron(Topology topology, Vector3 center, float width, float height, float depth)
        {
            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[]
                {
                    0,1,2,
                    0,3,1,
                    0,2,4,
                    0,5,3,
                    0,4,5,

                    1,6,2,
                    1,3,7,
                    1,7,6,

                    2,8,4,
                    2,6,8,

                    3,9,7,
                    3,5,9,

                    11,8,6,
                    11,7,9,
                    11,6,7,

                    10,4,8,
                    10,9,5,
                    10,5,4,
                    10,11,9,
                    10,8,11,
                };
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    0, 1,
                    0, 2,
                    0, 3,
                    1, 2,
                    1, 3,

                    0, 4,
                    0, 5,
                    4, 5,
                    4, 2,
                    5, 3,

                    1, 6,
                    1, 7,
                    6, 7,
                    6, 2,
                    7, 3,

                    2, 8,
                    3, 9,

                    11, 6,
                    11, 7,
                    6, 7,
                    6, 8,
                    7, 9,

                    10, 4,
                    10, 5,
                    4, 5,
                    4, 8,
                    5, 9,

                    10, 11,
                    10, 9,
                    10, 8,
                    11, 9,
                    11, 8,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            var vertices = new[]
            {
                new Vector3(-InverseGoldenRatio, +1f, 0f),
                new Vector3(+InverseGoldenRatio, +1f, 0f),

                new Vector3(0f, +InverseGoldenRatio, -1f),
                new Vector3(0f, +InverseGoldenRatio, +1f),

                new Vector3(-1f, 0f, -InverseGoldenRatio),
                new Vector3(-1f, 0f, +InverseGoldenRatio),
                new Vector3(+1f, 0f, -InverseGoldenRatio),
                new Vector3(+1f, 0f, +InverseGoldenRatio),

                new Vector3(0f, -InverseGoldenRatio, -1f),
                new Vector3(0f, -InverseGoldenRatio, +1f),

                new Vector3(-InverseGoldenRatio, -1f, 0f),
                new Vector3(+InverseGoldenRatio, -1f, 0f),
            };

            Matrix trn = Matrix.Scaling(width, height, depth) * Matrix.Translation(center);
            Vector3.TransformCoordinate(vertices, ref trn, vertices);

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a dodecahedron
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateDodecahedron(Topology topology, Vector3 center, float width, float height, float depth)
        {
            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[]
                {
                    //0,1,4,6,2,
                    0,6,2,
                    0,4,6,
                    0,1,4,
                    
                    //0,2,8,9,3,
                    0,9,3,
                    0,8,9,
                    0,2,8,

                    //1,0,3,7,5,
                    1,7,5,
                    1,3,7,
                    1,0,3,
                    
                    //1,5,11,10,4,
                    1,10,4,
                    1,11,10,
                    1,5,11,

                    //6,4,10,16,12,
                    6,16,12,
                    6,10,16,
                    6,4,10,

                    //6,12,14,8,2,
                    6,8,2,
                    6,14,8,
                    6,12,14,

                    //7,13,17,11,5,
                    7,11,5,
                    7,17,11,
                    7,13,17,

                    //7,3,9,15,13,
                    7,15,13,
                    7,9,15,
                    7,3,9,

                    //18,19,16,12,14,
                    18,14,12,
                    18,12,16,
                    18,16,19,

                    //18,14,8,9,15,
                    18,15,9,
                    18,9,8,
                    18,8,14,

                    //19,18,15,13,17,
                    19,17,13,
                    19,13,15,
                    19,15,18,

                    //19,17,11,10,16,
                    19,16,10,
                    19,10,11,
                    19,11,17,
                };
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    //0,1,4,6,2,
                    0,1,
                    1,4,
                    4,6,
                    6,2,
                    2,0,

                    //0,2,8,9,3,
                    2,8,
                    8,9,
                    9,3,
                    3,0,

                    //1,0,3,7,5,
                    3,7,
                    7,5,
                    5,1,

                    //1,5,11,10,4,
                    5,11,
                    11,10,
                    10,4,

                    //6,4,10,16,12,
                    6,12,
                    7,13,

                    //18,19,16,12,14,
                    18,19,
                    19,16,
                    16,12,
                    12,14,
                    14,18,

                    //18,14,8,9,15,
                    14,8,
                    8,9,
                    9,15,
                    15,18,

                    //19,18,15,13,17,
                    15,13,
                    13,17,
                    17,19,

                    //19,17,11,10,16,
                    17,11,
                    11,10,
                    10,16,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            var vertices = new[]
            {
                new Vector3(-QuadInverseGoldenRatio, +1f, 0f),
                new Vector3(+QuadInverseGoldenRatio, +1f, 0f),

                new Vector3(-InverseGoldenRatio, +InverseGoldenRatio, -InverseGoldenRatio),
                new Vector3(-InverseGoldenRatio, +InverseGoldenRatio, +InverseGoldenRatio),
                new Vector3(+InverseGoldenRatio, +InverseGoldenRatio, -InverseGoldenRatio),
                new Vector3(+InverseGoldenRatio, +InverseGoldenRatio, +InverseGoldenRatio),

                new Vector3(0f, +QuadInverseGoldenRatio, -1f),
                new Vector3(0f, +QuadInverseGoldenRatio, +1f),

                new Vector3(-1f, 0f, -QuadInverseGoldenRatio),
                new Vector3(-1f, 0f, +QuadInverseGoldenRatio),
                new Vector3(+1f, 0f, -QuadInverseGoldenRatio),
                new Vector3(+1f, 0f, +QuadInverseGoldenRatio),

                new Vector3(0f, -QuadInverseGoldenRatio, -1f),
                new Vector3(0f, -QuadInverseGoldenRatio, +1f),

                new Vector3(-InverseGoldenRatio, -InverseGoldenRatio, -InverseGoldenRatio),
                new Vector3(-InverseGoldenRatio, -InverseGoldenRatio, +InverseGoldenRatio),
                new Vector3(+InverseGoldenRatio, -InverseGoldenRatio, -InverseGoldenRatio),
                new Vector3(+InverseGoldenRatio, -InverseGoldenRatio, +InverseGoldenRatio),

                new Vector3(-QuadInverseGoldenRatio, -1f, 0f),
                new Vector3(+QuadInverseGoldenRatio, -1f, 0f),
            };

            Matrix trn = Matrix.Scaling(width, height, depth) * Matrix.Translation(center);
            Vector3.TransformCoordinate(vertices, ref trn, vertices);

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a pyramid
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="depth">Depth</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreatePyramid(Topology topology, Vector3 center, float width, float height, float depth)
        {
            uint[] indices;
            if (topology == Topology.TriangleList)
            {
                indices = new uint[]
                {
                    0, 1, 2,
                    0, 2, 3,
                    0, 3, 4,
                    0, 4, 1,
                    1, 3, 2,
                    3, 1, 4,
                };
            }
            else if (topology == Topology.LineList)
            {
                indices = new uint[]
                {
                    0, 1,
                    0, 2,
                    0, 3,
                    0, 4,

                    1, 2,
                    2, 3,
                    3, 4,
                    4, 1,
                };
            }
            else
            {
                throw new NotImplementedException();
            }

            var vertices = new[]
            {
                new Vector3(0f, 1f, 0f),
                new Vector3(-1f, -1f, 1f),
                new Vector3(1f, -1f, 1f),
                new Vector3(1f, -1f, -1f),
                new Vector3(-1f, -1f, -1f),
            };

            Matrix trn = Matrix.Scaling(width, height, depth) * Matrix.Translation(center);
            Vector3.TransformCoordinate(vertices, ref trn, vertices);

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Indices = indices,
            };
        }

        /// <summary>
        /// Creates a hemispheric
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slices (vertical)</param>
        /// <param name="stackCount">Stacks (horizontal)</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateHemispheric(Topology topology, float radius, int sliceCount, int stackCount)
        {
            return CreateHemispheric(topology, Vector3.Zero, radius, sliceCount, stackCount);
        }
        /// <summary>
        /// Creates a hemispheric
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="radius">Radius</param>
        /// <param name="sliceCount">Slices (vertical)</param>
        /// <param name="stackCount">Stacks (horizontal)</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateHemispheric(Topology topology, Vector3 center, float radius, int sliceCount, int stackCount)
        {
            var vertList = new List<Vector3>();
            var normList = new List<Vector3>();
            var tangList = new List<Vector3>();
            var binmList = new List<Vector3>();
            var uvList = new List<Vector2>();

            sliceCount--;
            stackCount++;

            #region Positions

            float phiStep = MathUtil.PiOverTwo / stackCount;
            float thetaStep = MathUtil.TwoPi / sliceCount;
            float halfStep = thetaStep / MathUtil.TwoPi / 2f;

            for (int st = 0; st <= stackCount; st++)
            {
                float phi = st * phiStep;

                for (int sl = 0; sl <= sliceCount; sl++)
                {
                    float theta = sl * thetaStep;

                    float sinPhi = (float)Math.Sin(phi);
                    float cosPhi = (float)Math.Cos(phi);
                    float sinTheta = (float)Math.Sin(theta);
                    float cosTheta = (float)Math.Cos(theta);

                    float x = sinPhi * cosTheta;
                    float y = cosPhi;
                    float z = sinPhi * sinTheta;

                    float tX = -sinPhi * sinTheta;
                    float tY = 0.0f;
                    float tZ = +sinPhi * cosTheta;

                    var position = radius * new Vector3(x, y, z);
                    var normal = new Vector3(x, y, z);
                    var tangent = Vector3.Normalize(new Vector3(tX, tY, tZ));
                    var binormal = Vector3.Cross(normal, tangent);

                    float u = theta / MathUtil.TwoPi;
                    float v = phi / MathUtil.PiOverTwo;

                    if (st == 0)
                    {
                        u -= halfStep;
                    }

                    var texture = new Vector2(u, v);

                    vertList.Add(position + center);
                    normList.Add(normal);
                    tangList.Add(tangent);
                    binmList.Add(binormal);
                    uvList.Add(texture);
                }
            }

            #endregion

            if (topology == Topology.TriangleList)
            {
                var indexList = new List<int>();

                #region Indexes

                int ringVertexCount = sliceCount + 1;
                for (int st = 0; st < stackCount; st++)
                {
                    for (int sl = 0; sl < sliceCount; sl++)
                    {
                        indexList.Add((st + 1) * ringVertexCount + sl + 0);
                        indexList.Add((st + 0) * ringVertexCount + sl + 1);
                        indexList.Add((st + 1) * ringVertexCount + sl + 1);

                        if (st == 0)
                        {
                            continue;
                        }

                        indexList.Add((st + 0) * ringVertexCount + sl + 0);
                        indexList.Add((st + 0) * ringVertexCount + sl + 1);
                        indexList.Add((st + 1) * ringVertexCount + sl + 0);
                    }
                }

                #endregion

                return new GeometryDescriptor()
                {
                    Vertices = vertList.ToArray(),
                    Normals = normList.ToArray(),
                    Tangents = tangList.ToArray(),
                    Binormals = binmList.ToArray(),
                    Uvs = uvList.ToArray(),
                    Indices = indexList.Select(i => (uint)i).ToArray(),
                };
            }
            else if (topology == Topology.LineList)
            {
                var indexList = new List<uint>();

                var count = vertList.Count / sliceCount;

                for (int r = 0; r < count; r++)
                {
                    for (int i = 0; i < sliceCount; i++)
                    {
                        int index = sliceCount * r;
                        int i0 = index + i;
                        int i1 = index + ((i + 1) % sliceCount);

                        indexList.Add((uint)i0);
                        indexList.Add((uint)i1);
                    }
                }

                return new GeometryDescriptor()
                {
                    Vertices = vertList,
                    Indices = indexList,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Creates a cylinder
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <param name="sliceCount">Slice count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCylinder(Topology topology, float radius, float height, int sliceCount)
        {
            return CreateCylinder(topology, Vector3.Zero, radius, height, sliceCount);
        }
        /// <summary>
        /// Creates a cylinder
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center position</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <param name="sliceCount">Slice count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCylinder(Topology topology, Vector3 center, float radius, float height, int sliceCount)
        {
            var verts = new List<Vector3>();

            var bsePosition = new Vector3(center.X, center.Y - (height * 0.5f), center.Z);
            var capPosition = new Vector3(center.X, center.Y + (height * 0.5f), center.Z);

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < sliceCount; j++)
                {
                    float theta = (j / (float)sliceCount) * 2 * (float)Math.PI;
                    float st = (float)Math.Sin(theta), ct = (float)Math.Cos(theta);

                    verts.Add(bsePosition + new Vector3(radius * st, height * i, radius * ct));
                }
            }
            verts.AddRange(new[] { bsePosition, capPosition });

            if (topology == Topology.TriangleList)
            {
                int cBase = verts.Count - 2;
                int cCap = verts.Count - 1;

                var indexList = new List<int>();

                for (int i = 0; i < sliceCount; i++)
                {
                    var p0Base = i;
                    var p1Base = (i + 1) % sliceCount;

                    var p0Cap = p0Base + sliceCount;
                    var p1Cap = p1Base + sliceCount;

                    indexList.AddRange(new[]
                    {
                    // Base circle
                    cBase,
                    p1Base,
                    p0Base,
                    
                    // Cap circle
                    cCap,
                    p0Cap,
                    p1Cap,

                    // Side
                    p0Base, p1Base, p0Cap,
                    p1Base, p1Cap, p0Cap,
                });
                }

                var norms = new List<Vector3>();

                for (int i = 0; i < sliceCount * 2; i++)
                {
                    norms.Add(Vector3.Normalize(new(verts[i].X, 0, verts[i].Z)));
                }

                norms.AddRange(new[] { Vector3.Down, Vector3.Up });

                return new GeometryDescriptor()
                {
                    Vertices = verts,
                    Normals = norms,
                    Indices = indexList.Select(i => (uint)i).ToArray(),
                };
            }
            else if (topology == Topology.LineList)
            {
                var indexList = new List<int>();

                for (int i = 0; i < sliceCount; i++)
                {
                    int i0 = i;
                    int i1 = (i + 1) % sliceCount;

                    indexList.AddRange(new[] { i0, i1 });
                    indexList.AddRange(new[] { i0 + sliceCount, i1 + sliceCount });

                    indexList.AddRange(new[] { i0, i0 + sliceCount });
                }

                return new GeometryDescriptor()
                {
                    Vertices = verts,
                    Indices = indexList.Select(i => (uint)i).ToArray(),
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Creates a cylinder
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="cylinder">Bounding cylinder</param>
        /// <param name="sliceCount">Slice count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCylinder(Topology topology, BoundingCylinder cylinder, int sliceCount)
        {
            return CreateCylinder(topology, cylinder.Center, cylinder.Radius, cylinder.Height, sliceCount);
        }

        /// <summary>
        /// Creates a capsule
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCapsule(Topology topology, float radius, float height, int sliceCount, int stackCount)
        {
            return CreateCapsule(topology, Vector3.Zero, radius, height, sliceCount, stackCount);
        }
        /// <summary>
        /// Creates a capsule
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="center">Center</param>
        /// <param name="radius">Radius</param>
        /// <param name="height">Height</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCapsule(Topology topology, Vector3 center, float radius, float height, int sliceCount, int stackCount)
        {
            var verts = new List<Vector3>();

            // Create a hemispheric for each position
            var hemispheric = CreateHemispheric(topology, Vector3.Zero, radius, sliceCount, stackCount);

            float hh = (height - radius - radius) * 0.5f;

            // Cap
            verts.AddRange(hemispheric.Vertices.Select(v => new Vector3(v.X, v.Y + hh, v.Z)));
            uint offset = (uint)verts.Count;

            // Base
            verts.AddRange(hemispheric.Vertices.Select(v => new Vector3(v.X, -v.Y - hh, v.Z)));

            if (center != Vector3.Zero)
            {
                // Translate to center
                verts = verts.Select(v => v + center).ToList();
            }

            if (topology == Topology.TriangleList)
            {
                var norms = new List<Vector3>();

                //Populate normals
                norms.AddRange(hemispheric.Normals);
                norms.AddRange(hemispheric.Normals.Select(n => new Vector3(n.X, -n.Y, n.Z)));

                var indexList = new List<uint>();

                //Cap
                indexList.AddRange(hemispheric.Indices);

                //Base (reverse faces)
                for (uint i = 0; i < hemispheric.Indices.Count(); i += 3)
                {
                    indexList.Add(hemispheric.Indices.ElementAt((int)i + 0) + offset);
                    indexList.Add(hemispheric.Indices.ElementAt((int)i + 2) + offset);
                    indexList.Add(hemispheric.Indices.ElementAt((int)i + 1) + offset);
                }

                //Add the side faces
                uint capCylinderOffset = offset - (uint)sliceCount;
                uint bseCylinderOffset = (uint)verts.Count - (uint)sliceCount;
                for (uint i = 0; i < sliceCount; i++)
                {
                    uint p0 = i;
                    uint p1 = (i + 1) % (uint)sliceCount;

                    indexList.AddRange(new[]
                    {
                        // Side
                        p0 + bseCylinderOffset, p0 + capCylinderOffset, p1 + bseCylinderOffset,
                        p1 + bseCylinderOffset, p0 + capCylinderOffset, p1 + capCylinderOffset,
                    });
                }

                return new GeometryDescriptor()
                {
                    Vertices = verts,
                    Normals = norms,
                    Indices = indexList,
                };
            }
            else if (topology == Topology.LineList)
            {
                var indexList = new List<uint>();

                var count = verts.Count / sliceCount;

                for (uint r = 0; r < count; r++)
                {
                    for (uint i = 0; i < sliceCount; i++)
                    {
                        uint i0 = ((uint)sliceCount * r) + i;
                        uint i1 = ((uint)sliceCount * r) + ((i + 1) % (uint)sliceCount);

                        indexList.Add(i0);
                        indexList.Add(i1);
                    }
                }

                return new GeometryDescriptor()
                {
                    Vertices = verts,
                    Indices = indexList,
                };
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Creates a capsule
        /// </summary>
        /// <param name="topology">Topology</param>
        /// <param name="capsule">Axis aligned bounding capsule</param>
        /// <param name="sliceCount">Slice count</param>
        /// <param name="stackCount">Stack count</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCapsule(Topology topology, BoundingCapsule capsule, int sliceCount, int stackCount)
        {
            return CreateCapsule(topology, capsule.Center, capsule.Radius, capsule.Height, sliceCount, stackCount);
        }

        /// <summary>
        /// Creates a XZ plane
        /// </summary>
        /// <param name="size">Plane size</param>
        /// <param name="height">Plane height</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateXZPlane(float size, float height)
        {
            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-size*0.5f, +height, -size*0.5f),
                new Vector3(-size*0.5f, +height, +size*0.5f),
                new Vector3(+size*0.5f, +height, -size*0.5f),
                new Vector3(+size*0.5f, +height, +size*0.5f),
            };

            Vector3[] normals = new Vector3[]
            {
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
                Vector3.Up,
            };

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(0.0f, size),
                new Vector2(size, 0.0f),
                new Vector2(size, size),
            };

            uint[] indices = new uint[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Normals = normals,
                Uvs = uvs,
                Indices = indices,
            };
        }
        /// <summary>
        /// Creates a plane with the specified normal
        /// </summary>
        /// <param name="size">Plane size</param>
        /// <param name="height">Plane height</param>
        /// <param name="normal">Plane normal</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreatePlane(float size, float height, Vector3 normal)
        {
            var geometry = CreateXZPlane(size, height);

            var rotNormal = Vector3.Normalize(normal);
            if (rotNormal == Vector3.Up)
            {
                //No need to transform
                return geometry;
            }

            float angle = Helper.AngleSigned(Vector3.Up, rotNormal);

            Vector3 axis;
            if (angle == MathUtil.Pi)
            {
                //Parallel negative axis: Vector3.Down
                axis = Vector3.Left;
            }
            else
            {
                axis = Vector3.Normalize(Vector3.Cross(Vector3.Up, rotNormal));
            }

            geometry.Transform(Matrix.RotationAxis(axis, angle));

            return geometry;
        }
        /// <summary>
        /// Creates a curve plane
        /// </summary>
        /// <param name="size">Quad size</param>
        /// <param name="textureRepeat">Texture repeat</param>
        /// <param name="planeWidth">Plane width</param>
        /// <param name="planeTop">Plane top</param>
        /// <param name="planeBottom">Plane bottom</param>
        /// <returns>Returns a geometry descriptor</returns>
        public static GeometryDescriptor CreateCurvePlane(uint size, int textureRepeat, float planeWidth, float planeTop, float planeBottom)
        {
            Vector3[] vertices = new Vector3[(size + 1) * (size + 1)];
            Vector2[] uvs = new Vector2[(size + 1) * (size + 1)];

            // Determine the size of each quad on the sky plane.
            float quadSize = planeWidth / size;

            // Calculate the radius of the sky plane based on the width.
            float radius = planeWidth * 0.5f;

            // Calculate the height constant to increment by.
            float constant = (planeTop - planeBottom) / (radius * radius);

            // Calculate the texture coordinate increment value.
            float textureDelta = (float)textureRepeat / size;

            // Loop through the sky plane and build the coordinates based on the increment values given.
            for (uint j = 0; j <= size; j++)
            {
                for (uint i = 0; i <= size; i++)
                {
                    // Calculate the vertex coordinates.
                    float positionX = (-0.5f * planeWidth) + (i * quadSize);
                    float positionZ = (-0.5f * planeWidth) + (j * quadSize);
                    float positionY = planeTop - (constant * ((positionX * positionX) + (positionZ * positionZ)));

                    // Calculate the texture coordinates.
                    float tu = i * textureDelta;
                    float tv = j * textureDelta;

                    // Calculate the index into the sky plane array to add this coordinate.
                    uint ix = j * (size + 1) + i;

                    // Add the coordinates to the sky plane array.
                    vertices[ix] = new Vector3(positionX, positionY, positionZ);
                    uvs[ix] = new Vector2(tu, tv);
                }
            }


            // Create the index array.
            var indexList = new List<uint>();

            // Load the vertex and index array with the sky plane array data.
            for (uint j = 0; j < size; j++)
            {
                for (uint i = 0; i < size; i++)
                {
                    uint index1 = j * (size + 1) + i;
                    uint index2 = j * (size + 1) + (i + 1);
                    uint index3 = (j + 1) * (size + 1) + i;
                    uint index4 = (j + 1) * (size + 1) + (i + 1);

                    indexList.AddRange(new[]
                    {
                        index1, // Triangle 1 - Upper Left
                        index2, // Triangle 1 - Upper Right
                        index3, // Triangle 1 - Bottom Left

                        index3, // Triangle 2 - Bottom Left
                        index2, // Triangle 2 - Upper Right
                        index4, // Triangle 2 - Bottom Right
                    });
                }
            }

            return new GeometryDescriptor()
            {
                Vertices = vertices,
                Uvs = uvs,
                Indices = indexList.ToArray(),
            };
        }

        /// <summary>
        /// Compute normal of three points in the same plane
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">point 2</param>
        /// <param name="p3">point 3</param>
        /// <returns>Returns a normal descriptor</returns>
        public static NormalDescriptor ComputeNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var p = new Plane(p1, p2, p3);

            return new NormalDescriptor()
            {
                Normal = p.Normal,
            };
        }
        /// <summary>
        /// Calculate tangent, normal and bi-normals of triangle vertices
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="p3">Point 3</param>
        /// <param name="uv1">Texture uv 1</param>
        /// <param name="uv2">Texture uv 2</param>
        /// <param name="uv3">Texture uv 3</param>
        /// <returns>Returns a normal descriptor</returns>
        public static NormalDescriptor ComputeNormals(Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            // Calculate the two vectors for the face.
            var vector1 = p2 - p1;
            var vector2 = p3 - p1;

            // Calculate the tu and tv texture space vectors.
            var tuVector = new Vector2(uv2.X - uv1.X, uv3.X - uv1.X);
            var tvVector = new Vector2(uv2.Y - uv1.Y, uv3.Y - uv1.Y);

            // Calculate the denominator of the tangent / bi-normal equation.
            var den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and bi-normal.
            var tangent = new Vector3
            {
                X = (tvVector[1] * vector1.X - tvVector[0] * vector2.X) * den,
                Y = (tvVector[1] * vector1.Y - tvVector[0] * vector2.Y) * den,
                Z = (tvVector[1] * vector1.Z - tvVector[0] * vector2.Z) * den
            };

            var binormal = new Vector3
            {
                X = (tuVector[0] * vector2.X - tuVector[1] * vector1.X) * den,
                Y = (tuVector[0] * vector2.Y - tuVector[1] * vector1.Y) * den,
                Z = (tuVector[0] * vector2.Z - tuVector[1] * vector1.Z) * den
            };

            tangent.Normalize();
            binormal.Normalize();

            // Calculate the cross product of the tangent and bi-normal which will give the normal vector.
            var normal = Vector3.Cross(tangent, binormal);

            return new NormalDescriptor()
            {
                Normal = normal,
                Tangent = tangent,
                Binormal = binormal,
            };
        }

        /// <summary>
        /// Generates a bounding box from a vertex item list
        /// </summary>
        /// <param name="vertexListItems">Vertex item list</param>
        /// <returns>Returns the minimum bounding box that contains all the specified vertex item list</returns>
        public static BoundingBox CreateBoundingBox<T>(IEnumerable<T> vertexListItems) where T : IVertexList
        {
            var points = vertexListItems.SelectMany(v => v.GetVertices()).Distinct().ToArray();

            return SharpDXExtensions.BoundingBoxFromPoints(points);
        }
        /// <summary>
        /// Generates a bounding sphere from a vertex item list
        /// </summary>
        /// <param name="vertexListItems">Vertex item list</param>
        /// <returns>Returns the minimum bounding sphere that contains all the specified vertex item list</returns>
        public static BoundingSphere CreateBoundingSphere<T>(IEnumerable<T> vertexListItems) where T : IVertexList
        {
            var points = vertexListItems.SelectMany(v => v.GetVertices()).Distinct().ToArray();

            return SharpDXExtensions.BoundingSphereFromPoints(points);
        }
        /// <summary>
        /// Generates a bounding cylinder from a vertex item list
        /// </summary>
        /// <param name="vertexListItems">Vertex item list</param>
        /// <returns>Returns the minimum bounding cylinder that contains all the specified vertex item list</returns>
        public static BoundingCylinder CreateBoundingCylinder<T>(IEnumerable<T> vertexListItems) where T : IVertexList
        {
            var points = vertexListItems.SelectMany(v => v.GetVertices()).Distinct().ToArray();

            return BoundingCylinder.FromPoints(points);
        }
        /// <summary>
        /// Generates a bounding capsule from a vertex item list
        /// </summary>
        /// <param name="vertexListItems">Vertex item list</param>
        /// <returns>Returns the minimum bounding capsule that contains all the specified vertex item list</returns>
        public static BoundingCapsule CreateBoundingCapsule<T>(IEnumerable<T> vertexListItems) where T : IVertexList
        {
            var points = vertexListItems.SelectMany(v => v.GetVertices()).Distinct().ToArray();

            return BoundingCapsule.FromPoints(points);
        }

        /// <summary>
        /// Computes constraints into vertices
        /// </summary>
        /// <param name="constraint">Constraint</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="res">Resulting vertices</param>
        public static void ConstraintVertices(BoundingBox constraint, IEnumerable<VertexData> vertices, out IEnumerable<VertexData> res)
        {
            var tmpVertices = new List<VertexData>();

            for (int i = 0; i < vertices.Count(); i += 3)
            {
                if (constraint.Contains(vertices.ElementAt(i + 0).Position.Value) != ContainmentType.Disjoint ||
                    constraint.Contains(vertices.ElementAt(i + 1).Position.Value) != ContainmentType.Disjoint ||
                    constraint.Contains(vertices.ElementAt(i + 2).Position.Value) != ContainmentType.Disjoint)
                {
                    tmpVertices.Add(vertices.ElementAt(i + 0));
                    tmpVertices.Add(vertices.ElementAt(i + 1));
                    tmpVertices.Add(vertices.ElementAt(i + 2));
                }
            }

            res = tmpVertices;
        }
        /// <summary>
        /// Computes constraints into vertices
        /// </summary>
        /// <param name="constraint">Constraint</param>
        /// <param name="vertices">Vertices</param>
        /// <returns>Resulting vertices</returns>
        public static async Task<IEnumerable<VertexData>> ConstraintVerticesAsync(BoundingBox constraint, IEnumerable<VertexData> vertices)
        {
            return await Task.Factory.StartNew(() =>
            {
                ConstraintVertices(constraint, vertices, out var tres);

                return tres;
            },
            TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// Computes constraints into vertices and indices
        /// </summary>
        /// <param name="constraint">Constraint</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <param name="resVertices">Resulting vertices</param>
        /// <param name="resIndices">Resulting indices</param>
        public static void ConstraintIndices(BoundingBox constraint, IEnumerable<VertexData> vertices, IEnumerable<uint> indices, out IEnumerable<VertexData> resVertices, out IEnumerable<uint> resIndices)
        {
            var tmpIndices = new List<uint>();

            // Gets all triangle indices into the constraint
            for (int i = 0; i < indices.Count(); i += 3)
            {
                var i0 = indices.ElementAt(i + 0);
                var i1 = indices.ElementAt(i + 1);
                var i2 = indices.ElementAt(i + 2);

                var v0 = vertices.ElementAt((int)i0);
                var v1 = vertices.ElementAt((int)i1);
                var v2 = vertices.ElementAt((int)i2);

                if (constraint.Contains(v0.Position.Value) != ContainmentType.Disjoint ||
                    constraint.Contains(v1.Position.Value) != ContainmentType.Disjoint ||
                    constraint.Contains(v2.Position.Value) != ContainmentType.Disjoint)
                {
                    tmpIndices.Add(i0);
                    tmpIndices.Add(i1);
                    tmpIndices.Add(i2);
                }
            }

            var tmpVertices = new List<VertexData>();
            var dict = new List<Tuple<uint, uint>>();

            // Adds all the selected vertices for each unique index, and create a index translator for the new vertex list
            foreach (uint index in tmpIndices.Distinct())
            {
                tmpVertices.Add(vertices.ElementAt((int)index));
                dict.Add(new Tuple<uint, uint>(index, (uint)tmpVertices.Count - 1));
            }

            // Set the new index values
            for (int i = 0; i < tmpIndices.Count; i++)
            {
                uint newIndex = dict.Find(d => d.Item1 == tmpIndices[i]).Item2;

                tmpIndices[i] = newIndex;
            }

            resVertices = tmpVertices;
            resIndices = tmpIndices;
        }
        /// <summary>
        /// Computes constraints into vertices and indices
        /// </summary>
        /// <param name="constraint">Constraint</param>
        /// <param name="vertices">Vertices</param>
        /// <param name="indices">Indices</param>
        /// <returns>Resulting vertices and indices</returns>
        public static async Task<(IEnumerable<VertexData> Vertices, IEnumerable<uint> Indices)> ConstraintIndicesAsync(BoundingBox constraint, IEnumerable<VertexData> vertices, IEnumerable<uint> indices)
        {
            return await Task.Factory.StartNew(() =>
            {
                ConstraintIndices(constraint, vertices, indices, out var tvertices, out var tindices);

                return (tvertices, tindices);
            },
            TaskCreationOptions.LongRunning);
        }
    }
}
