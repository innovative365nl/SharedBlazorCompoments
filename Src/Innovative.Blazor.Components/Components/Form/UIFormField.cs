using System.Diagnostics.CodeAnalysis;
using Innovative.Blazor.Components.Attributes;

namespace Innovative.Blazor.Components.Components;

/// <summary>
/// Attribute to mark a property as a form field for InnovativeForm. Controls rendering, grouping, component selection, and behavior.
/// </summary>
/// <param name="name">The display name or resource key for the field label.</param>
[ExcludeFromCodeCoverage]
[AttributeUsage(validOn: AttributeTargets.Property)]
public sealed class UIFormField(string name) : UIField(name)
{
    private Type? _displayComponent;
    private Type? _formComponent;

    /// <summary>
    /// The name of the column group this field belongs to. Used for grouping fields in the form layout.
    /// </summary>
    public string? ColumnGroup { get; set; }

    /// <summary>
    /// If true, renders this field using a WYSIWYG HTML editor (RadzenHtmlEditor) instead of a standard textbox.
    /// Only applies to string properties.
    /// </summary>
    public bool UseWysiwyg { get; set; }

    /// <summary>
    /// The component type to use for displaying this field in read-only (display) mode.
    /// Must inherit from CustomComponent&lt;T&gt;.
    /// </summary>
    public Type? DisplayComponent
    {
        get => _displayComponent;
        set
        {
            if (value != null
             && !InheritsFromGenericCustomComponent(value))
                throw new ArgumentException($"DisplayComponent must inherit from CustomComponent<T>, but got {value.FullName}");
            _displayComponent = value;
        }
    }

    /// <summary>
    /// The component type to use for editing this field in the form.
    /// Must inherit from CustomComponent&lt;T&gt;.
    /// </summary>
    public Type? FormComponent
    {
        get => _formComponent;
        set
        {
            if (value != null
             && !InheritsFromGenericCustomComponent(value))
                throw new ArgumentException($"FormComponent must inherit from CustomComponent<T>, but got {value.FullName}");
            _formComponent = value;
        }
    }

    /// <summary>
    /// Parameters to pass to the display component (as name=value strings).
    /// </summary>
    public string[]? DisplayParameters { get; set; }

    /// <summary>
    /// Parameters to pass to the form (edit) component (as name=value strings).
    /// </summary>
    public string[]? FormParameters { get; set; }

    /// <summary>
    /// The name of the property to use for display text (for complex types or lookups).
    /// </summary>
    public string? TextProperty { get; set; }

    /// <summary>
    /// The data-test-id attribute value for this field, used for automated testing.
    /// </summary>
    public string DataTestId { get; set; } = string.Empty;

    /// <summary>
    /// If true, changes to this field will trigger change notifications (INotifyFormValueChanged).
    /// </summary>
    public bool ShouldNotifyChanges { get; set; }

    /// <summary>
    /// If true, the field is rendered as read-only in the form, regardless of property setter.
    /// </summary>
    public bool ReadOnly { get; set; }

    // Conditional visibility support (similar to UIFormViewAction)
    // When set, the field is only visible if the specified property on the model meets the condition.
    // - If VisibleWhenEquals is null: the property must be a boolean and equal to true, otherwise require non-null.
    // - If VisibleWhenEquals is set: the property's value (converted to string) must equal this value (case-insensitive).
    //   Works for string, enum, and numeric types (comparison uses ToString of the property value).
    // You can invert the result by setting InvertCondition=true.
    public string? VisibleWhenProperty { get; init; }
    public string? VisibleWhenEquals { get; init; }
    public bool InvertCondition { get; init; }

    /// <summary>
    /// Checks if a type inherits from CustomComponent&lt;T&gt; (generic base class).
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type inherits from CustomComponent&lt;T&gt;, otherwise false.</returns>
    internal static bool InheritsFromGenericCustomComponent(Type? type)
    {
        while (type != null
            && type != typeof(object))
        {
            if (type.IsGenericType
             && type.GetGenericTypeDefinition() == typeof(CustomComponent<>))
                return true;
            type = type.BaseType;
        }
        return false;
    }
}
