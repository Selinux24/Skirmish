﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.Effects
{
    /// <summary>
    /// Pool of drawers
    /// </summary>
    static class DrawerPool
    {
        /// <summary>
        /// Graphics
        /// </summary>
        private static Graphics graphics;
        /// <summary>
        /// Effect list
        /// </summary>
        private static readonly List<IDrawer> effects = new List<IDrawer>();

        /// <summary>
        /// Initializes pool
        /// </summary>
        /// <param name="graphics">Device</param>
        public static void Initialize(Graphics graphics)
        {
            DrawerPool.graphics = graphics;
        }
        /// <summary>
        /// Dispose of used resources
        /// </summary>
        public static void DisposeResources()
        {
            effects.ForEach(ef => ef?.Dispose());
            effects.Clear();
        }

        /// <summary>
        /// Creates a new effect from resources
        /// </summary>
        /// <typeparam name="T">Effect type</typeparam>
        /// <param name="graphics">Graphics device</param>
        /// <param name="resCso">Compiled resource</param>
        /// <param name="resFx">Source code resource</param>
        /// <returns>Returns the new generated effect instance</returns>
        private static T CreateEffect<T>() where T : Drawer
        {
            var effect = (T)Activator.CreateInstance(typeof(T), graphics);

            effect.Optimize();

            return effect;
        }
        /// <summary>
        /// Gets or create an effect
        /// </summary>
        /// <typeparam name="T">Type of effect</typeparam>
        public static T GetEffect<T>() where T : Drawer
        {
            var ef = effects.OfType<T>().FirstOrDefault();
            if (ef != null)
            {
                return ef;
            }

            ef = CreateEffect<T>();
            effects.Add(ef);

            return ef;
        }
    }
}
