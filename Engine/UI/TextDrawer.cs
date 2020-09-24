﻿using SharpDX;
using System.Collections.Generic;
using System.Linq;

namespace Engine.UI
{
    using Engine.Common;
    using Engine.Effects;

    /// <summary>
    /// Text drawer
    /// </summary>
    class TextDrawer : Drawable, IScreenFitted
    {
        /// <summary>
        /// Vertex buffer descriptor
        /// </summary>
        private readonly BufferDescriptor vertexBuffer = null;
        /// <summary>
        /// Index buffer descriptor
        /// </summary>
        private readonly BufferDescriptor indexBuffer = null;
        /// <summary>
        /// Index count
        /// </summary>
        private int indexDrawCount = 0;
        /// <summary>
        /// Vertices
        /// </summary>
        private VertexFont[] vertices;
        /// <summary>
        /// Indices
        /// </summary>
        private uint[] indices;
        /// <summary>
        /// Update buffers flag
        /// </summary>
        private bool updateBuffers = false;
        /// <summary>
        /// View * projection matrix
        /// </summary>
        private Matrix viewProjection;

        /// <summary>
        /// Font map
        /// </summary>
        private readonly FontMap fontMap = null;
        /// <summary>
        /// Base line threshold
        /// </summary>
        private readonly float baseLineThr = 0;
        /// <summary>
        /// Text
        /// </summary>
        private string text = null;
        /// <summary>
        /// Text fore color
        /// </summary>
        private Color4 textColor;
        /// <summary>
        /// Text shadow color
        /// </summary>
        private Color4 shadowColor;

        /// <summary>
        /// Parent control
        /// </summary>
        private UIControl parent = null;
        /// <summary>
        /// Horizontal align
        /// </summary>
        private HorizontalTextAlign horizontalAlign = HorizontalTextAlign.Left;
        /// <summary>
        /// Vertical align
        /// </summary>
        private VerticalTextAlign verticalAlign = VerticalTextAlign.Middle;
        /// <summary>
        /// Update internals flag
        /// </summary>
        private bool updateInternals = false;

        /// <summary>
        /// Manipulator
        /// </summary>
        protected Manipulator2D Manipulator { get; private set; }
        /// <summary>
        /// Shadow manipulator
        /// </summary>
        protected Manipulator2D ShadowManipulator { get; private set; }

