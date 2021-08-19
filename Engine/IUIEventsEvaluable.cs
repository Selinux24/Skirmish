using System;

namespace Engine
{
    using Engine.UI;

    /// <summary>
    /// UI events evaluable interface
    /// </summary>
    public interface IUIEventsEvaluable
    {
        /// <summary>
        /// Mouse over event
        /// </summary>
        event MouseEventHandler MouseOver;
        /// <summary>
        /// Mouse enter event
        /// </summary>
        event MouseEventHandler MouseEnter;
        /// <summary>
        /// Mouse leave event
        /// </summary>
        event MouseEventHandler MouseLeave;
        /// <summary>
        /// Mouse pressed
        /// </summary>
        event MouseEventHandler MousePressed;
        /// <summary>
        /// Mouse just pressed
        /// </summary>
        event MouseEventHandler MouseJustPressed;
        /// <summary>
        /// Mouse just released
        /// </summary>
        event MouseEventHandler MouseJustReleased;
        /// <summary>
        /// Mouse click
        /// </summary>
        event MouseEventHandler MouseClick;
        /// <summary>
        /// Mouse double click
        /// </summary>
        event MouseEventHandler MouseDoubleClick;
        /// <summary>
        /// Set focus event
        /// </summary>
        event EventHandler SetFocus;
        /// <summary>
        /// Lost focus event
        /// </summary>
        event EventHandler LostFocus;

        /// <summary>
        /// Gets or sets whether the control is enabled for event processing
        /// </summary>
        bool EventsEnabled { get; set; }
        /// <summary>
        /// Gets whether the mouse is over the button rectangle or not
        /// </summary>
        bool IsMouseOver { get; }
        /// <summary>
        /// Pressed buttons state flags
        /// </summary>
        MouseButtons PressedState { get; }

        /// <summary>
        /// Sets the focus over the control
        /// </summary>
        void SetFocusControl();
        /// <summary>
        /// Lost focus over the control
        /// </summary>
        void SetFocusLost();

        /// <summary>
        /// Gets whether the specified UI control is event-evaluable or not
        /// </summary>
        /// <returns>Returns true if the control is evaluable for UI events</returns>
        bool IsEvaluable();
        /// <summary>
        /// Initializes the UI state
        /// </summary>
        void InitControlState();
        /// <summary>
        /// Evaluates input over the specified scene control
        /// </summary>
        /// <param name="topMostControl">Returns the last events enabled control in the control hierarchy</param>
        /// <param name="focusedControl">Returns the last clicked control with any mouse button</param>
        /// <remarks>Iterates over the control's children collection</remarks>
        void EvaluateTopMostControl(out IUIControl topMostControl, out IUIControl focusedControl);
        /// <summary>
        /// Evaluate events enabled control
        /// </summary>
        /// <param name="focusedControl">Returns the focused control (last clicked control with any mouse button)</param>
        void EvaluateEventsEnabledControl(out IUIControl focusedControl);
        /// <summary>
        /// Invalidates the internal state and forces an update in the next call
        /// </summary>
        void Invalidate();
        /// <summary>
        /// Gets the control update order
        /// </summary>
        int GetUpdateOrder();
    }
}
