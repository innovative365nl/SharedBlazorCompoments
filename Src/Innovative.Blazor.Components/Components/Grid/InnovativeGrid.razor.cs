#region

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Radzen;
using Radzen.Blazor;

#endregion

namespace Innovative.Blazor.Components.Components;

/// <summary>
/// Flexible and customizable grid component for displaying and managing data.
/// It supports sorting, filtering, formatting, auto translation, and custom components.
/// </summary>
/// <typeparam name="TItem"></typeparam>
public partial class InnovativeGrid<TItem> : ComponentBase
{
    private readonly bool allowSorting;
    private readonly string? defaultSortField;
    private readonly IInnovativeStringLocalizer localizer;
    private readonly ILogger<InnovativeGrid<TItem>> logger;

#pragma warning disable CA1859
    private IList<TItem> selectedItems = new List<TItem>();
#pragma warning restore CA1859
    public InnovativeGrid(ILogger<InnovativeGrid<TItem>> logger, IInnovativeStringLocalizerFactory localizerFactory)
    {
        this.localizerFactory = localizerFactory;
        this.logger = logger;
        var uiClassAttribute = typeof(TItem).GetCustomAttribute<UIGridClass>();
        allowSorting = uiClassAttribute?.AllowSorting ?? true;
        defaultSortField = uiClassAttribute?.DefaultSortField;
        var resourceType = ResourceType ?? uiClassAttribute?.ResourceType ?? typeof(TItem);
        localizer = this.localizerFactory.Create(resourceType);
    }

    private IInnovativeStringLocalizerFactory localizerFactory { get; }

    /// <summary>
    ///     The data collection to be displayed in the grid. This serves as the source for all grid operations
    ///     including filtering, sorting, and pagination. The data must be of type TItem.
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    ///     Determines the selection behavior of the grid. This can be set to Single for allowing only one item
    ///     to be selected at a time, Multiple for allowing multiple selections, or None to disable selection.
    ///     This works in conjunction with EnableRowSelection to control the overall selection behavior.
    /// </summary>
    [Parameter]
    public DataGridSelectionMode? SelectionMode { get; init; }

    /// <summary>
    ///     Enables or disables row selection in the grid. When set to true, users will be able to select rows
    ///     according to the SelectionMode. When false, row selection is completely disabled regardless of SelectionMode.
    /// </summary>
    [Parameter]
    public bool EnableRowSelection { get; set; }

    [Parameter] public GridHeight? MinHeightOption { get; set; } = GridHeight.Minimal;

