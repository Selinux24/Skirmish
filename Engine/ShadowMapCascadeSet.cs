using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine
{
    /// <summary>
    /// Cascade shadow map matrix set
    /// </summary>
    public class ShadowMapCascadeSet
    {
        /// <summary>
        /// Total cascades
        /// </summary>
        public readonly int TotalCascades;

        private readonly int shadowMapSize;
        private readonly float cascadeTotalRange;
        private readonly float[] cascadeRanges;

        private float shadowBoundRadius = 0;
        private readonly Vector3[] cascadeBoundCenter;
        private readonly float[] cascadeBoundRadius;

        private Vector3 lightPosition = Vector3.Zero;
        private Matrix worldToShadowSpace = Matrix.Identity;
        private readonly Matrix[] worldToCascadeProj;

        private Vector4 toCascadeOffsetX;
        private Vector4 toCascadeOffsetY;
        private Vector4 toCascadeScale;

        /// <summary>
        /// Gets or sets the anti-flicker flag
        /// </summary>
        public bool AntiFlicker { get; set; } = false;

        /// <summary>
        /// Extract the frustum corners for the given near and far values
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="near">Near cascade clip plane distance</param>
        /// <param name="far">Far cascade clip plane distance</param>
        /// <param name="frustumCorners">Resulting frustum corners</param>
        private static void ExtractFrustumPoints(Camera camera, float near, float far, out Vector3[] frustumCorners)
        {
            // Get the camera bases
            var camPos = camera.Position;
            var camRight = camera.Right;
            var camUp = camera.Up;
            var camForward = camera.Direction;

            // Calculate the tangent values (this can be cached
            float tanFOVX = (float)Math.Tan(camera.AspectRelation * camera.FieldOfView);
            float tanFOVY = (float)Math.Tan(camera.AspectRelation);

            frustumCorners = new Vector3[8];

            // Calculate the points on the near plane
            frustumCorners[0] = camPos + (-camRight * tanFOVX + camUp * tanFOVY + camForward) * near;
            frustumCorners[1] = camPos + (camRight * tanFOVX + camUp * tanFOVY + camForward) * near;
            frustumCorners[2] = camPos + (camRight * tanFOVX - camUp * tanFOVY + camForward) * near;
            frustumCorners[3] = camPos + (-camRight * tanFOVX - camUp * tanFOVY + camForward) * near;

            // Calculate the points on the far plane
            frustumCorners[4] = camPos + (-camRight * tanFOVX + camUp * tanFOVY + camForward) * far;
            frustumCorners[5] = camPos + (camRight * tanFOVX + camUp * tanFOVY + camForward) * far;
            frustumCorners[6] = camPos + (camRight * tanFOVX - camUp * tanFOVY + camForward) * far;
            frustumCorners[7] = camPos + (-camRight * tanFOVX - camUp * tanFOVY + camForward) * far;
        }
        /// <summary>
        /// Extract the frustum bounding sphere for the given near and far values
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="near">Near cascade clip plane distance</param>
        /// <param name="far">Far cascade clip plane distance</param>
        /// <param name="boundingSphere">Resulting bounding sphere</param>
        private static void ExtractFrustumBoundSphere(Camera camera, float near, float far, out BoundingSphere boundingSphere)
        {
            // Get the camera bases
            var camPos = camera.Position;
            var camRight = camera.Right;
            var camUp = camera.Up;
            var camForward = camera.Direction;

            // Calculate the tangent values (this can be cached as long as the FOV doesn't change)
            float tanFOVX = (float)Math.Tan(camera.AspectRelation * camera.FieldOfView);
            float tanFOVY = (float)Math.Tan(camera.AspectRelation);

            // The center of the sphere is in the center of the frustum
            var boundCenter = camPos + camForward * (near + 0.5f * (near + far));

            // Radius is the distance to one of the frustum far corners
            var boundSpan = camPos + (-camRight * tanFOVX + camUp * tanFOVY + camForward) * far - boundCenter;
            float boundRadius = boundSpan.Length();

            boundingSphere = new BoundingSphere(boundCenter, boundRadius);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mapSize">Shadow map size</param>
        /// <param name="nearClip">Near clipping distance</param>
        /// <param name="cascades">Cascade far clipping distances</param>
        public ShadowMapCascadeSet(int mapSize, float nearClip, float[] cascades)
        {
            shadowMapSize = mapSize;

            TotalCascades = cascades.Length;

            var ranges = new List<float>(cascades);
            ranges.Insert(0, nearClip);
            cascadeRanges = ranges.ToArray();

            cascadeTotalRange = ranges[^1];

            cascadeBoundCenter = Helper.CreateArray(TotalCascades, Vector3.Zero);
            cascadeBoundRadius = Helper.CreateArray(TotalCascades, 0.0f);
            worldToCascadeProj = Helper.CreateArray(TotalCascades, Matrix.Identity);
        }

        /// <summary>
        /// Updates the matrix set
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="lightDirection">Light direction</param>
        public void Update(Camera camera, Vector3 lightDirection)
        {
            // Find the view matrix
            lightPosition = camera.Position + camera.Direction * cascadeTotalRange * 0.5f;
            var lookAt = lightPosition + (lightDirection) * camera.FarPlaneDistance;
            var up = Vector3.Normalize(Vector3.Cross(lightDirection, Vector3.Left));
            var shadowView = Matrix.LookAtLH(lightPosition, lookAt, up);

            // Get the bounds for the shadow space
            ExtractFrustumBoundSphere(
                camera,
                cascadeRanges[0],
                cascadeRanges[^1],
                out BoundingSphere boundingSphere);

            // Expand the radius to compensate for numerical errors
            shadowBoundRadius = Math.Max(shadowBoundRadius, boundingSphere.Radius);

            // Find the projection matrix
            var shadowProj = Matrix.OrthoLH(
                shadowBoundRadius,
                shadowBoundRadius,
                -shadowBoundRadius,
                shadowBoundRadius);

            // The combined transformation from world to shadow space
            worldToShadowSpace = shadowView * shadowProj;

            toCascadeOffsetX = Vector4.Zero;
            toCascadeOffsetY = Vector4.Zero;
            toCascadeScale = Vector4.Zero;

            if (AntiFlicker)
            {
                UpdateAntiFlicker(camera, shadowView);
            }
            else
            {
                UpdateSimple(camera);
            }
        }
        /// <summary>
        /// Updates the matrix set with anti flicker
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="shadowView">Shadow view</param>
        private void UpdateAntiFlicker(Camera camera, Matrix shadowView)
        {
            var shadowViewInv = Matrix.Invert(shadowView);

            // For each cascade find the transformation from shadow to cascade space
            for (int cascadeIdx = 0; cascadeIdx < TotalCascades; cascadeIdx++)
            {
                UpdateCascadesAntiFlicker(
                    camera, shadowView, shadowViewInv,
                    cascadeIdx,
                    out var cascadeTrans, out var cascadeScale);

                // Combine the matrices to get the transformation from world to cascade space
                worldToCascadeProj[cascadeIdx] = worldToShadowSpace * cascadeTrans * cascadeScale;
            }
        }
        /// <summary>
        /// Updates the matrix set with antiflikering activated
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="shadowView">Shadow view</param>
        /// <param name="shadowViewInv">Inverse shadow view</param>
        /// <param name="cascadeIdx">Cascade index</param>
        /// <param name="cascadeTrans">Resulting cascade transform</param>
        /// <param name="cascadeScale">Resulting cascade scale</param>
        /// <remarks>
        /// To avoid anti flickering we need to make the transformation invariant to camera rotation and translation.
        /// By encapsulating the cascade frustum with a sphere we achive the rotation invariance.
        /// </remarks>
        private void UpdateCascadesAntiFlicker(Camera camera, Matrix shadowView, Matrix shadowViewInv, int cascadeIdx, out Matrix cascadeTrans, out Matrix cascadeScale)
        {
            // Extract the bounding box
            ExtractFrustumBoundSphere(
                camera,
                cascadeRanges[cascadeIdx],
                cascadeRanges[cascadeIdx + 1],
                out BoundingSphere newBoundingSphere);

            // Expend the radius to compensate for numerical errors
            cascadeBoundRadius[cascadeIdx] = Math.Max(cascadeBoundRadius[cascadeIdx], newBoundingSphere.Radius);

            // Only update the cascade bounds if it moved at least a full pixel unit
            // This makes the transformation invariant to translation
            if (CascadeNeedsUpdate(shadowView, cascadeIdx, newBoundingSphere.Center, out Vector3 offset))
            {
                // To avoid flickering we need to move the bound center in full units
                var offsetOut = Vector3.TransformNormal(offset, shadowViewInv);
                cascadeBoundCenter[cascadeIdx] += offsetOut;
            }

            // Get the cascade center in shadow space
            var cascadeCenterShadowSpace = Vector3.TransformCoordinate(cascadeBoundCenter[cascadeIdx], worldToShadowSpace);

            // Update the translation from shadow to cascade space
            toCascadeOffsetX[cascadeIdx] = -cascadeCenterShadowSpace.X;
            toCascadeOffsetY[cascadeIdx] = -cascadeCenterShadowSpace.Y;
            cascadeTrans = Matrix.Translation(toCascadeOffsetX[cascadeIdx], toCascadeOffsetY[cascadeIdx], 0.0f);

            // Update the scale from shadow to cascade space
            toCascadeScale[cascadeIdx] = shadowBoundRadius / cascadeBoundRadius[cascadeIdx];
            cascadeScale = Matrix.Scaling(toCascadeScale[cascadeIdx], toCascadeScale[cascadeIdx], 1.0f);
        }
        /// <summary>
        /// Updates the matrix set, the cheaper option
        /// </summary>
        /// <param name="camera">Camera</param>
        private void UpdateSimple(Camera camera)
        {
            for (int cascadeIdx = 0; cascadeIdx < TotalCascades; cascadeIdx++)
            {
                UpdateCascadesSimple(
                    camera,
                    cascadeIdx,
                    out var cascadeTrans, out var cascadeScale);

                // Combine the matrices to get the transformation from world to cascade space
                worldToCascadeProj[cascadeIdx] = worldToShadowSpace * cascadeTrans * cascadeScale;
            }
        }
        /// <summary>
        /// Updates the matrix set with antiflikering deactivated
        /// </summary>
        /// <param name="camera">Camera</param>
        /// <param name="cascadeIdx">Cascade index</param>
        /// <param name="cascadeTrans">Resulting cascade transform</param>
        /// <param name="cascadeScale">Resulting cascade scale</param>
        /// <remarks>
        /// Since we don't care about flickering we can make the cascade fit tightly around the frustum
        /// </remarks>
        private void UpdateCascadesSimple(Camera camera, int cascadeIdx, out Matrix cascadeTrans, out Matrix cascadeScale)
        {
            // Extract the bounding box
            ExtractFrustumPoints(
                camera,
                cascadeRanges[cascadeIdx],
                cascadeRanges[cascadeIdx + 1],
                out Vector3[] frustumPoints);

            // Transform to shadow space and extract the minimum and maximum
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);
            for (int i = 0; i < frustumPoints.Length; i++)
            {
                var pointInShadowSpace = Vector3.TransformCoordinate(frustumPoints[i], worldToShadowSpace);

                for (int j = 0; j < 3; j++)
                {
                    if (min[j] > pointInShadowSpace[j])
                    {
                        min[j] = pointInShadowSpace[j];
                    }
                    if (max[j] < pointInShadowSpace[j])
                    {
                        max[j] = pointInShadowSpace[j];
                    }
                }
            }

            var cascadeCenterShadowSpace = 0.5f * (min + max);

            // Update the translation from shadow to cascade space
            toCascadeOffsetX[cascadeIdx] = -cascadeCenterShadowSpace.X;
            toCascadeOffsetY[cascadeIdx] = -cascadeCenterShadowSpace.Y;
            cascadeTrans = Matrix.Translation(toCascadeOffsetX[cascadeIdx], toCascadeOffsetY[cascadeIdx], 0.0f);

            // Update the scale from shadow to cascade space
            toCascadeScale[cascadeIdx] = 2.0f / Math.Max(max.X - min.X, max.Y - min.Y);
            cascadeScale = Matrix.Scaling(toCascadeScale[cascadeIdx], toCascadeScale[cascadeIdx], 1.0f);
        }

        /// <summary>
        /// Test if a cascade needs an update
        /// </summary>
        /// <param name="shadowView">Shadow view matrix</param>
        /// <param name="cascadeIdx">Cascade index</param>
        /// <param name="newCenter">New bounding sphere center</param>
        /// <param name="offset">Resulting offset</param>
        /// <returns>Returns true if the cascade needs update</returns>
        private bool CascadeNeedsUpdate(Matrix shadowView, int cascadeIdx, Vector3 newCenter, out Vector3 offset)
        {
            offset = Vector3.Zero;

            // Find the offset between the new and old bound ceter
            var oldCenterInCascade = Vector3.TransformCoordinate(cascadeBoundCenter[cascadeIdx], shadowView);
            var newCenterInCascade = Vector3.TransformCoordinate(newCenter, shadowView);
            var centerDiff = newCenterInCascade - oldCenterInCascade;

            // Find the pixel size based on the diameters and map pixel size
            float pixelSize = shadowMapSize / (2.0f * cascadeBoundRadius[cascadeIdx]);

            float pixelOffX = centerDiff.X * pixelSize;
            float pixelOffY = centerDiff.Y * pixelSize;

            // Check if the center moved at least half a pixel unit
            bool needUpdate = Math.Abs(pixelOffX) > 0.5f || Math.Abs(pixelOffY) > 0.5f;
            if (needUpdate)
            {
                // Round to the 
                offset.X = (float)Math.Floor(0.5f + pixelOffX) / pixelSize;
                offset.Y = (float)Math.Floor(0.5f + pixelOffY) / pixelSize;
                offset.Z = centerDiff.Z;
            }

            return needUpdate;
        }
        /// <summary>
        /// Gets the world to shadow space matrix
        /// </summary>
        /// <returns>Returns the world to shadow space matrix</returns>
        public Matrix GetWorldToShadowSpace()
        {
            return worldToShadowSpace;
        }
        /// <summary>
        /// Gets the world to cascade projection matrix
        /// </summary>
        /// <returns>Returns the world to cascade projection matrix</returns>
        public Matrix[] GetWorldToCascadeProj()
        {
            return worldToCascadeProj;
        }
        /// <summary>
        /// Gets the cascade X offset
        /// </summary>
        /// <returns>Returns the cascade X offset</returns>
        public Vector4 GetToCascadeOffsetX()
        {
            return toCascadeOffsetX;
        }
        /// <summary>
        /// Gets the cascade Y offset
        /// </summary>
        /// <returns>Returns the cascade Y offset</returns>
        public Vector4 GetToCascadeOffsetY()
        {
            return toCascadeOffsetY;
        }
        /// <summary>
        /// Gets the cascade scale
        /// </summary>
        /// <returns>Returns the cascade scale</returns>
        public Vector4 GetToCascadeScale()
        {
            return toCascadeScale;
        }
        /// <summary>
        /// Gets the light position used in the view matrix calculation
        /// </summary>
        /// <returns>Returns the light position</returns>
        public Vector3 GetLigthPosition()
        {
            return lightPosition;
        }
    }
}
