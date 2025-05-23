namespace Innovative.Blazor.Components.Attributes;

public abstract class UIField(string name) : Attribute
{
    /// <summary>
    ///     Returns the name of the field.
    /// </summary>
    /// <remarks>
    ///     The name is used as a key to get the resource string. If the resource is not found it uses the name as a caption.
    /// </remarks>
    public string Name { get; } = name;
}