    /// <summary>
    ///     Controls the visibility of the loading indicator. When set to true, the grid displays a loading animation
    ///     overlay, signaling to users that data is being loaded or processed. This is useful during asynchronous
    ///     operations to provide visual feedback to users.
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter] public string? Title { get; set; }

    /// <summary>
    ///     Event that fires when the selection in the grid changes. This provides a way for parent components
    ///     to react to selection changes and retrieve the currently selected items. The event passes the complete
    ///     collection of selected items as its parameter.
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> OnSelectionChanged { get; set; }

    /// <summary>
    ///     Optional resource type used for localization of grid elements. If specified, this type will be used
    ///     instead of TItem for retrieving localized strings. This allows for separation of data models and
    ///     localization resources when needed.
    /// </summary>
    [Parameter]
    public Type? ResourceType { get; init; }

    private RadzenDataGrid<TItem>? dataGrid { get; set; }

    /// <summary>
    ///     Gets the collection of items currently selected in the grid. This property provides access to the
    ///     selected items without triggering selection events, making it useful for read-only operations.
    /// </summary>
    public IEnumerable<TItem> SelectedItems => selectedItems;

    /// <summary>
    ///     Sets a single item as the selected item in the grid. This method clears any existing selections
    ///     and selects only the specified item. It also triggers the OnSelectionChanged event.
    /// </summary>
    /// <param name="item">The item to be selected</param>
    public async Task SetSelectedItemsAsync(TItem item)
    {
        await OnSelectAsync(new List<TItem>() { item }).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sets multiple items as the selected items in the grid. This method clears any existing selections
    ///     and selects the specified collection of items. It also triggers the OnSelectionChanged event.
    ///     This is useful for programmatically selecting items based on external logic.
    /// </summary>
    /// <param name="items">The collection of items to be selected</param>
    public async Task SetSelectedItemsAsync(IEnumerable<TItem> items)
    {
        Debug.Assert(items != null, nameof(items) + " != null");
        await OnSelectAsync(items: items).ConfigureAwait(false);
    }

    /// <summary>
    ///     Adds a single item to the current selection without clearing existing selections. This allows for
    ///     incrementally building a selection set. The method triggers the OnSelectionChanged event after
    ///     adding the item to inform subscribers of the change.
    /// </summary>
    /// <param name="item">The item to add to the current selection</param>
    public async Task AddSelectedItemAsync(TItem item)
    {
        selectedItems.Add(item);
        await OnSelectAsync(items: selectedItems).ConfigureAwait(false);
    }

    /// <summary>
    ///     Applies a filter to the grid based on the specified column, value, and filter operator. This method
    ///     programmatically sets filters without requiring user interaction. It finds the specified column and
    ///     applies the filter condition, then reloads the grid to display the filtered results.
    ///     If the column is not found, a warning is logged.
    /// </summary>
    /// <param name="columnName">The name of the column to filter</param>
    /// <param name="value">The value to filter by</param>
    /// <param name="filterOperator">The operator to use for filtering (e.g., Equals, Contains)</param>
    public async Task ApplyFilter(string columnName, object value, FilterOperator filterOperator)
    {
        if (dataGrid is { ColumnsCollection: not null })
        {
            var column = (dataGrid.ColumnsCollection).FirstOrDefault(c => c.Property == columnName);
            if (column != null)
            {
                await column.SetFilterValueAsync(value: value).ConfigureAwait(false);
                column.SetFilterOperator(value: FilterOperator.Equals);
                await dataGrid.Reload().ConfigureAwait(false);
            }
            else
            {
                logger.LogWarning(message: "Column {ColumnName} not found", columnName);
            }
        }
    }

    /// <summary>
    ///     Clears all filters from the grid, showing the unfiltered dataset. This removes any filtering
    ///     that has been applied either programmatically or through user interaction, returning the grid
    ///     to its initial state in terms of data visibility.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task ClearFilter()
    {
        dataGrid?.ColumnsCollection.Clear();
        return Task.CompletedTask;
    }

    private string GetColumnTitle(PropertyInfo property, UIGridField attribute)
    {
        if (string.IsNullOrEmpty(value: attribute?.Name))
        {
            return property.Name;
        }

        var localizedString = localizer[name: attribute.Name];
        return localizedString.ResourceNotFound ? property.Name : localizedString.Value;
    }

    /// <summary>
    ///     Gets the localized column title using the TranslationKey from the UiFieldGrid attribute
    ///     or falls back to the property name if no key is specified.
    /// </summary>
    private bool IsSortableColumn(PropertyInfo property)
    {
        if (allowSorting) return false;
        var attribute = property.GetCustomAttribute<UIGridField>();
        return attribute?.Sortable ?? true;
    }

    private static bool IsFilterableColumn(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<UIGridField>();
        return attribute?.Filterable ?? false;
    }

    private static bool IsVisibleColumn(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<UIGridField>();
        return attribute?.ShowByDefault ?? false;
    }

    private static bool IsFrozenColumn(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<UIGridField>();
        return attribute?.IsSticky ?? false;
    }

    private static bool HasCustomComponent(PropertyInfo property)
    {
        var attribute = property.GetCustomAttribute<UIGridField>();
        return attribute?.CustomComponentType != null;
    }

    /// <summary>
    ///     Refreshes the grid by reloading its data. This is useful when the underlying data source has changed
    ///     and the grid needs to be updated to reflect these changes. The method ensures that any sorting,
    ///     filtering, or pagination settings are preserved while refreshing the data.
    /// </summary>
    /// <returns>A task representing the asynchronous reload operation</returns>
    public async Task ReloadAsync()
    {
        if (dataGrid != null)
            await dataGrid.Reload().ConfigureAwait(false);
    }

    /// <summary>
    ///     Clears all current selections in the grid. This removes all items from the selected items collection
    ///     without triggering the OnSelectionChanged event. Use this method when you need to reset the selection
    ///     state without notifying subscribers.
    /// </summary>
    public void ClearSelection()
    {
        selectedItems.Clear();
    }

    private async Task OnSelectAsync(IEnumerable<TItem> items)
    {
        selectedItems.Clear();
        foreach (var item in items)
        {
            selectedItems.Add(item);
        }

        await OnSelectionChanged.InvokeAsync(arg: selectedItems).ConfigureAwait(false);
    }

    private static IEnumerable<PropertyWithAttribute> GetPropertiesWithAttributes()
    {
        return typeof(TItem).GetProperties()
            .Select(selector: p => new { Property = p, Attribute = p.GetCustomAttribute<UIGridField>() })
            .Where(predicate: x => x.Attribute != null) // Only include properties with the UiFieldGrid attribute
            .Select(selector: x =>
                new PropertyWithAttribute(PropertyInfo: x.Property, Name: x.Property.Name, GridField: x.Attribute!));
    }

    private string GetGridStyle()
    {
        var minHeight = MinHeightOption == GridHeight.Max ? "1162px" : "";
        return string.IsNullOrEmpty(minHeight)
            ? "--max-height: 1162px;"
            : $"--max-height: 1162px; --min-height: {minHeight};";
    }

    [ExcludeFromCodeCoverage]
    private static RenderFragment RenderCustomComponent(PropertyInfo property, object context, UIGridField gridField)
    {
        return builder =>
        {
            try
            {
                var value = property.GetValue(obj: context);
                if (value == null)
                {
                    builder.AddMarkupContent(sequence: 0, markupContent: "<span class=\"text-muted\">-</span>");
                    return;
                }

                if (value is IEnumerable<object> list && !value.GetType().Equals(o: typeof(string)))
                {
                    builder.OpenElement(sequence: 0, elementName: "div");
                    var index = 1;
                    foreach (var item in list)
                    {
                        builder.OpenComponent(sequence: index++, componentType: gridField.CustomComponentType!);
                        builder.AddAttribute(sequence: index++, name: "Value", value: item.ToString());

                        if (gridField.Parameters?.Length > 0)
                        {
                            foreach (var param in gridField.Parameters)
                            {
                                var parts = param.Split(separator: ':');
                                if (parts.Length == 2)
                                {
                                    builder.AddAttribute(sequence: index++, name: parts[0], value: parts[1]);
                                }
                            }
                        }

                        builder.CloseComponent();
                    }

                    builder.CloseElement();
                }
                else
                {
                    if (gridField.CustomComponentType != null)
                        builder.OpenComponent(sequence: 0, componentType: gridField.CustomComponentType);
                    builder.AddAttribute(sequence: 1, name: "Value", value: value);

                    if (gridField.Parameters?.Length > 0)
                    {
                        var index = 2;
                        foreach (var param in gridField.Parameters)
                        {
                            var parts = param.Split(separator: ':');
                            if (parts.Length == 2)
                            {
                                builder.AddAttribute(sequence: index++, name: parts[0], value: parts[1]);
                            }
                        }
                    }

                    builder.CloseComponent();
                }
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                builder.AddMarkupContent(sequence: 0,
                    markupContent: $"<span class=\"text-danger\">Error: {ex.Message}</span>");
            }
        };
    }

    private record PropertyWithAttribute(PropertyInfo PropertyInfo, string Name, UIGridField GridField);
}
