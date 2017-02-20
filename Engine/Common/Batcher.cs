using System;
using SharpDX;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;
using InputElement = SharpDX.Direct3D11.InputElement;
using InputClassification = SharpDX.Direct3D11.InputClassification;
using SharpDX.DXGI;

namespace Engine.Common
{
    using Engine.Helpers;

    /// <summary>
    /// Batcher
    /// </summary>
    /// <typeparam name="T">Data type</typeparam>
    class Batcher<T> : IDisposable where T : struct
    {
        private Game game = null;

        /// <summary>
        /// Buffer
        /// </summary>
        public Buffer[] Buffers;
        /// <summary>
        /// Vertex buffer binding
        /// </summary>
        public VertexBufferBinding[][] BufferBindings;

        private Dictionary<BatchChannels, InputElement[]> inputElements = new Dictionary<BatchChannels, InputElement[]>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        public Batcher(Game game)
        {
            this.game = game;
        }

        public void Dispose()
        {
            Helper.Dispose(this.Buffers);
        }

        public void AddComponents(ICollection<Drawable> components)
        {
            BatchComponents bcp = new BatchComponents();

            //Decompose
            foreach (var component in components)
            {
                //component.DecomposeGeometry(ref bcp);
            }

            this.Buffers = new Buffer[12];
            this.BufferBindings = new VertexBufferBinding[12][];

            int bufferIndex = 0;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("PositionBuffer", bcp.positions.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Vector3)), 0) };
            inputElements.Add(BatchChannels.Position, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("NormalBuffer", bcp.normals.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Vector3)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("NORMAL", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("ColorBuffer", bcp.colors.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Color4)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("ColorBuffer", bcp.tex0.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Vector2)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("Tex1Buffer", bcp.tex1.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Vector2)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("TEXCOORD", 1, Format.R32G32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("TangentBuffer", bcp.tangents.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Vector3)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("TANGENT", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("SkinningBuffer", bcp.skinningData.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Skinning)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("WEIGHTS", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("BONEINDICES", 0, Format.R8G8B8A8_UInt, 12, bufferIndex, InputClassification.PerVertexData, 0 ),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("InstancingBuffer", bcp.instancingData.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(InstancingData)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("localTransform", 0, Format.R32G32B32A32_Float, 0, bufferIndex, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 1, Format.R32G32B32A32_Float, 16, bufferIndex, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 2, Format.R32G32B32A32_Float, 32, bufferIndex, InputClassification.PerInstanceData, 1),
                new InputElement("localTransform", 3, Format.R32G32B32A32_Float, 48, bufferIndex, InputClassification.PerInstanceData, 1),
                new InputElement("animationData", 0, Format.R32G32B32_UInt, 64, bufferIndex, InputClassification.PerInstanceData, 1),
                new InputElement("textureIndex", 0, Format.R32_Float, 76, bufferIndex, InputClassification.PerInstanceData, 1),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("BillboardsBuffer", bcp.billboards.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(Billboard)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("SIZE", 0, Format.R32G32_Float, 12, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("ParticlesBuffer", bcp.particles.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(ParticleData)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("VELOCITY", 0, Format.R32G32B32_Float, 12, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("RANDOM", 0, Format.R32G32B32A32_Float, 24, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("MAX_AGE", 0, Format.R32_Float, 40, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("EmittersBuffer", bcp.emitters.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(EmitterData)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("TYPE", 0, Format.R32_UInt, 0, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("EMISSION_TIME", 0, Format.R32_Float, 4, bufferIndex, InputClassification.PerVertexData, 0),
            });

            bufferIndex++;
            this.Buffers[bufferIndex] = game.Graphics.Device.CreateVertexBufferImmutable("SpritesBuffer", bcp.sprites.ToArray());
            this.BufferBindings[bufferIndex] = new VertexBufferBinding[] { new VertexBufferBinding(this.Buffers[bufferIndex], Marshal.SizeOf(typeof(SpriteData)), 0) };
            inputElements.Add(BatchChannels.Normal, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, bufferIndex, InputClassification.PerVertexData, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 28, bufferIndex, InputClassification.PerVertexData, 0),
            });
        }
    }

    public struct Skinning
    {
        /// <summary>
        /// Weight 1
        /// </summary>
        public float Weight1;
        /// <summary>
        /// Weight 2
        /// </summary>
        public float Weight2;
        /// <summary>
        /// Weight 3
        /// </summary>
        public float Weight3;
        /// <summary>
        /// Bone 1
        /// </summary>
        public byte BoneIndex1;
        /// <summary>
        /// Bone 2
        /// </summary>
        public byte BoneIndex2;
        /// <summary>
        /// Bone 3
        /// </summary>
        public byte BoneIndex3;
        /// <summary>
        /// Bone 4
        /// </summary>
        public byte BoneIndex4;
    }

    public struct InstancingData
    {
        /// <summary>
        /// Local transformation for the instance
        /// </summary>
        public Matrix Local;
        /// <summary>
        /// Texture index
        /// </summary>
        public uint TextureIndex;
        /// <summary>
        /// Animation offset in current clip
        /// </summary>
        public uint AnimationOffset;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Padding1;
        /// <summary>
        /// Padding
        /// </summary>
        public uint Padding2;
    }

    public struct Billboard
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Sprite size
        /// </summary>
        public Vector2 Size;
    }

    public struct ParticleData
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Velocity
        /// </summary>
        public Vector3 Velocity;
        /// <summary>
        /// Particle random values
        /// </summary>
        public Vector4 RandomValues;
        /// <summary>
        /// Particle maximum age
        /// </summary>
        public float MaxAge;
    }

    public struct EmitterData
    {
        /// <summary>
        /// Particle type
        /// </summary>
        public uint Type;
        /// <summary>
        /// Total emission time
        /// </summary>
        public float EmissionTime;
    }

    public struct SpriteData
    {
        /// <summary>
        /// Position
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Color
        /// </summary>
        public Color4 Color;
        /// <summary>
        /// Texture UV
        /// </summary>
        public Vector2 Texture;
    }

    public enum BatchChannels
    {
        Position,
        Normal,
        Color,
        Texture,
        Tangent,
        Skinning,
        Instancing,
        Billboard,
        Particle,
        Emitter,
        Sprite,
    }

    public class BatchComponents
    {
        public List<Vector3> positions = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Color4> colors = new List<Color4>();
        public List<Vector2> tex0 = new List<Vector2>();
        public List<Vector2> tex1 = new List<Vector2>();
        public List<Vector3> tangents = new List<Vector3>();
        public List<Skinning> skinningData = new List<Skinning>();
        public List<InstancingData> instancingData = new List<InstancingData>();
        public List<Billboard> billboards = new List<Billboard>();
        public List<ParticleData> particles = new List<ParticleData>();
        public List<EmitterData> emitters = new List<EmitterData>();
        public List<SpriteData> sprites = new List<SpriteData>();

        public int AddPositions(Vector3[] positions)
        {
            int offset = this.positions.Count;

            this.positions.AddRange(positions);

            return offset;
        }

        public int AddNormals(Vector3[] normals)
        {
            int offset = this.normals.Count;

            this.normals.AddRange(normals);

            return offset;
        }

        public int AddColors(Color4[] colors)
        {
            int offset = this.colors.Count;

            this.colors.AddRange(colors);

            return offset;
        }

        public int AddTex0(Vector2[] tex0)
        {
            int offset = this.tex0.Count;

            this.tex0.AddRange(tex0);

            return offset;
        }

        public int AddTex1(Vector2[] tex1)
        {
            int offset = this.tex1.Count;

            this.tex1.AddRange(tex1);

            return offset;
        }

        public int AddTangents(Vector3[] tangents)
        {
            int offset = this.tangents.Count;

            this.tangents.AddRange(tangents);

            return offset;
        }

        public int AddSkinningData(Skinning[] skinningData)
        {
            int offset = this.skinningData.Count;

            this.skinningData.AddRange(skinningData);

            return offset;
        }

        public int AddInstancingData(InstancingData[] instancingData)
        {
            int offset = this.instancingData.Count;

            this.instancingData.AddRange(instancingData);

            return offset;
        }

        public int AddBillboards(Billboard[] billboards)
        {
            int offset = this.positions.Count;

            this.billboards.AddRange(billboards);

            return offset;
        }

        public int AddParticleData(ParticleData[] particles)
        {
            int offset = this.particles.Count;

            this.particles.AddRange(particles);

            return offset;
        }

        public int AddEmitterData(EmitterData[] emitters)
        {
            int offset = this.emitters.Count;

            this.emitters.AddRange(emitters);

            return offset;
        }

        public int AddSprites(SpriteData[] sprites)
        {
            int offset = this.sprites.Count;

            this.sprites.AddRange(sprites);

            return offset;
        }
    }
}