        /// <summary>
        /// Font name
        /// </summary>
        public readonly string Font = null;
        /// <summary>
        /// Parent control
        /// </summary>
        public UIControl Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != value)
                {
                    parent = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets text to draw
        /// </summary>
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (!string.Equals(text, value))
                {
                    text = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the horizontal align
        /// </summary>
        public HorizontalTextAlign HorizontalAlign
        {
            get
            {
                return horizontalAlign;
            }
            set
            {
                if (horizontalAlign != value)
                {
                    horizontalAlign = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets the vertical align
        /// </summary>
        public VerticalTextAlign VerticalAlign
        {
            get
            {
                return verticalAlign;
            }
            set
            {
                if (verticalAlign != value)
                {
                    verticalAlign = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Use the texure color flag
        /// </summary>
        public bool UseTextureColor { get; set; } = false;
        /// <summary>
        /// Gets or sets text fore color
        /// </summary>
        public Color4 TextColor
        {
            get
            {
                return textColor;
            }
            set
            {
                if (textColor != value)
                {
                    textColor = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Gets or sets text shadow color
        /// </summary>
        public Color4 ShadowColor
        {
            get
            {
                return shadowColor;
            }
            set
            {
                if (shadowColor != value)
                {
                    shadowColor = value;

                    updateInternals = true;
                }
            }
        }
        /// <summary>
        /// Alpha color component
        /// </summary>
        public float Alpha { get; set; } = 1f;
        /// <summary>
        /// Gets or sets relative position of shadow
        /// </summary>
        public Vector2 ShadowDelta { get; set; }
        /// <summary>
        /// Gets or sets the text color alpha multiplier
        /// </summary>
        public float AlphaMultplier { get; set; } = 1.2f;
        /// <summary>
        /// Gets whether the internal buffers were ready or not
        /// </summary>
        public bool BuffersReady
        {
            get
            {
                return vertexBuffer?.Ready == true && indexBuffer?.Ready == true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene">Scene</param>
        /// <param name="description">Text description</param>
        public TextDrawer(Scene scene, TextDrawerDescription description)
            : base(scene, description)
        {
            Manipulator = new Manipulator2D(Game);
            ShadowManipulator = new Manipulator2D(Game);

            Font = string.Format("{0} {1}", description.FontFamily, description.FontSize);

            viewProjection = Game.Form.GetOrthoProjectionMatrix();

            if (!string.IsNullOrWhiteSpace(description.FontFileName) && !string.IsNullOrWhiteSpace(description.ContentPath))
            {
                fontMap = FontMap.FromFile(Game, description.ContentPath, description.FontFileName, description.FontSize, description.Style);
            }
            else if (description.FontMapping != null)
            {
                fontMap = FontMap.FromMap(Game, description.ContentPath, description.FontMapping);
            }
            else if (!string.IsNullOrWhiteSpace(description.FontFamily))
            {
                fontMap = FontMap.FromFamily(Game, description.FontFamily, description.FontSize, description.Style);
            }

            VertexFont[] verts = new VertexFont[FontMap.MAXTEXTLENGTH * 4];
            uint[] idx = new uint[FontMap.MAXTEXTLENGTH * 6];

            vertexBuffer = BufferManager.AddVertexData(description.Name, true, verts);
            indexBuffer = BufferManager.AddIndexData(description.Name, true, idx);

            TextColor = description.TextColor;
            UseTextureColor = description.UseTextureColor;
            ShadowColor = description.ShadowColor;
            ShadowDelta = description.ShadowDelta;
            horizontalAlign = description.HorizontalAlign;
            verticalAlign = description.VerticalAlign;

            if (description.LineAdjust)
            {
                string sampleChar = $"{fontMap.GetSampleCharacter()}";
                RectangleF rect = Game.Form.RenderRectangle;

                // Set base line threshold
                var size = MeasureText(sampleChar, rect, HorizontalTextAlign.Left, VerticalTextAlign.Top);

                baseLineThr = size.Y * 0.1666f; // --> 0.3333f * 0.5f
            }
        }
        /// <summary>
        /// Destructor
        /// </summary>
        ~TextDrawer()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Remove data from buffer manager
                BufferManager?.RemoveVertexData(vertexBuffer);
                BufferManager?.RemoveIndexData(indexBuffer);
            }
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            if (updateInternals)
            {
                MapText();

                updateInternals = false;
            }

            Vector2 sca;
            Vector2 pos;
            float rot;
            Vector2 parentCenter;
            float parentScale;

            if (parent != null)
            {
                sca = Vector2.One * parent.AbsoluteScale;
                pos = new Vector2(parent.AbsoluteLeft, parent.AbsoluteTop);
                rot = parent.AbsoluteRotation;
                parentCenter = parent.GrandpaRectangle.Center;
                parentScale = parent.GrandpaScale;
            }
            else
            {
                sca = Vector2.One;
                pos = Vector2.Zero;
                rot = 0f;
                parentCenter = Vector2.Zero;
                parentScale = 1f;
            }

            // Adjust position
            var rect = GetRenderArea();
            pos.X = rect.X;
            pos.Y = rect.Y + baseLineThr;

            // Calculate new transforms
            Manipulator.SetScale(sca);
            Manipulator.SetRotation(rot);
            Manipulator.SetPosition(pos);
            Manipulator.Update(parentCenter, parentScale);

            if (ShadowColor != Color.Transparent)
            {
                ShadowManipulator.SetScale(sca);
                ShadowManipulator.SetRotation(rot);
                ShadowManipulator.SetPosition(pos + ShadowDelta);
                ShadowManipulator.Update(parentCenter, parentScale);
            }
        }

        /// <inheritdoc/>
        public override void Draw(DrawContext context)
        {
            if (!Visible)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (fontMap == null)
            {
                return;
            }

            if (!BuffersReady)
            {
                return;
            }

            bool draw = context.ValidateDraw(BlendMode, true);
            if (!draw)
            {
                return;
            }

            WriteBuffers();

            BufferManager.SetIndexBuffer(indexBuffer);

            var effect = DrawerPool.EffectDefaultFont;
            var technique = effect.FontDrawer;

            BufferManager.SetInputAssembler(technique, vertexBuffer, Topology.TriangleList);

            if (ShadowColor != Color.Transparent)
            {
                //Draw with shadows
                int offset = indexDrawCount / 2;
                int count = indexDrawCount / 2;
                DrawText(effect, technique, ShadowManipulator.LocalTransform, UseTextureColor, offset, count);
                DrawText(effect, technique, Manipulator.LocalTransform, UseTextureColor, 0, count);
            }
            else
            {
                //Draw fore color only
                DrawText(effect, technique, Manipulator.LocalTransform, UseTextureColor, 0, indexDrawCount);
            }
        }
        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="effect">Effect</param>
        /// <param name="technique">Technique</param>
        /// <param name="local">Local transform</param>
        /// <param name="useTextureColor">Use the texture color</param>
        private void DrawText(EffectDefaultFont effect, EngineEffectTechnique technique, Matrix local, bool useTextureColor, int index, int count)
        {
            effect.UpdatePerFrame(
                local,
                viewProjection,
                Alpha * AlphaMultplier,
                useTextureColor,
                fontMap.Texture);

            var graphics = Game.Graphics;

            for (int p = 0; p < technique.PassCount; p++)
            {
                graphics.EffectPassApply(technique, p, 0);

                graphics.DrawIndexed(count, indexBuffer.BufferOffset + index, vertexBuffer.BufferOffset);
            }
        }
        /// <summary>
        /// Writes text data into buffers
        /// </summary>
        private void WriteBuffers()
        {
            if (!updateBuffers)
            {
                return;
            }

            bool vertsWrited = BufferManager.WriteVertexBuffer(vertexBuffer, vertices);
            bool idxWrited = BufferManager.WriteIndexBuffer(indexBuffer, indices);
            if (!vertsWrited || !idxWrited)
            {
                return;
            }

            indexDrawCount = indices?.Length ?? 0;

            updateBuffers = false;
        }

        /// <summary>
        /// Resize
        /// </summary>
        public void Resize()
        {
            viewProjection = Game.Form.GetOrthoProjectionMatrix();

            updateInternals = true;
        }

        /// <summary>
        /// Map text
        /// </summary>
        private void MapText()
        {
            if (fontMap == null)
            {
                return;
            }

            List<VertexFont> vList = new List<VertexFont>();
            List<uint> iList = new List<uint>();

            fontMap.MapSentence(
                text,
                TextColor,
                ShadowColor,
                false,
                GetRenderArea(),
                horizontalAlign,
                verticalAlign,
                out var verticesC, out var indicesC, out _);

            iList.AddRange(indicesC);
            vList.AddRange(verticesC);

            if (ShadowColor != Color.Transparent)
            {
                fontMap.MapSentence(
                    text,
                    TextColor,
                    ShadowColor,
                    true,
                    GetRenderArea(),
                    horizontalAlign,
                    verticalAlign,
                    out var verticesSM, out var indicesSM, out _);

                indicesSM.ToList().ForEach((i) => { iList.Add(i + (uint)vList.Count); });
                vList.AddRange(verticesSM);
            }

            vertices = vList.ToArray();
            indices = iList.ToArray();

            updateBuffers = true;
        }
        /// <summary>
        /// Gets the text render area
        /// </summary>
        /// <returns>Returns the text render area</returns>
        private RectangleF GetRenderArea()
        {
            return parent?.GetRenderArea() ?? Game.Form.RenderRectangle;
        }

        /// <summary>
        /// Measures the specified text
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="rect">Drawing rectangle</param>
        /// <param name="horizontalAlign">Horizontal align</param>
        /// <param name="verticalAlign">Vertical align</param>
        /// <returns>Returns a size vector where X is the width, and Y is the height</returns>
        public Vector2 MeasureText(string text, RectangleF rect, HorizontalTextAlign horizontalAlign, VerticalTextAlign verticalAlign)
        {
            if (fontMap == null)
            {
                return Vector2.Zero;
            }

            fontMap.MapSentence(
                text,
                Color.Transparent,
                Color.Transparent,
                false,
                rect,
                horizontalAlign,
                verticalAlign,
                out _, out _, out Vector2 size);

            return size;
        }
        /// <summary>
        /// Gets the single line height
        /// </summary>
        public float GetLineHeight()
        {
            if (fontMap == null)
            {
                return 0;
            }

            string sampleChar = $"{fontMap.GetSampleCharacter()}";
            RectangleF rect = Game.Form.RenderRectangle;

            fontMap.MapSentence(
                sampleChar,
                Color.Transparent,
                Color.Transparent,
                false,
                rect,
                HorizontalTextAlign.Left,
                VerticalTextAlign.Top,
                out _, out _, out Vector2 size);

            return size.Y;
        }
    }
}
