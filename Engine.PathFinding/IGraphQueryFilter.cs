using System;

namespace Engine.PathFinding
{
    /// <summary>
    /// Query filter interface
    /// </summary>
    public interface IGraphQueryFilter
    {
        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        bool PassFilter(int polyFlags);
        /// <summary>
        /// Returns true if the polygon can be visited. (I.e. Is traversable.)
        /// </summary>
        /// <param name="polyFlags">Sample polygon flags.</param>
        /// <returns>Returns true if the filter pass</returns>
        bool PassFilter<T>(T polyFlags) where T : Enum;

        /// <summary>
        /// Returns the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <returns>The traversal cost of the area.</returns>
        float GetAreaCost(int i);
        /// <summary>
        /// Returns the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <returns>The traversal cost of the area.</returns>
        float GetAreaCost<T>(T i) where T : Enum;
        /// <summary>
        /// Sets the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <param name="cost">The new cost of traversing the area.</param>
        void SetAreaCost(int i, float cost);
        /// <summary>
        /// Sets the traversal cost of the area.
        /// </summary>
        /// <param name="i">The id of the area.</param>
        /// <param name="cost">The new cost of traversing the area.</param>
        void SetAreaCost<T>(T i, float cost) where T : Enum;
        /// <summary>
        /// Clears the area-costs list
        /// </summary>
        void ClearCosts();

        /// <summary>
        /// Returns the include flags for the filter.
        /// Any polygons that include one or more of these flags will be included in the operation.
        /// </summary>
        /// <returns></returns>
        int GetIncludeFlags();
        /// <summary>
        /// Returns the include flags for the filter.
        /// Any polygons that include one or more of these flags will be included in the operation.
        /// </summary>
        /// <returns></returns>
        T GetIncludeFlags<T>() where T : Enum;
        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        void SetIncludeFlags(int flags);
        /// <summary>
        /// Sets the include flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        void SetIncludeFlags<T>(T flags) where T : Enum;

        /// <summary>
        /// Returns the exclude flags for the filter.
        /// Any polygons that include one ore more of these flags will be excluded from the operation.
        /// </summary>
        /// <returns></returns>
        int GetExcludeFlags();
        /// <summary>
        /// Returns the exclude flags for the filter.
        /// Any polygons that include one ore more of these flags will be excluded from the operation.
        /// </summary>
        /// <returns></returns>
        T GetExcludeFlags<T>() where T : Enum;
        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        void SetExcludeFlags(int flags);
        /// <summary>
        /// Sets the exclude flags for the filter.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        void SetExcludeFlags<T>(T flags) where T : Enum;

        /// <summary>
        /// Gets the default walkable area type
        /// </summary>
        int GetDefaultWalkableAreaType();
        /// <summary>
        /// Gets the default walkable area type
        /// </summary>
        /// <typeparam name="T">Area type</typeparam>
        T GetDefaultWalkableAreaType<T>() where T : Enum;
        /// <summary>
        /// Evaluates de area available actions
        /// </summary>
        /// <param name="area">Area</param>
        int EvaluateArea(int area);
        /// <summary>
        /// Evaluates de area available actions
        /// </summary>
        /// <typeparam name="TArea">Area type</typeparam>
        /// <typeparam name="TAction">Action type</typeparam>
        /// <param name="area">Area</param>
        TAction EvaluateArea<TArea, TAction>(TArea area) where TArea : Enum where TAction : Enum;
    }
}
