namespace ToadDesign;

/// <summary>An interaction state shown by <see cref="Theme.SetPreviewState"/> without real input.</summary>
public enum PreviewState
{
    /// <summary>The real state.</summary>
    None,

    /// <summary>As if the mouse were over the control.</summary>
    Hover,

    /// <summary>As if the control were pressed.</summary>
    Pressed,

    /// <summary>As if the control had keyboard focus.</summary>
    Focus,
}
