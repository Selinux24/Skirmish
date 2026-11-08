// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Mathematics
{
    /// <summary>
    /// Defines a frustum which can be used in frustum culling, zoom to Extents (zoom to fit) operations, 
    /// (matrix, frustum, camera) interchange, and many kind of intersection testing.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    [JsonObject(MemberSerialization.OptIn)]
    public struct BoundingFrustum : IEquatable<BoundingFrustum>, IFormattable
    {
        const string BoundingFrustumStr = "Camera:{0}";

        private Matrix4x4 pMatrix;
        private Plane pNear;
        private Plane pFar;
        private Plane pLeft;
        private Plane pRight;
        private Plane pTop;
        private Plane pBottom;

        /// <summary>
        /// Gets or sets the Matrix4x4 that describes this bounding frustum.
        /// </summary>
        [JsonProperty]
        public Matrix4x4 Matrix
        {
            readonly get
            {
                return pMatrix;
            }
            set
            {
                pMatrix = value;
                GetPlanesFromMatrix(ref pMatrix, out pNear, out pFar, out pLeft, out pRight, out pTop, out pBottom);
            }
        }
        /// <summary>
        /// Gets the near plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Near => pNear;
        /// <summary>
        /// Gets the far plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Far => pFar;
        /// <summary>
        /// Gets the left plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Left => pLeft;
        /// <summary>
        /// Gets the right plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Right => pRight;
        /// <summary>
        /// Gets the top plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Top => pTop;
        /// <summary>
        /// Gets the bottom plane of the BoundingFrustum.
        /// </summary>
        public readonly Plane Bottom => pBottom;
        /// <summary>
        /// Indicate whether the current BoundingFrustrum is Orthographic.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the current BoundingFrustrum is Orthographic; otherwise, <c>false</c>.
        /// </value>
        public readonly bool IsOrthographic => (pLeft.Normal == -pRight.Normal) && (pTop.Normal == -pBottom.Normal);

        /// <summary>
        /// Creates a new instance of BoundingFrustum.
        /// </summary>
        /// <param name="matrix">Combined matrix that usually takes view × projection matrix.</param>
        public BoundingFrustum(Matrix4x4 matrix)
        {
            pMatrix = matrix;
            GetPlanesFromMatrix(ref pMatrix, out pNear, out pFar, out pLeft, out pRight, out pTop, out pBottom);
        }

        /// <summary>
        /// Returns one of the 6 planes related to this frustum.
        /// </summary>
        /// <param name="index">Plane index where 0 fro Left, 1 for Right, 2 for Top, 3 for Bottom, 4 for Near, 5 for Far</param>
        /// <returns></returns>
        public readonly Plane GetPlane(int index)
        {
            return index switch
            {
                0 => pLeft,
                1 => pRight,
                2 => pTop,
                3 => pBottom,
                4 => pNear,
                5 => pFar,
                _ => new Plane(),
            };
        }

        /// <summary>
        /// Gets the frustum planes from the specified matrix
        /// </summary>
        /// <param name="matrix">Matrix</param>
        /// <param name="near">Near plane</param>
        /// <param name="far">Far plane</param>
        /// <param name="left">Left plane</param>
        /// <param name="right">Right plane</param>
        /// <param name="top">Top plane</param>
        /// <param name="bottom">Bottom plane</param>
        private static void GetPlanesFromMatrix(ref Matrix4x4 matrix, out Plane near, out Plane far, out Plane left, out Plane right, out Plane top, out Plane bottom)
        {
            //http://www.chadvernon.com/blog/resources/directx9/frustum-culling/

            // Left plane
            left.Normal.X = matrix.M14 + matrix.M11;
            left.Normal.Y = matrix.M24 + matrix.M21;
            left.Normal.Z = matrix.M34 + matrix.M31;
            left.D = matrix.M44 + matrix.M41;
            left.Normalize();

            // Right plane
            right.Normal.X = matrix.M14 - matrix.M11;
            right.Normal.Y = matrix.M24 - matrix.M21;
            right.Normal.Z = matrix.M34 - matrix.M31;
            right.D = matrix.M44 - matrix.M41;
            right.Normalize();

            // Top plane
            top.Normal.X = matrix.M14 - matrix.M12;
            top.Normal.Y = matrix.M24 - matrix.M22;
            top.Normal.Z = matrix.M34 - matrix.M32;
            top.D = matrix.M44 - matrix.M42;
            top.Normalize();

            // Bottom plane
            bottom.Normal.X = matrix.M14 + matrix.M12;
            bottom.Normal.Y = matrix.M24 + matrix.M22;
            bottom.Normal.Z = matrix.M34 + matrix.M32;
            bottom.D = matrix.M44 + matrix.M42;
            bottom.Normalize();

            // Near plane
            near.Normal.X = matrix.M13;
            near.Normal.Y = matrix.M23;
            near.Normal.Z = matrix.M33;
            near.D = matrix.M43;
            near.Normalize();

            // Far plane
            far.Normal.X = matrix.M14 - matrix.M13;
            far.Normal.Y = matrix.M24 - matrix.M23;
            far.Normal.Z = matrix.M34 - matrix.M33;
            far.D = matrix.M44 - matrix.M43;
            far.Normalize();
        }

        /// <summary>
        /// Creates a new frustum relaying on perspective camera parameters
        /// </summary>
        /// <param name="cameraPos">The camera pos.</param>
        /// <param name="lookDir">The look dir.</param>
        /// <param name="upDir">Up dir.</param>
        /// <param name="fov">The fov.</param>
        /// <param name="znear">The znear.</param>
        /// <param name="zfar">The zfar.</param>
        /// <param name="aspect">The aspect.</param>
        /// <returns>The bounding frustum calculated from perspective camera</returns>
        public static BoundingFrustum FromCamera(Vector3 cameraPos, Vector3 lookDir, Vector3 upDir, float fov, float znear, float zfar, float aspect)
        {
            //http://knol.google.com/k/view-frustum

            lookDir = Vector3.Normalize(lookDir);
            upDir = Vector3.Normalize(upDir);

            Vector3 nearCenter = cameraPos + lookDir * znear;
            Vector3 farCenter = cameraPos + lookDir * zfar;
            float nearHalfHeight = (float)(znear * Math.Tan(fov / 2f));
            float farHalfHeight = (float)(zfar * Math.Tan(fov / 2f));
            float nearHalfWidth = nearHalfHeight * aspect;
            float farHalfWidth = farHalfHeight * aspect;

            Vector3 rightDir = Vector3.Normalize(Vector3.Cross(upDir, lookDir));
            Vector3 Near1 = nearCenter - nearHalfHeight * upDir + nearHalfWidth * rightDir;
            Vector3 Near2 = nearCenter + nearHalfHeight * upDir + nearHalfWidth * rightDir;
            Vector3 Near3 = nearCenter + nearHalfHeight * upDir - nearHalfWidth * rightDir;
            Vector3 Near4 = nearCenter - nearHalfHeight * upDir - nearHalfWidth * rightDir;
            Vector3 Far1 = farCenter - farHalfHeight * upDir + farHalfWidth * rightDir;
            Vector3 Far2 = farCenter + farHalfHeight * upDir + farHalfWidth * rightDir;
            Vector3 Far3 = farCenter + farHalfHeight * upDir - farHalfWidth * rightDir;
            Vector3 Far4 = farCenter - farHalfHeight * upDir - farHalfWidth * rightDir;

            var result = new BoundingFrustum
            {
                pNear = new Plane(Near1, Near2, Near3),
                pFar = new Plane(Far3, Far2, Far1),
                pLeft = new Plane(Near4, Near3, Far3),
                pRight = new Plane(Far1, Far2, Near2),
                pTop = new Plane(Near2, Far2, Far3),
                pBottom = new Plane(Far4, Far1, Near1)
            };

            result.pNear.Normalize();
            result.pFar.Normalize();
            result.pLeft.Normalize();
            result.pRight.Normalize();
            result.pTop.Normalize();
            result.pBottom.Normalize();

            result.pMatrix =
                Matrix4x4.LookAtLH(cameraPos, cameraPos + lookDir * 10, upDir) *
                Matrix4x4.PerspectiveFovLH(fov, aspect, znear, zfar);

            return result;
        }
        /// <summary>
        /// Creates a new frustum relaying on perspective camera parameters
        /// </summary>
        /// <param name="cameraParams">The camera params.</param>
        /// <returns>The bounding frustum from camera params</returns>
        public static BoundingFrustum FromCamera(FrustumCameraParams cameraParams)
        {
            return FromCamera(
                cameraParams.Position,
                cameraParams.LookAtDir,
                cameraParams.UpDir,
                cameraParams.FOV,
                cameraParams.ZNear,
                cameraParams.ZFar,
                cameraParams.AspectRatio);
        }

        /// <summary>
        /// Returns the 8 corners of the frustum, element0 is Near1 (near right down corner)
        /// , element1 is Near2 (near right top corner)
        /// , element2 is Near3 (near Left top corner)
        /// , element3 is Near4 (near Left down corner)
        /// , element4 is Far1 (far right down corner)
        /// , element5 is Far2 (far right top corner)
        /// , element6 is Far3 (far left top corner)
        /// , element7 is Far4 (far left down corner)
        /// </summary>
        /// <returns>The 8 corners of the frustum</returns>
        public Vector3[] GetCorners()
        {
            var corners = new Vector3[8];
            GetCorners(corners);
            return corners;
        }
        /// <summary>
        /// Returns the 8 corners of the frustum, element0 is Near1 (near right down corner)
        /// , element1 is Near2 (near right top corner)
        /// , element2 is Near3 (near Left top corner)
        /// , element3 is Near4 (near Left down corner)
        /// , element4 is Far1 (far right down corner)
        /// , element5 is Far2 (far right top corner)
        /// , element6 is Far3 (far left top corner)
        /// , element7 is Far4 (far left down corner)
        /// </summary>
        /// <returns>The 8 corners of the frustum</returns>
        public void GetCorners(Vector3[] corners)
        {
            Collision.PlanesIntersectsPoint(ref pNear, ref pBottom, ref pRight, out corners[0]);   //Near1
            Collision.PlanesIntersectsPoint(ref pNear, ref pTop, ref pRight, out corners[1]);      //Near2
            Collision.PlanesIntersectsPoint(ref pNear, ref pTop, ref pLeft, out corners[2]);       //Near3
            Collision.PlanesIntersectsPoint(ref pNear, ref pBottom, ref pLeft, out corners[3]);    //Near3
            Collision.PlanesIntersectsPoint(ref pFar, ref pBottom, ref pRight, out corners[4]);    //Far1
            Collision.PlanesIntersectsPoint(ref pFar, ref pTop, ref pRight, out corners[5]);       //Far2
            Collision.PlanesIntersectsPoint(ref pFar, ref pTop, ref pLeft, out corners[6]);        //Far3
            Collision.PlanesIntersectsPoint(ref pFar, ref pBottom, ref pLeft, out corners[7]);     //Far3
        }

        /// <summary>
        /// Extracts perspective camera parameters from the frustum, doesn't work with orthographic frustums.
        /// </summary>
        /// <returns>Perspective camera parameters from the frustum</returns>
        public FrustumCameraParams GetCameraParams()
        {
            Collision.PlanesIntersectsPoint(ref pRight, ref pTop, ref pLeft, out var position);
            var corners = GetCorners();

            return new()
            {
                Position = position,
                LookAtDir = pNear.Normal,
                UpDir = Vector3.Normalize(Vector3.Cross(pRight.Normal, pNear.Normal)),
                FOV = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal))) * 2),
                AspectRatio = (corners[6] - corners[5]).Length() / (corners[4] - corners[5]).Length(),
                ZNear = (position + (pNear.Normal * pNear.D)).Length(),
                ZFar = (position + (pFar.Normal * pFar.D)).Length()
            };
        }

        /// <summary>
        /// Get the width of the frustum at specified depth.
        /// </summary>
        /// <param name="depth">the depth at which to calculate frustum width.</param>
        /// <returns>With of the frustum at the specified depth</returns>
        public readonly float GetWidthAtDepth(float depth)
        {
            float hAngle = Helper.PiOver2 - MathF.Acos(Vector3.Dot(pNear.Normal, pLeft.Normal));
            return MathF.Tan(hAngle) * depth * 2f;
        }
        /// <summary>
        /// Get the height of the frustum at specified depth.
        /// </summary>
        /// <param name="depth">the depth at which to calculate frustum height.</param>
        /// <returns>Height of the frustum at the specified depth</returns>
        public readonly float GetHeightAtDepth(float depth)
        {
            float vAngle = Helper.PiOver2 - MathF.Acos(Vector3.Dot(pNear.Normal, pTop.Normal));
            return MathF.Tan(vAngle) * depth * 2f;
        }

        /// <summary>
        /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
        /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The zoom to fit distance</returns>
        public readonly float GetZoomToExtentsShiftDistance(ref BoundingBox boundingBox)
        {
            return GetZoomToExtentsShiftDistance(boundingBox.GetCorners());
        }
        /// <summary>
        /// Get the distance which when added to camera position along the lookat direction will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// if the returned value is positive, the camera will move toward the lookat direction (ZoomIn).
        /// if the returned value is negative, the camera will move in the reverse direction of the lookat direction (ZoomOut).
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The zoom to fit distance</returns>
        public readonly float GetZoomToExtentsShiftDistance(Vector3[] points)
        {
            float vAngle = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pTop.Normal))));
            float vSin = (float)Math.Sin(vAngle);
            float hAngle = (float)((Math.PI / 2.0 - Math.Acos(Vector3.Dot(pNear.Normal, pLeft.Normal))));
            float hSin = (float)Math.Sin(hAngle);
            float horizontalToVerticalMapping = vSin / hSin;

            var ioFrustrum = GetInsideOutClone();

            float maxPointDist = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                float pointDist = Collision.DistancePlanePoint(ref ioFrustrum.pTop, ref points[i]);
                pointDist = Math.Max(pointDist, Collision.DistancePlanePoint(ref ioFrustrum.pBottom, ref points[i]));
                pointDist = Math.Max(pointDist, Collision.DistancePlanePoint(ref ioFrustrum.pLeft, ref points[i]) * horizontalToVerticalMapping);
                pointDist = Math.Max(pointDist, Collision.DistancePlanePoint(ref ioFrustrum.pRight, ref points[i]) * horizontalToVerticalMapping);

                maxPointDist = Math.Max(maxPointDist, pointDist);
            }

            return -maxPointDist / vSin;
        }
        /// <summary>
        /// Gets the inside-out cone
        /// </summary>
        private readonly BoundingFrustum GetInsideOutClone()
        {
            var frustum = this;
            frustum.pNear.Normal = -frustum.pNear.Normal;
            frustum.pFar.Normal = -frustum.pFar.Normal;
            frustum.pLeft.Normal = -frustum.pLeft.Normal;
            frustum.pRight.Normal = -frustum.pRight.Normal;
            frustum.pTop.Normal = -frustum.pTop.Normal;
            frustum.pBottom.Normal = -frustum.pBottom.Normal;
            return frustum;
        }

        /// <summary>
        /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// </summary>
        /// <param name="boundingBox">The bounding box.</param>
        /// <returns>The zoom to fit vector</returns>
        public readonly Vector3 GetZoomToExtentsShiftVector(ref BoundingBox boundingBox)
        {
            return GetZoomToExtentsShiftDistance(boundingBox.GetCorners()) * pNear.Normal;
        }
        /// <summary>
        /// Get the vector shift which when added to camera position will do the effect of zoom to extents (zoom to fit) operation,
        /// so all the passed points will fit in the current view.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>The zoom to fit vector</returns>
        public readonly Vector3 GetZoomToExtentsShiftVector(Vector3[] points)
        {
            return GetZoomToExtentsShiftDistance(points) * pNear.Normal;
        }

        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public readonly ContainmentType Contains(ref Vector3 point)
        {
            var f = this;
            return Collision.FrustumContainsPoint(ref f, ref point);
        }
        /// <summary>
        /// Checks whether a point lay inside, intersects or lay outside the frustum.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>Type of the containment</returns>
        public readonly ContainmentType Contains(Vector3 point)
        {
            return Contains(ref point);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns>Type of the containment</returns>
        public readonly ContainmentType Contains(ref BoundingBox box)
        {
            var f = this;
            return Collision.BoxContainsFrustum(ref box, ref f);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns>Type of the containment</returns>
        public readonly ContainmentType Contains(BoundingBox box)
        {
            return Contains(ref box);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding box.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="result">Type of the containment.</param>
        public readonly void Contains(ref BoundingBox box, out ContainmentType result)
        {
            result = Contains(ref box);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public readonly ContainmentType Contains(ref BoundingSphere sphere)
        {
            var f = this;
            return Collision.SphereContainsFrustum(ref sphere, ref f);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public readonly ContainmentType Contains(BoundingSphere sphere)
        {
            return Contains(ref sphere);
        }
        /// <summary>
        /// Determines the intersection relationship between the frustum and a bounding sphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <param name="result">Type of the containment.</param>
        public readonly void Contains(ref BoundingSphere sphere, out ContainmentType result)
        {
            result = Contains(ref sphere);
        }

        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingSphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <returns>Type of the containment</returns>
        public readonly bool Intersects(ref BoundingSphere sphere)
        {
            return Contains(ref sphere) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingSphere.
        /// </summary>
        /// <param name="sphere">The sphere.</param>
        /// <param name="result">Set to <c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</param>
        public readonly void Intersects(ref BoundingSphere sphere, out bool result)
        {
            result = Contains(ref sphere) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</returns>
        public readonly bool Intersects(ref BoundingBox box)
        {
            return Contains(ref box) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects a BoundingBox.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="result"><c>true</c> if the current BoundingFrustum intersects a BoundingSphere.</param>
        public readonly void Intersects(ref BoundingBox box, out bool result)
        {
            result = Contains(ref box) != ContainmentType.Disjoint;
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <returns>Plane intersection type.</returns>
        public PlaneIntersectionType Intersects(ref Plane plane)
        {
            return Collision.PlaneIntersectsPoints(ref plane, GetCorners());
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="result">Plane intersection type.</param>
        public void Intersects(ref Plane plane, out PlaneIntersectionType result)
        {
            result = Collision.PlaneIntersectsPoints(ref plane, GetCorners());
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The ray.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public readonly bool Intersects(ref Ray ray)
        {
            return Intersects(ref ray, out float? _, out float? _);
        }
        /// <summary>
        /// Checks whether the current BoundingFrustum intersects the specified Ray.
        /// </summary>
        /// <param name="ray">The Ray to check for intersection with.</param>
        /// <param name="inDistance">The distance at which the ray enters the frustum if there is an intersection and the ray starts outside the frustum.</param>
        /// <param name="outDistance">The distance at which the ray exits the frustum if there is an intersection.</param>
        /// <returns><c>true</c> if the current BoundingFrustum intersects the specified Ray.</returns>
        public readonly bool Intersects(ref Ray ray, out float? inDistance, out float? outDistance)
        {
            var f = this;
            return Collision.RayIntersectsFrustum(ref ray, ref f, out inDistance, out outDistance);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingFrustum left, BoundingFrustum right)
        {
            return left.Equals(ref right);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingFrustum left, BoundingFrustum right)
        {
            return !left.Equals(ref right);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var camParams = GetCameraParams();

            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingFrustumStr,
                camParams.ToString());
        }
        /// <inheritdoc/>
        public string ToString(string format)
        {
            if (format == null) return ToString();

            var camParams = GetCameraParams();

            return string.Format(
                CultureInfo.CurrentCulture,
                BoundingFrustumStr,
                camParams.ToString(format, CultureInfo.CurrentCulture));
        }
        /// <inheritdoc/>
        public string ToString(IFormatProvider formatProvider)
        {
            var camParams = GetCameraParams();

            return string.Format(
                formatProvider,
                BoundingFrustumStr,
                camParams.ToString());
        }
        /// <inheritdoc/>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null) return ToString(formatProvider);

            var camParams = GetCameraParams();

            return string.Format(
                formatProvider,
                BoundingFrustumStr,
                camParams.ToString(format, formatProvider));
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return pMatrix.GetHashCode();
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ref BoundingFrustum other)
        {
            return NearEqual(this, other);
        }
        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(BoundingFrustum other)
        {
            return Equals(ref other);
        }
        /// <inheritdoc/>
        public readonly override bool Equals(object obj)
        {
            if (obj is not BoundingFrustum) return false;

            var strongValue = (BoundingFrustum)obj;
            return Equals(ref strongValue);
        }

        /// <summary>
        /// Gets whether the two BoundingFrustum structures are equal.
        /// </summary>
        /// <param name="a">The first BoundingFrustum structure to compare.</param>
        /// <param name="b">The second BoundingFrustum structure to compare.</param>
        /// <param name="epsilon">The tolerance for equality.</param>
        /// <returns>true if the two structures are equal; otherwise, false.</returns>
        public static bool NearEqual(BoundingFrustum a, BoundingFrustum b, float epsilon = Helper.ZeroTolerance)
        {
            return Matrix4x4.NearEqual(a.pMatrix, b.pMatrix, epsilon);
        }
    }
}
