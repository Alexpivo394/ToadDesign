namespace ToadDesign;

/// <summary>Colour of a control, as Mantine's <c>color</c>: buttons, badges and menu items use the same tones.</summary>
public enum Tone
{
    /// <summary>The accent scale (green by default).</summary>
    Accent,

    /// <summary>Grey.</summary>
    Neutral,

    /// <summary>Blue: help and information.</summary>
    Info,

    /// <summary>Orange: warnings.</summary>
    Warning,

    /// <summary>Red: errors and destructive actions.</summary>
    Danger,
}
