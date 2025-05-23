namespace Innovative.Blazor.Components.Attributes;

public abstract class UIClass : Attribute
{
    public Type? ResourceType { get; set; }
}