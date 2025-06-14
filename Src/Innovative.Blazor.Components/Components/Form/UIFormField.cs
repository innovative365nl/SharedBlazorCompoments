using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components;

[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormField(string name) : UIField(name)
{
    public string? ColumnGroup { get; set; }
    public bool UseWysiwyg { get; set; }
    private static bool InheritsFromGenericCustomComponent(Type? type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(CustomComponent<>))
                return true;
            type = type.BaseType;
        }
        return false;
    }
    private Type? _displayComponent;
    public Type? DisplayComponent
    {
        get => _displayComponent;
        set
        {
            if (value != null && !InheritsFromGenericCustomComponent(value))
            {
                throw new ArgumentException($"DisplayComponent must inherit from CustomComponent<T>, but got {value.FullName}");
            }
            _displayComponent = value;
        }
    }
    public string[]? DisplayParameters { get; set; }
    private Type? _formComponent;
    public Type? FormComponent
    {
        get => _formComponent;
        set
        {
            if (value != null && !InheritsFromGenericCustomComponent(value))
            {
                throw new ArgumentException($"FormComponent must inherit from CustomComponent<T>, but got {value.FullName}");
            }
            _formComponent = value;
        }
    }
    public string[]? FormParameters { get; set; }
    public string? TextProperty { get; set; }
    public string DataTestId { get; set; } = string.Empty;
}
