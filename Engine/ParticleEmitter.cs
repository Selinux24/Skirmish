using SharpDX;
using System;
using System.Collections.Generic;
using BindFlags = SharpDX.Direct3D11.BindFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags;
using ResourceUsage = SharpDX.Direct3D11.ResourceUsage;
using ShaderResourceView = SharpDX.Direct3D11.ShaderResourceView;
using StreamOutputBufferBinding = SharpDX.Direct3D11.StreamOutputBufferBinding;
using VertexBufferBinding = SharpDX.Direct3D11.VertexBufferBinding;

namespace Engine
{
    using Engine.Common;
    using Engine.Content;
    using Engine.Helpers;

    /// <summary>
    /// Particle emitter
    /// </summary>
    public class ParticleEmitter : IDisposable
    {
        /// <summary>
        /// Emitter initialization buffer
        /// </summary>
        private Buffer emittersBuffer;
        /// <summary>
        /// Drawing buffer
        /// </summary>
        private Buffer drawingBuffer;
        /// <summary>
        /// Stream out buffer
        /// </summary>
        private Buffer streamOutBuffer;
        /// <summary>
        /// Stride
        /// </summary>
        private int inputStride;

        /// <summary>
        /// Particle description
        /// </summary>
        public ParticleEmitterDescription Description;
        /// <summary>
        /// Seed for random numbers
        /// </summary>
        public int RandomSeed = 0;
        /// <summary>
        /// Total game time
        /// </summary>
        public float TotalTime = 0f;
        /// <summary>
        /// Elapsed game time
        /// </summary>
        public float ElapsedTime = 0f;
        /// <summary>
        /// First stream out flag
        /// </summary>
        public bool FirstRun = true;
        /// <summary>
        /// Textures
        /// </summary>
        public ShaderResourceView TextureArray;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="emitter">Emitter description</param>
        public ParticleEmitter(Game game, ParticleEmitterDescription emitter)
        {
            this.Description = emitter;

            VertexParticle[] data = Helper.CreateArray(emitter.ParticleCountMax, new VertexParticle());

            this.emittersBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(data, ResourceUsage.Default, BindFlags.VertexBuffer, CpuAccessFlags.None);
            this.drawingBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(emitter.ParticleCountMax, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.streamOutBuffer = game.Graphics.Device.CreateBuffer<VertexParticle>(emitter.ParticleCountMax, ResourceUsage.Default, BindFlags.VertexBuffer | BindFlags.StreamOutput, CpuAccessFlags.None);
            this.inputStride = default(VertexParticle).Stride;

            ImageContent imgContent = new ImageContent()
            {
                Streams = ContentManager.FindContent(emitter.ContentPath, emitter.Textures),
            };

            this.TextureArray = game.ResourceManager.CreateResource(imgContent);
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Helper.Dispose(this.emittersBuffer);
            Helper.Dispose(this.drawingBuffer);
            Helper.Dispose(this.streamOutBuffer);
        }
        /// <summary>
        /// Updates the emitter state
        /// </summary>
        /// <param name="gameTime">Game time</param>
        public void Update(GameTime gameTime)
        {
            this.ElapsedTime = gameTime.ElapsedSeconds;
            this.TotalTime += gameTime.ElapsedSeconds;
        }

        /// <summary>
        /// Prepare graphics for stream out
        /// </summary>
        /// <param name="game">Game instance</param>
        public void PrepareStreamOut(Game game)
        {
            var iaBinding = new VertexBufferBinding(this.FirstRun ? this.emittersBuffer : this.drawingBuffer, this.inputStride, 0);
            var soBinding = new StreamOutputBufferBinding(this.streamOutBuffer, 0);

            game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
            Counters.IAVertexBuffersSets++;
            game.Graphics.DeviceContext.StreamOutput.SetTargets(new[] { soBinding });

            game.Graphics.SetDepthStencilRDZDisabled();
        }
        /// <summary>
        /// Reset graphics after stream out
        /// </summary>
        /// <param name="game"></param>
        public void EndStreamOut(Game game)
        {
            game.Graphics.DeviceContext.StreamOutput.SetTargets(null);
        }
        /// <summary>
        /// Toggle stream out and drawing buffers
        /// </summary>
        public void ToggleBuffers()
        {
            var temp = this.drawingBuffer;
            this.drawingBuffer = this.streamOutBuffer;
            this.streamOutBuffer = temp;
        }
        /// <summary>
        /// Prepare graphics for drawing
        /// </summary>
        /// <param name="game">Game instance</param>
        public void PrepareDrawing(Game game)
        {
            var iaBinding = new VertexBufferBinding(this.drawingBuffer, this.inputStride, 0);

            game.Graphics.DeviceContext.InputAssembler.SetVertexBuffers(0, new[] { iaBinding });
            Counters.IAVertexBuffersSets++;

            game.Graphics.SetDepthStencilRDZEnabled();
        }
    }
}
