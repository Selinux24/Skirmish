using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Engine.UI
{
    /// <summary>
    /// Grid layout helper
    /// </summary>
    public struct GridLayout
    {
        public static GridLayout Default
        {
            get
            {
                return new GridLayout
                {
                    FitType = GridFitTypes.None,
                };
            }
        }
        public static GridLayout Uniform
        {
            get
            {
                return new GridLayout
                {
                    Spacing = Spacing.Zero,
                    Padding = Padding.Zero,
                    FitType = GridFitTypes.Uniform,
                    FitX = true,
                    FitY = true,
                };
            }
        }
        public static GridLayout Width
        {
            get
            {
                return new GridLayout
                {
                    Spacing = Spacing.Zero,
                    Padding = Padding.Zero,
                    FitType = GridFitTypes.Width,
                    FitX = true,
                    FitY = true,
                };
            }
        }
        public static GridLayout Height
        {
            get
            {
                return new GridLayout
                {
                    Spacing = Spacing.Zero,
                    Padding = Padding.Zero,
                    FitType = GridFitTypes.Height,
                    FitX = true,
                    FitY = true,
                };
            }
        }
        public static GridLayout FixedCell(Vector2 cellSize)
        {
            return new GridLayout
            {
                Spacing = Spacing.Zero,
                Padding = Padding.Zero,
                FitType = GridFitTypes.Uniform,
                FitX = false,
                FitY = false,
                CellSize = cellSize,
            };
        }
        public static GridLayout FixedCellWidth(float width)
        {
            return new GridLayout
            {
                Spacing = Spacing.Zero,
                Padding = Padding.Zero,
                FitType = GridFitTypes.Uniform,
                FitX = false,
                FitY = true,
                CellSize = new Vector2(width, 0),
            };
        }
        public static GridLayout FixedCellHeight(float height)
        {
            return new GridLayout
            {
                Spacing = Spacing.Zero,
                Padding = Padding.Zero,
                FitType = GridFitTypes.Uniform,
                FitX = true,
                FitY = false,
                CellSize = new Vector2(0, height),
            };
        }
        public static GridLayout FixedRows(int rows)
        {
            return new GridLayout
            {
                Spacing = Spacing.Zero,
                Padding = Padding.Zero,
                FitType = GridFitTypes.FixedRows,
                Rows = rows,
                FitX = true,
                FitY = true,
            };
        }
        public static GridLayout FixedColumns(int columns)
        {
            return new GridLayout
            {
                Spacing = Spacing.Zero,
                Padding = Padding.Zero,
                FitType = GridFitTypes.FixedColumns,
                Columns = columns,
                FitX = true,
                FitY = true,
            };
        }

        /// <summary>
        /// Spacing
        /// </summary>
        public Spacing Spacing { get; set; }
        /// <summary>
        /// Padding
        /// </summary>
        public Padding Padding { get; set; }
        /// <summary>
        /// Fit type
        /// </summary>
        public GridFitTypes FitType { get; set; }
        /// <summary>
        /// Number of rows
        /// </summary>
        public int Rows { get; set; }
        /// <summary>
        /// Number of columns
        /// </summary>
        public int Columns { get; set; }
        /// <summary>
        /// Fixed cell size
        /// </summary>
        public Vector2 CellSize { get; set; }
        /// <summary>
        /// Fit the x component
        /// </summary>
        public bool FitX { get; set; }
        /// <summary>
        /// Fit the y component
        /// </summary>
        public bool FitY { get; set; }

        /// <summary>
        /// Updates the grid layout
        /// </summary>
        /// <param name="panel">Panel</param>
        /// <param name="controls">Control list</param>
        /// <param name="parameters">Layout parameters</param>
        public static void UpdateLayout(UIPanel panel, IEnumerable<UIControl> controls, GridLayout parameters)
        {
            if (panel == null)
            {
                return;
            }

            if (controls?.Any() != true)
            {
                return;
            }

            if (parameters.FitType == GridFitTypes.None)
            {
                return;
            }

            Spacing spacing = parameters.Spacing;
            Padding padding = parameters.Padding;
            GridFitTypes fitType = parameters.FitType;
            int rows = parameters.Rows;
            int cols = parameters.Columns;
            Vector2 cellSize = parameters.CellSize;
            bool fitX = parameters.FitX;
            bool fitY = parameters.FitY;

            if (fitType == GridFitTypes.Width || fitType == GridFitTypes.Height || fitType == GridFitTypes.Uniform)
            {
                float sqrt = (float)Math.Sqrt(controls.Count());
                rows = (int)Math.Ceiling(sqrt);
                cols = (int)Math.Ceiling(sqrt);
            }

            if (fitType == GridFitTypes.Width || fitType == GridFitTypes.FixedColumns)
            {
                rows = (int)Math.Ceiling(controls.Count() / (float)cols);
            }
            else if (fitType == GridFitTypes.Height || fitType == GridFitTypes.FixedRows)
            {
                cols = (int)Math.Ceiling(controls.Count() / (float)rows);
            }

            var rect = panel.GetRenderArea();

            float cellWidth = (rect.Width / cols) - (spacing.Horizontal / cols * (cols - 1)) - (padding.Left / cols) - (padding.Right / cols);
            float cellHeight = (rect.Height / rows) - (spacing.Vertical / rows * (rows - 1)) - (padding.Top / rows) - (padding.Bottom / rows);

            cellSize.X = fitX ? cellWidth : cellSize.X;
            cellSize.Y = fitY ? cellHeight : cellSize.Y;

            for (int i = 0; i < controls.Count(); i++)
            {
                int rowCount = i / cols;
                int columnCount = i % cols;

                var xPos = (cellSize.X * columnCount) + (spacing.Horizontal * columnCount) + padding.Left;
                var yPos = cellSize.Y * rowCount + (spacing.Vertical * rowCount) + padding.Top;

                var item = controls.ElementAt(i);
                item.SetPosition(xPos, yPos);
                item.Width = cellSize.X;
                item.Height = cellSize.Y;
            }
        }
    }
}
