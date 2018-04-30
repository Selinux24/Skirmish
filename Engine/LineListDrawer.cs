using SharpDX;
using SharpDX.Direct3D;
using System.Collections.Generic;

namespace Engine
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Line list drawer
    /// </summary>
    public class LineListDrawer : Drawable
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Primitives to draw
        /// </summary>
        private int drawCount = 0;
        /// <summary>
        /// Lines dictionary by color
        /// </summary>
        private Dictionary<Color4, List<Line3D>> dictionary = new Dictionary<Color4, List<Line3D>>();
        /// <summary>
        /// Dictionary changes flag
        /// </summary>
        private bool dictionaryChanged = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Component description</param>
        public LineListDrawer(Scene scene, LineListDrawerDescription description)
            : base(scene, description)
        {
            int count = 0;

            if (description.Triangles != null && description.Triangles.Length > 0)
            {
                var lines = Line3D.CreateWiredTriangle(description.Triangles);

                count = description.Lines.Length * 2;

                this.dictionary.Add(description.Color, new List<Line3D>(lines));
                this.dictionaryChanged = true;
            }
            else if (description.Lines != null && description.Lines.Length > 0)
            {
                count = description.Lines.Length * 2;

                this.dictionary.Add(description.Color, new List<Line3D>(description.Lines));
                this.dictionaryChanged = true;
            }
            else
            {
                count = description.Count * 2;

                this.dictionaryChanged = false;
            }

            this.InitializeBuffers(description.Name, count);
        }
        /// <summary>
        /// Internal resources disposition
        /// </summary>
        public override void Dispose()
        {
            //Remove data from buffer manager
            this.BufferManager.RemoveVertexData(this.vertexBuffer);
        }
        /// <summary>
        /// Update content
        /// </summary>
        /// <param name="context">Update context</param>
        public override void Update(UpdateContext context)
        {

        }
        /// <summary>
        /// Draw content
        /// </summary>
        /// <param name="context">Drawing context</param>
        public override void Draw(DrawContext context)
        {
            var mode = context.DrawerMode;

            if ((mode.HasFlag(DrawerModesEnum.OpaqueOnly) && !this.Description.AlphaEnabled) ||
                (mode.HasFlag(DrawerModesEnum.TransparentOnly) && this.Description.AlphaEnabled))
            {
                this.WriteDataInBuffer();

                if (this.drawCount > 0)
                {
                    var effect = DrawerPool.EffectDefaultBasic;

                    Counters.InstancesPerFrame += this.dictionary.Count;
                    Counters.PrimitivesPerFrame += this.drawCount / 2;

                    effect.UpdatePerFrameBasic(Matrix.Identity, context);
                    effect.UpdatePerObject(0, null, 0, false);

                    var technique = effect.GetTechnique(VertexTypes.PositionColor, false);
                    this.BufferManager.SetInputAssembler(technique, this.vertexBuffer.Slot, PrimitiveTopology.LineList);

                    var graphics = this.Game.Graphics;

                    if (this.Description.AlphaEnabled)
                    {
                        graphics.SetBlendDefaultAlpha();
                    }

                    for (int p = 0; p < technique.PassCount; p++)
                    {
                        graphics.EffectPassApply(technique, p, 0);

                        graphics.Draw(this.drawCount, this.vertexBuffer.Offset);
                    }
                }
            }
        }

        /// <summary>
        /// Initialize buffers
        /// </summary>
        /// <param name="name">Buffer name</param>
        /// <param name="vertexCount">Vertex count</param>
        private void InitializeBuffers(string name, int vertexCount)
        {
            this.vertexBuffer = this.BufferManager.Add(name, new VertexPositionColor[vertexCount], true, 0);
        }
        /// <summary>
        /// Set line
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="line">Line</param>
        public void SetLines(Color4 color, Line3D line)
        {
            SetLines(color, new[] { line });
        }
        /// <summary>
        /// Set line list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Line list</param>
        public void SetLines(Color4 color, Line3D[] lines)
        {
            if (lines != null && lines.Length > 0)
            {
                if (!this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Add(color, new List<Line3D>());
                }
                else
                {
                    this.dictionary[color].Clear();
                }

                this.dictionary[color].AddRange(lines);

                this.dictionaryChanged = true;
            }
            else
            {
                if (this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Remove(color);

                    this.dictionaryChanged = true;
                }
            }
        }
        /// <summary>
        /// Add line to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="line">Line</param>
        public void AddLines(Color4 color, Line3D line)
        {
            AddLines(color, new[] { line });
        }
        /// <summary>
        /// Add lines to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Line list</param>
        public void AddLines(Color4 color, Line3D[] lines)
        {
            if (!this.dictionary.ContainsKey(color))
            {
                this.dictionary.Add(color, new List<Line3D>());
            }

            this.dictionary[color].AddRange(lines);

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Set triangle list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void SetTriangles(Color4 color, Triangle[] triangles)
        {
            if (triangles != null && triangles.Length > 0)
            {
                if (!this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Add(color, new List<Line3D>());
                }
                else
                {
                    this.dictionary[color].Clear();
                }

                this.dictionary[color].AddRange(Line3D.CreateWiredTriangle(triangles));

                this.dictionaryChanged = true;
            }
            else
            {
                if (this.dictionary.ContainsKey(color))
                {
                    this.dictionary.Remove(color);

                    this.dictionaryChanged = true;
                }
            }
        }
        /// <summary>
        /// Add triangles to list
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="lines">Triangle list</param>
        public void AddTriangles(Color4 color, Triangle[] triangles)
        {
            if (!this.dictionary.ContainsKey(color))
            {
                this.dictionary.Add(color, new List<Line3D>());
            }

            this.dictionary[color].AddRange(Line3D.CreateWiredTriangle(triangles));

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Remove by color
        /// </summary>
        /// <param name="color">Color</param>
        public void Clear(Color4 color)
        {
            if (this.dictionary.ContainsKey(color))
            {
                this.dictionary.Remove(color);
            }

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Remove all
        /// </summary>
        public void Clear()
        {
            this.dictionary.Clear();

            this.dictionaryChanged = true;
        }
        /// <summary>
        /// Writes dictionary data in buffer
        /// </summary>
        public void WriteDataInBuffer()
        {
            if (this.dictionaryChanged)
            {
                List<VertexPositionColor> data = new List<VertexPositionColor>();

                foreach (Color4 color in this.dictionary.Keys)
                {
                    List<Line3D> lines = this.dictionary[color];
                    if (lines.Count > 0)
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            data.Add(new VertexPositionColor() { Position = lines[i].Point1, Color = color });
                            data.Add(new VertexPositionColor() { Position = lines[i].Point2, Color = color });
                        }
                    }
                }

                this.drawCount = data.Count;

                if (data.Count > 0)
                {
                    this.BufferManager.WriteBuffer(this.vertexBuffer.Slot, this.vertexBuffer.Offset, data.ToArray());
                }

                this.dictionaryChanged = false;
            }
        }
    }
}
