using Engine;
using Engine.BuiltIn.Components.Primitives;
using Engine.Common;
using SharpDX;
using System;

namespace AISamples.Common
{
    class Visualizer(GeometryColorDrawer<Triangle> opaqueDrawer, GeometryColorDrawer<Triangle> triangleDrawer, GeometryColorDrawer<Line3D> lineDrawer)
    {
        private static readonly Color4 bColor = new(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color4 wPositiveColor = Color.Yellow;
        private static readonly Color4 wNegativeColor = Color.Blue;

        private readonly GeometryColorDrawer<Triangle> opaqueDrawer = opaqueDrawer;
        private readonly GeometryColorDrawer<Triangle> triangleDrawer = triangleDrawer;
        private readonly GeometryColorDrawer<Line3D> lineDrawer = lineDrawer;

        public void DrawNetwork(NeuralNetwork network, Vector3 position, int margin, int totalWidth, int totalHeight, float nodeRadius)
        {
            opaqueDrawer.Clear();
            triangleDrawer.Clear();
            lineDrawer.Clear();

            int bgMargin = (int)(margin * 0.9f);

            int left = bgMargin + (int)position.X;
            int top = bgMargin - (int)position.Y;
            int width = totalWidth - bgMargin * 2;
            int height = totalHeight - bgMargin * 2;

            DrawBackground(-left, -top, -width, -height, position.Z);

            left = margin + (int)position.X;
            top = margin - (int)position.Y;
            width = totalWidth - margin * 2;
            height = totalHeight - margin * 2;

            int levels = network.GetLevelCount();
            int levelHeight = height / levels;
            for (int i = levels - 1; i >= 0; i--)
            {
                float levelTop = top + MathUtil.Lerp(height - levelHeight, 0, GetX(i, levels));

                DrawLevel(network.GetLevel(i), nodeRadius, -left, (int)-levelTop, -width, -levelHeight, position.Z);
            }
        }

        private void DrawBackground(int left, int top, int width, int height, float depth)
        {
            Vector3 p0 = new(left, top, 1.5f + depth);
            Vector3 p1 = new(left, top + height, 1.5f + depth);
            Vector3 p2 = new(left + width, top + height, 1.5f + depth);
            Vector3 p3 = new(left + width, top, 1.5f + depth);

            var bgT = GeometryUtil.CreatePolygonTriangleList([p0, p1, p2, p3], false);
            opaqueDrawer.AddPrimitives(bColor, Triangle.ComputeTriangleList(bgT));
        }

        private void DrawLevel(Level level, float nodeRadius, int left, int top, int width, int heigth, float depth)
        {
            int right = left + width;
            int bottom = top + heigth;

            const int stacks = 32;
            float tNodeRadius = nodeRadius * 0.95f;

            int inputCount = level.GetInputCount();
            int outputCount = level.GetOutputCount();

            for (int i = 0; i < inputCount; i++)
            {
                for (int o = 0; o < outputCount; o++)
                {
                    var l = new Line3D(
                        new(MathUtil.Lerp(left, right, GetX(i, inputCount)), bottom, 1 + depth),
                        new(MathUtil.Lerp(left, right, GetX(o, outputCount)), top, 1 + depth));

                    float w = level.GetWeight(i, o);
                    lineDrawer.AddPrimitives(ColorFromValue(w), l);
                }
            }

            for (int i = 0; i < inputCount; i++)
            {
                float x = MathUtil.Lerp(left, right, GetX(i, inputCount));

                float iv = level.GetInput(i);
                var gT = GeometryUtil.CreateCircle(Topology.TriangleList, new(x, bottom, 0.5f + depth), tNodeRadius, stacks, Vector3.ForwardLH);
                triangleDrawer.AddPrimitives(ColorFromValue(iv), Triangle.ComputeTriangleList(gT));
            }

            for (int o = 0; o < outputCount; o++)
            {
                float x = MathUtil.Lerp(left, right, GetX(o, outputCount));

                float ov = level.GetOutput(o);
                var gT = GeometryUtil.CreateCircle(Topology.TriangleList, new(x, top, 0.5f + depth), tNodeRadius, stacks, Vector3.ForwardLH);
                triangleDrawer.AddPrimitives(ColorFromValue(ov), Triangle.ComputeTriangleList(gT));

                float bv = level.GetBias(o);
                var gL = GeometryUtil.CreateCircle(Topology.LineList, new(x, top, 0.5f + depth), nodeRadius, stacks, Vector3.ForwardLH);
                lineDrawer.AddPrimitives(ColorFromValue(bv), Line3D.CreateFromVertices(gL));
            }
        }

        private static float GetX(int i, int count)
        {
            return count == 1 ? 0.5f : (float)i / (count - 1);
        }
        private static Color4 ColorFromValue(float v)
        {
            float alpha = MathUtil.Clamp(MathF.Abs(v), 0f, 1f);
            Color3 color = v > 0 ? wPositiveColor.ToVector3() : wNegativeColor.ToVector3();

            return new Color4(color, alpha);
        }
    }
}
