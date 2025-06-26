using Microsoft.AspNetCore.Components;
using System.Collections;

namespace Innovative.Blazor.Components.Components;

/// <summary>
/// Base class for InnovativeDropdown component containing all parameters and basic functionality.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public abstract class InnovativeDropdownBase<TValue> : ComponentBase
{
    /// <summary>
    /// Gets or sets the element reference.
    /// </summary>
    protected ElementReference ElementRef { get; set; }

    /// <summary>
    /// Gets or sets the component id.
    /// </summary>
    [Parameter] public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name attribute.
    /// </summary>
    [Parameter] public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CSS class.
    /// </summary>
    [Parameter] public string CssClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CSS style.
    /// </summary>
    [Parameter] public string Style { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tab index.
    /// </summary>
    [Parameter] public int TabIndex { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the component is visible.
    /// </summary>
    [Parameter] public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the component is disabled.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the component is read-only.
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the current value.
    /// </summary>
    [Parameter] public TValue Value { get; set; } = default(TValue)!;

    /// <summary>
    /// Gets or sets the value changed callback.
    /// </summary>
    [Parameter] public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the change event callback.
    /// </summary>
    [Parameter] public EventCallback<TValue> Change { get; set; }

    /// <summary>
    /// Gets or sets the search text changed callback.
    /// </summary>
    [Parameter] public EventCallback<string> SearchTextChanged { get; set; }

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    [Parameter] public IEnumerable? Data { get; set; }

    /// <summary>
    /// Gets or sets the property name to use for display text.
    /// </summary>
    [Parameter] public string TextProperty { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property name to use for the value.
    /// </summary>
    [Parameter] public string ValueProperty { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the property name to check if an item is disabled.
    /// </summary>
    [Parameter] public string DisabledProperty { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the filter placeholder text.
    /// </summary>
    [Parameter] public string FilterPlaceholder { get; set; } = "Search...";

    /// <summary>
    /// Gets or sets a value indicating whether multiple selection is allowed.
    /// </summary>
    [Parameter] public bool Multiple { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether filtering is allowed.
    /// </summary>
    [Parameter] public bool AllowFiltering { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether clearing the value is allowed.
    /// </summary>
    [Parameter] public bool AllowClear { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether select all is allowed (for multiple selection).
    /// </summary>
    [Parameter] public bool AllowSelectAll { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the dropdown should open on focus.
    /// </summary>
    [Parameter] public bool OpenOnFocus { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether selected items should be displayed as chips.
    /// </summary>
    [Parameter] public bool Chips { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of selected labels to display.
    /// </summary>
    [Parameter] public int MaxSelectedLabels { get; set; } = 4;

    /// <summary>
    /// Gets or sets the separator for multiple selected items display.
    /// </summary>
    [Parameter] public string Separator { get; set; } = ", ";

    /// <summary>
    /// Gets or sets the text to display for selected items count.
    /// </summary>
    [Parameter] public string SelectedItemsText { get; set; } = "items selected";

    /// <summary>
    /// Gets or sets the select all text.
    /// </summary>
    [Parameter] public string SelectAllText { get; set; } = "Select All";

    /// <summary>
    /// Gets or sets the popup style (e.g., max-height).
    /// </summary>
    [Parameter] public string PopupStyle { get; set; } = "max-height: 200px; overflow-y: auto;";

    /// <summary>
    /// Gets or sets the template for rendering individual items.
    /// </summary>
    [Parameter] public RenderFragment<object>? ItemTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for rendering the selected value.
    /// </summary>
    [Parameter] public RenderFragment<object>? ValueTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for the dropdown header.
    /// </summary>
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>
    /// Gets or sets the template for when no items are found.
    /// </summary>
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the component.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? Attributes { get; set; }

    /// <summary>
    /// Gets or sets the callback for when the dropdown is opened.
    /// </summary>
    [Parameter] public Action? OnOpen { get; set; }

    /// <summary>
    /// Gets or sets the callback for when the dropdown is closed.
    /// </summary>
    [Parameter] public Action? OnClose { get; set; }
}