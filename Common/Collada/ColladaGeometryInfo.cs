using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D;

namespace Common.Collada
{
    using Common.Utils;

    public class ColladaGeometryInfo
    {
        private IVertex[] vertices = null;
        private Vector3[] positions = null;
        private Vector3[] normals = null;
        private Vector2[] uvs = null;
        private Color4[] colors = null;
        private Vector2[] sizes = null;

        public VertexPosition[] Position
        {
            get
            {
                List<VertexPosition> vList = new List<VertexPosition>();

                Array.ForEach(this.vertices, (v) => { if (v is VertexPosition) vList.Add((VertexPosition)v); });

                return vList.ToArray();
            }
        }
        public VertexPositionColor[] PositionColor
        {
            get
            {
                List<VertexPositionColor> vList = new List<VertexPositionColor>();

                Array.ForEach(this.vertices, (v) => { if (v is VertexPositionColor) vList.Add((VertexPositionColor)v); });

                return vList.ToArray();
            }
        }
        public VertexPositionNormalColor[] PositionNormalColor
        {
            get
            {
                List<VertexPositionNormalColor> vList = new List<VertexPositionNormalColor>();

                Array.ForEach(this.vertices, (v) => { if (v is VertexPositionNormalColor) vList.Add((VertexPositionNormalColor)v); });

                return vList.ToArray();
            }
        }
        public VertexPositionTexture[] PositionTexture
        {
            get
            {
                List<VertexPositionTexture> vList = new List<VertexPositionTexture>();

                Array.ForEach(this.vertices, (v) => { if (v is VertexPositionTexture) vList.Add((VertexPositionTexture)v); });

                return vList.ToArray();
            }
        }
        public VertexPositionNormalTexture[] PositionNormalTexture
        {
            get
            {
                List<VertexPositionNormalTexture> vList = new List<VertexPositionNormalTexture>();

                Array.ForEach(this.vertices, (v) => { if (v is VertexPositionNormalTexture) vList.Add((VertexPositionNormalTexture)v); });

                return vList.ToArray();
            }
        }
        public VertexBillboard[] Billboard
        {
            get
            {
                List<VertexBillboard> vList = new List<VertexBillboard>();

                Array.ForEach(this.vertices, (v) => { if (v is VertexBillboard) vList.Add((VertexBillboard)v); });

                return vList.ToArray();
            }
        }
        public Material Material { get; set; }
        public VertexTypes VertexType { get; private set; }
        public PrimitiveTopology Topology { get; private set; }

        public void AddVertices(IVertex[] vertices, PrimitiveTopology topology)
        {
            if (vertices != null && vertices.Length > 0)
            {
                this.vertices = vertices;
                this.VertexType = this.vertices[0].GetVertexType();
                this.Topology = topology;
            }
        }
        public Vector3[] GetPositions()
        {
            if (this.positions == null)
            {
                List<Vector3> positionList = new List<Vector3>();

                if (this.VertexType == VertexTypes.Position)
                {
                    Array.ForEach(this.Position, (v) => { positionList.Add(v.Position); });
                }
                else if (this.VertexType == VertexTypes.PositionColor)
                {
                    Array.ForEach(this.PositionColor, (v) => { positionList.Add(v.Position); });
                }
                else if (this.VertexType == VertexTypes.PositionNormalColor)
                {
                    Array.ForEach(this.PositionNormalColor, (v) => { positionList.Add(v.Position); });
                }
                else if (this.VertexType == VertexTypes.PositionTexture)
                {
                    Array.ForEach(this.PositionTexture, (v) => { positionList.Add(v.Position); });
                }
                else if (this.VertexType == VertexTypes.PositionNormalTexture)
                {
                    Array.ForEach(this.PositionNormalTexture, (v) => { positionList.Add(v.Position); });
                }
                else if (this.VertexType == VertexTypes.Billboard)
                {
                    Array.ForEach(this.Billboard, (v) => { positionList.Add(v.Position); });
                }

                this.positions = positionList.ToArray();
            }

            return this.positions;
        }
        public Vector3[] GetNormals()
        {
            if (this.normals == null)
            {
                List<Vector3> normalList = new List<Vector3>();

                if (this.VertexType == VertexTypes.PositionNormalColor)
                {
                    Array.ForEach(this.PositionNormalColor, (v) => { normalList.Add(v.Normal); });
                }
                else if (this.VertexType == VertexTypes.PositionNormalTexture)
                {
                    Array.ForEach(this.PositionNormalTexture, (v) => { normalList.Add(v.Normal); });
                }

                this.normals = normalList.ToArray();
            }

            return this.normals;
        }
        public Vector2[] GetUVs()
        {
            if (this.uvs == null)
            {
                List<Vector2> uvsList = new List<Vector2>();

                if (this.VertexType == VertexTypes.PositionTexture)
                {
                    Array.ForEach(this.PositionTexture, (v) => { uvsList.Add(v.Texture); });
                }
                else if (this.VertexType == VertexTypes.PositionNormalTexture)
                {
                    Array.ForEach(this.PositionNormalTexture, (v) => { uvsList.Add(v.Texture); });
                }

                this.uvs = uvsList.ToArray();
            }

            return this.uvs;
        }
        public Color4[] GetColors()
        {
            if (this.colors == null)
            {
                List<Color4> colorList = new List<Color4>();

                if (this.VertexType == VertexTypes.PositionColor)
                {
                    Array.ForEach(this.PositionColor, (v) => { colorList.Add(v.Color); });
                }
                else if (this.VertexType == VertexTypes.PositionNormalColor)
                {
                    Array.ForEach(this.PositionNormalColor, (v) => { colorList.Add(v.Color); });
                }

                this.colors = colorList.ToArray();
            }

            return this.colors;
        }
        public Vector2[] GetSizes()
        {
            if (this.sizes == null)
            {
                List<Vector2> sizeList = new List<Vector2>();

                if (this.VertexType == VertexTypes.Billboard)
                {
                    Array.ForEach(this.Billboard, (v) => { sizeList.Add(v.Size); });
                }

                this.sizes = sizeList.ToArray();
            }

            return this.sizes;
        }
        public Vector3 GetMin()
        {
            Vector3 min = new Vector3(float.MaxValue);

            Vector3[] positions = this.GetPositions();
            for (int i = 0; i < positions.Length; i++)
            {
                min = Vector3.Min(min, positions[i]);
            }

            return min;
        }
        public Vector3 GetMax()
        {
            Vector3 max = new Vector3(float.MinValue);

            Vector3[] positions = this.GetPositions();
            for (int i = 0; i < positions.Length; i++)
            {
                max = Vector3.Max(max, positions[i]);
            }

            return max;
        }
        public Triangle[] ComputeTriangleList()
        {
            List<Triangle> triangleList = new List<Triangle>();

            if (this.Topology == PrimitiveTopology.TriangleList ||
                this.Topology == PrimitiveTopology.TriangleStrip)
            {
                Vector3[] positions = this.GetPositions();

                for (int i = 0; i < positions.Length; i += 3)
                {
                    Triangle tri = new Triangle(positions[i], positions[i + 1], positions[i + 2]);

                    triangleList.Add(tri);
                }
            }

            return triangleList.ToArray();
        }
        public BoundingBox ComputeBoundingBox()
        {
            return BoundingBox.FromPoints(this.GetPositions());
        }
        public BoundingSphere ComputeBoundingSphere()
        {
            return BoundingSphere.FromPoints(this.GetPositions());
        }
        public OrientedBoundingBox ComputeOrientedBoundingBox()
        {
            return new OrientedBoundingBox(this.GetPositions());
        }
    }
}
