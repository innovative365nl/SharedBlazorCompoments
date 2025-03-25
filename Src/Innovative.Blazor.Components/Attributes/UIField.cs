namespace Innovative.Blazor.Components.Attributes;

public abstract class UIField : Attribute
{
    /// <summary>
    /// Key that is used to get the resource string if not found it will use the value
    /// </summary>
    public string? Name { get; set; }
}