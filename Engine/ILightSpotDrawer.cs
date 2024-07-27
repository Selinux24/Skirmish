﻿using Engine.Common;

namespace Engine
{
    /// <summary>
    /// Light spot drawer interface
    /// </summary>
    public interface ILightSpotDrawer : IDrawer
    {
        /// <summary>
        /// Updates the geometry map
        /// </summary>
        /// <param name="geometryMap">Geometry map</param>
        void UpdateGeometryMap(EngineShaderResourceView[] geometryMap);
        /// <summary>
        /// Updates per light buffer
        /// </summary>
        /// <param name="dc">Device context</param>
        /// <param name="light">Light constant buffer</param>
        void UpdatePerLight(IEngineDeviceContext dc, ISceneLightSpot light);
    }
}
