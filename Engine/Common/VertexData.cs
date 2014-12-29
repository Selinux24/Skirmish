using System;
using System.Collections.Generic;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace Engine.Common
{
    using Engine.Helpers;

    [Serializable]
    public struct VertexData
    {
        public int FaceIndex;
        public int VertexIndex;
        public Vector3? Position;
        public Vector3? Normal;
        public Vector3? Tangent;
        public Vector3? BiNormal;
        public Vector2? Texture;
        public Color4? Color;
        public Vector2? Size;
        public float[] Weights;
        public byte[] BoneIndices;

        public static VertexData CreateVertexBillboard(Vector3 position, Vector2 size)
        {
            return new VertexData()
            {
                Position = position,
                Size = size,
            };
        }
        public static VertexData CreateVertexPosition(Vector3 position)
        {
            return new VertexData()
            {
                Position = position,
            };
        }
        public static VertexData CreateVertexPositionColor(Vector3 position, Color4 color)
        {
            return new VertexData()
            {
                Position = position,
                Color = color,
            };
        }
        public static VertexData CreateVertexPositionTexture(Vector3 position, Vector2 texture)
        {
            return new VertexData()
            {
                Position = position,
                Texture = texture,
            };
        }
        public static VertexData CreateVertexPositionNormalColor(Vector3 position, Vector3 normal, Color4 color)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Color = color,
            };
        }
        public static VertexData CreateVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Texture = texture,
            };
        }
        public static VertexData CreateVertexPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texture)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
                BiNormal = binormal,
                Texture = texture,
            };
        }
        public static VertexData CreateVertexSkinnedPosition(Vector3 position, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        public static VertexData CreateVertexSkinnedPositionColor(Vector3 position, Color4 color, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Color = color,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        public static VertexData CreateVertexSkinnedPositionTexture(Vector3 position, Vector2 texture, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Texture = texture,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        public static VertexData CreateVertexSkinnedPositionNormalColor(Vector3 position, Vector3 normal, Color4 color, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Color = color,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        public static VertexData CreateVertexSkinnedPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Texture = texture,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        public static VertexData CreateVertexSkinnedPositionNormalTextureTangent(Vector3 position, Vector3 normal, Vector3 tangent, Vector3 binormal, Vector2 texture, float[] weights, byte[] boneIndices)
        {
            return new VertexData()
            {
                Position = position,
                Normal = normal,
                Tangent = tangent,
                BiNormal = binormal,
                Texture = texture,
                Weights = weights,
                BoneIndices = boneIndices,
            };
        }
        public static bool IsSkinned(VertexTypes vertexTypes)
        {
            return
                vertexTypes == VertexTypes.PositionSkinned ||
                vertexTypes == VertexTypes.PositionColorSkinned ||
                vertexTypes == VertexTypes.PositionNormalColorSkinned ||
                vertexTypes == VertexTypes.PositionTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureTangentSkinned;
        }
        public static bool IsTextured(VertexTypes vertexTypes)
        {
            return
                vertexTypes == VertexTypes.Billboard ||

                vertexTypes == VertexTypes.PositionTexture ||
                vertexTypes == VertexTypes.PositionNormalTexture ||
                vertexTypes == VertexTypes.PositionNormalTextureTangent ||

                vertexTypes == VertexTypes.PositionTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureSkinned ||
                vertexTypes == VertexTypes.PositionNormalTextureTangentSkinned;
        }
        public static VertexTypes GetSkinnedEquivalent(VertexTypes vertexType)
        {
            if (vertexType == VertexTypes.Position) return VertexTypes.PositionSkinned;
            if (vertexType == VertexTypes.PositionColor) return VertexTypes.PositionColorSkinned;
            if (vertexType == VertexTypes.PositionNormalColor) return VertexTypes.PositionNormalColorSkinned;
            if (vertexType == VertexTypes.PositionTexture) return VertexTypes.PositionTextureSkinned;
            if (vertexType == VertexTypes.PositionNormalTexture) return VertexTypes.PositionNormalTextureSkinned;
            if (vertexType == VertexTypes.PositionNormalTextureTangent) return VertexTypes.PositionNormalTextureTangentSkinned;

            return VertexTypes.Unknown;
        }

        public static Buffer CreateVertexBuffer(Device device, IVertexData[] vertices)
        {
            Buffer buffer = null;

            if (vertices != null && vertices.Length > 0)
            {
                if (vertices[0].VertexType == VertexTypes.Billboard)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexBillboard)v));
                }
                else if (vertices[0].VertexType == VertexTypes.Position)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexPosition)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColor)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionColor)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalColor)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTexture)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionTexture)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalTexture)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalTextureTangent)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPosition)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionColor)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalColor)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionTexture)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalTexture)v));
                }
                else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
                {
                    buffer = device.CreateIndexBufferWrite(Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalTextureTangent)v));
                }
                else
                {
                    throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
                }
            }

            return buffer;
        }
        public static void WriteVertexBuffer(DeviceContext deviceContext, Buffer buffer, IVertexData[] vertices)
        {
            if (vertices[0].VertexType == VertexTypes.Billboard)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexBillboard)v));
            }
            else if (vertices[0].VertexType == VertexTypes.Position)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColor)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColor)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTexture)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTexture)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangent)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexPositionNormalTextureTangent)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPosition)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionColorSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalColorSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalColor)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionTextureSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalTexture)v));
            }
            else if (vertices[0].VertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                deviceContext.WriteBuffer(buffer, Array.ConvertAll((IVertexData[])vertices, v => (VertexSkinnedPositionNormalTextureTangent)v));
            }
            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertices[0].VertexType));
            }
        }
        public static IVertexData[] Convert(VertexTypes vertexType, VertexData[] vertices, Weight[] weights)
        {
            List<IVertexData> vertexList = new List<IVertexData>();

            if (vertexType == VertexTypes.Billboard)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexBillboard.Create(v)); });
            }
            else if (vertexType == VertexTypes.Position)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexPosition.Create(v)); });
            }
            else if (vertexType == VertexTypes.PositionColor)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexPositionColor.Create(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalColor)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexPositionNormalColor.Create(v)); });
            }
            else if (vertexType == VertexTypes.PositionTexture)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexPositionTexture.Create(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalTexture)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexPositionNormalTexture.Create(v)); });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangent)
            {
                Array.ForEach(vertices, (v) => { vertexList.Add(VertexPositionNormalTextureTangent.Create(v)); });
            }

            else if (vertexType == VertexTypes.PositionSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexSkinnedPosition.Create(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionColorSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexSkinnedPositionColor.Create(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalColorSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexSkinnedPositionNormalColor.Create(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionTextureSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexSkinnedPositionTexture.Create(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexSkinnedPositionNormalTexture.Create(v, vw));
                });
            }
            else if (vertexType == VertexTypes.PositionNormalTextureTangentSkinned)
            {
                Array.ForEach(vertices, (v) =>
                {
                    Weight[] vw = Array.FindAll<Weight>(weights, w => w.VertexIndex == v.VertexIndex);

                    vertexList.Add(VertexSkinnedPositionNormalTextureTangent.Create(v, vw));
                });
            }

            else
            {
                throw new Exception(string.Format("Unknown vertex type: {0}", vertexType));
            }

            return vertexList.ToArray();
        }

        public static void CalculateNormals(VertexData vertex1, VertexData vertex2, VertexData vertex3, out Vector3 tangent, out Vector3 binormal, out Vector3 normal)
        {
            // Calculate the two vectors for the face.
            Vector3 vector1 = vertex2.Position.Value - vertex1.Position.Value;
            Vector3 vector2 = vertex3.Position.Value - vertex1.Position.Value;

            // Calculate the tu and tv texture space vectors.
            Vector2 tuVector = new Vector2(
                vertex2.Texture.Value.X - vertex1.Texture.Value.X,
                vertex3.Texture.Value.X - vertex1.Texture.Value.X);
            Vector2 tvVector = new Vector2(
                vertex2.Texture.Value.Y - vertex1.Texture.Value.Y,
                vertex3.Texture.Value.Y - vertex1.Texture.Value.Y);

            // Calculate the denominator of the tangent / binormal equation.
            var den = 1.0f / (tuVector[0] * tvVector[1] - tuVector[1] * tvVector[0]);

            // Calculate the cross products and multiply by the coefficient to get the tangent and binormal.
            tangent.X = (tvVector[1] * vector1.X - tvVector[0] * vector2.X) * den;
            tangent.Y = (tvVector[1] * vector1.Y - tvVector[0] * vector2.Y) * den;
            tangent.Z = (tvVector[1] * vector1.Z - tvVector[0] * vector2.Z) * den;

            binormal.X = (tuVector[0] * vector2.X - tuVector[1] * vector1.X) * den;
            binormal.Y = (tuVector[0] * vector2.Y - tuVector[1] * vector1.Y) * den;
            binormal.Z = (tuVector[0] * vector2.Z - tuVector[1] * vector1.Z) * den;

            tangent.Normalize();
            binormal.Normalize();

            // Calculate the cross product of the tangent and binormal which will give the normal vector.
            normal = Vector3.Cross(tangent, binormal);

            normal.Normalize();
        }

        public void Transform(Matrix transform)
        {
            if (!transform.IsIdentity)
            {
                if (this.Position.HasValue)
                {
                    Vector3 position = this.Position.Value;

                    Vector3.TransformCoordinate(ref position, ref transform, out position);

                    this.Position = position;
                }

                if (this.Normal.HasValue)
                {
                    Vector3 normal = this.Normal.Value;

                    Vector3.TransformNormal(ref normal, ref transform, out normal);

                    this.Normal = normal;
                }

                if (this.Tangent.HasValue)
                {
                    Vector3 tangent = this.Tangent.Value;

                    Vector3.TransformNormal(ref tangent, ref transform, out tangent);

                    this.Tangent = tangent;
                }

                if (this.BiNormal.HasValue)
                {
                    Vector3 biNormal = this.BiNormal.Value;

                    Vector3.TransformNormal(ref biNormal, ref transform, out biNormal);

                    this.BiNormal = biNormal;
                }
            }
        }
        public override string ToString()
        {
            string text = null;

            if (this.Weights != null && this.Weights.Length > 0) text += "Skinned; ";

            text += string.Format("FaceIndex: {0}; ", this.FaceIndex);
            text += string.Format("VertexIndex: {0}; ", this.VertexIndex);

            if (this.Position.HasValue) text += string.Format("Position: {0}; ", this.Position);
            if (this.Normal.HasValue) text += string.Format("Normal: {0}; ", this.Normal);
            if (this.Tangent.HasValue) text += string.Format("Tangent: {0}; ", this.Tangent);
            if (this.BiNormal.HasValue) text += string.Format("BiNormal: {0}; ", this.BiNormal);
            if (this.Texture.HasValue) text += string.Format("Texture: {0}; ", this.Texture);
            if (this.Color.HasValue) text += string.Format("Color: {0}; ", this.Color);
            if (this.Size.HasValue) text += string.Format("Size: {0}; ", this.Size);
            if (this.Weights != null && this.Weights.Length > 0) text += string.Format("Weights: {0}; ", this.Weights.Length);
            if (this.BoneIndices != null && this.BoneIndices.Length > 0) text += string.Format("BoneIndices: {0}; ", this.BoneIndices.Length);

            return text;
        }
    }
}
