using Engine.Common;
using Engine.UI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engine.BuiltIn.UI
{
    /// <summary>
    /// Slider control
    /// </summary>
    /// <param name="scene">Scene</param>
    /// <param name="id">Control id</param>
    /// <param name="name">Control name</param>
    public class UISlider(Scene scene, string id, string name) : UIControl<UISliderDescription>(scene, id, name)
    {
        /// <summary>
        /// Sprite bars collection
        /// </summary>
        private readonly List<Sprite> bars = [];
        /// <summary>
        /// Sprite selectors collection
        /// </summary>
        private readonly List<Sprite> selectors = [];
        /// <summary>
        /// Selector current values
        /// </summary>
        private readonly List<float> values = [];
        /// <summary>
        /// Selected selector
        /// </summary>
        private Sprite currentSelector = null;

        /// <summary>
        /// Minimum value
        /// </summary>
        public float Minimum { get; set; } = 0f;
        /// <summary>
        /// Maximum value
        /// </summary>
        public float Maximum { get; set; } = 1f;
        /// <summary>
        /// Value step
        /// </summary>
        public float Step { get; set; } = 0.1f;
        /// <summary>
        /// Gets or sets the selector ranges
        /// </summary>
        public float[] Ranges { get; set; }
        /// <summary>
        /// Value changed event
        /// </summary>
        public Action<int, float> OnValueChanged { get; set; }

        /// <summary>
        /// Expands the value to the range of Minimum and Maximum properties
        /// </summary>
        /// <param name="value">Value</param>
        private float ExpandValue(float value)
        {
            // Interpolate to a min/max range
            float realValue = value * (Maximum - Minimum) + Minimum;

            // Step the value
            realValue = MathF.Round(realValue / Step) * Step;

            return realValue;
        }
        /// <summary>
        /// Collpas the value to a range from 0 to 1 based on Minimum and Maximum properties
        /// </summary>
        /// <param name="value">Value</param>
        private float CollapseValue(float value)
        {
            // Step the value
            float convertedValue = MathF.Round(value / Step) * Step;

            // Convert value to a range from 0 to 1 based on Minimum and Maximum properties
            return (convertedValue - Minimum) / (Maximum - Minimum);
        }
        /// <summary>
        /// Gets the selector values by selector index
        /// </summary>
        /// <param name="index">Selector index</param>
        public float GetValue(int index)
        {
            return ExpandValue(values[index]);
        }
        /// <summary>
        /// Sets the selector value by index
        /// </summary>
        /// <param name="index">Selector index</param>
        /// <param name="value">Value</param>
        public void SetValue(int index, float value)
        {
            values[index] = CollapseValue(value);

            OnValueChanged?.Invoke(index, value);
        }

        /// <inheritdoc/>
        public override async Task ReadAssets(UISliderDescription description)
        {
            await base.ReadAssets(description);

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(description.RangeCount);
            ArgumentOutOfRangeException.ThrowIfLessThan(description.SelectorInitialValues?.Length ?? 0, description.RangeCount);
            ArgumentOutOfRangeException.ThrowIfLessThan(description.SelectorColors?.Length ?? 0, description.RangeCount);
            ArgumentOutOfRangeException.ThrowIfLessThan(description.BarRanges?.Length ?? 0, description.RangeCount);
            ArgumentOutOfRangeException.ThrowIfLessThan((description.BarColors?.Length ?? 0) + 1, description.RangeCount);

            Minimum = description.Minimum;
            Maximum = description.Maximum;
            Step = description.Step;

            Ranges = description.BarRanges;
            values.AddRange(description.SelectorInitialValues);

            for (int i = 0; i < description.RangeCount + 1; i++)
            {
                bars.Add(await CreateBarSprite(i, description.BarColors[i], description.Height));
            }

            for (int i = 0; i < description.RangeCount; i++)
            {
                selectors.Add(await CreateSelectorSprite(i, description.SelectorColors[i], description.SelectorWidth, description.SelectorHeight));
            }

            AddChildren(bars, false);
            AddChildren(selectors, false);
        }
        private async Task<Sprite> CreateBarSprite(int index, Color4 barColor, float height)
        {
            var desc = SpriteDescription.Default(barColor);
            desc.EventsEnabled = true;
            desc.Height = height;

            string barName = $"{Id}.Bar_{index}";

            var bar = await Scene.CreateComponent<Sprite, SpriteDescription>(barName, barName, desc);
            bar.MouseJustPressed += BarJustPressed;

            return bar;
        }
        private void BarJustPressed(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (currentSelector != null)
            {
                return;
            }

            if (sender is not Sprite sprite)
            {
                return;
            }

            UpdateValue(sprite);
        }
        private async Task<Sprite> CreateSelectorSprite(int index, Color4 selectorColor, float width, float height)
        {
            var desc = SpriteDescription.Default(selectorColor);
            desc.EventsEnabled = true;
            desc.Width = width;
            desc.Height = height;

            string selectorName = $"{Id}.Selector_{index}";

            var selector = await Scene.CreateComponent<Sprite, SpriteDescription>(selectorName, selectorName, desc);
            selector.MousePressed += SelectorPressed;
            selector.MouseJustReleased += SelectorReleased;

            return selector;
        }
        private void SelectorPressed(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            if (currentSelector != null)
            {
                return;
            }

            if (sender is not Sprite sprite)
            {
                return;
            }

            currentSelector = sprite;
        }
        private void SelectorReleased(IUIControl sender, MouseEventArgs e)
        {
            if (!e.Buttons.HasFlag(MouseButtons.Left))
            {
                return;
            }

            currentSelector = null;
        }

        /// <inheritdoc/>
        public override void Update(UpdateContext context)
        {
            base.Update(context);

            if (!Active)
            {
                return;
            }

            if (currentSelector != null)
            {
                UpdateValue(currentSelector);
            }

            UpdateSelector();
        }
        private void UpdateValue(Sprite sprite)
        {
            if (sprite is null)
            {
                return;
            }

            //Get the range index
            int index = GetRangeIndex(sprite);

            //Get range values of the index
            float min = index == 0 ? 0f : Ranges[index - 1];
            float max = Ranges[index];

            //Get the bounds in control space
            float left = Width * min;
            float right = Width * max;

            //Get the percentage of the mouse position
            float xmouse = MathUtil.Clamp(Game.Input.MouseX - Left, left, right);
            float value = xmouse / Width;

            //Set the range value
            values[index] = value;

            OnValueChanged?.Invoke(index, ExpandValue(value));
        }
        private int GetRangeIndex(Sprite sprite)
        {
            int index = selectors.IndexOf(sprite);
            if (index >= 0)
            {
                return index;
            }

            index = bars.IndexOf(sprite);
            if (index < 0)
            {
                return index;
            }

            if (index == bars.Count - 1)
            {
                index--;
            }

            return index;
        }
        private void UpdateSelector()
        {
            //Set selector positions and bar widths
            for (int i = 0; i < selectors.Count; i++)
            {
                float p = Width * values[i];
                selectors[i].Left = p - selectors[i].Width * .5f;
                selectors[i].Top = (Height - selectors[i].Height) * .5f;

                bars[i].Left = i == 0 ? 0 : bars[i - 1].Left + bars[i - 1].Width;
                bars[i].Width = p - bars[i].Left;
            }

            bars[^1].Left = bars[^2].Left + bars[^2].Width;
            bars[^1].Width = Width - bars[^1].Left;
        }
    }
}
