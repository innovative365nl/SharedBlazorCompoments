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

namespace Innovative.Blazor.Components.Components.Grid;

public partial class InnovativeGrid<TItem> : ComponentBase
{
    private readonly bool _allowSorting;
    private readonly string? _defaultSortField;
    private readonly IInnovativeStringLocalizer _localizer;
    private readonly ILogger<InnovativeGrid<TItem>> _logger;

#pragma warning disable CA1859
    private IList<TItem> _selectedItems = new List<TItem>();
#pragma warning restore CA1859
    public InnovativeGrid(ILogger<InnovativeGrid<TItem>> logger, IInnovativeStringLocalizerFactory localizerFactory)
    {
        LocalizerFactory = localizerFactory;
        _logger = logger;
        var uiClassAttribute = typeof(TItem).GetCustomAttribute<UIGridClass>();
        _allowSorting = uiClassAttribute?.AllowSorting ?? true;
        _defaultSortField = uiClassAttribute?.DefaultSortField;
        var resourceType = ResourceType ?? uiClassAttribute?.ResourceType ?? typeof(TItem);
        _localizer = LocalizerFactory.Create(resourceType);
    }

    private IInnovativeStringLocalizerFactory LocalizerFactory { get; }

    /// <summary>
    ///     Data to be displayed in the grid
    /// </summary>
    [Parameter]
    public IEnumerable<TItem>? Data { get; set; }

    /// <summary>
    ///     Set selection mode for the grid
    /// </summary>
    [Parameter]
    public DataGridSelectionMode? SelectionMode { get; init; }

    /// <summary>
    ///     Parameter to enable row selection
    /// </summary>
    [Parameter]
    public bool EnableRowSelection { get; set; }

    [Parameter] public GridHeight? MinHeightOption { get; set; } = GridHeight.Minimal;

    /// <summary>
    ///     Enable or disable the grid loading screen
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter] public string? Title { get; set; }

    /// <summary>
    ///     The event that is triggered when the row selection changes
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> OnSelectionChanged { get; set; }

    /// <summary>
    ///     Optional resource type to use for localization instead of TItem
    /// </summary>
    [Parameter]
    public Type? ResourceType { get; init; }

    private RadzenDataGrid<TItem>? DataGrid { get; set; }

    /// <summary>
    ///     The items that are selected in the grid
    /// </summary>

    public IEnumerable<TItem> SelectedItems => _selectedItems;

    public async Task SetSelectedItemsAsync(IEnumerable<TItem> items)
    {
        Debug.Assert(items != null, nameof(items) + " != null");
        await OnSelectAsync(items: items).ConfigureAwait(false);
    }

    public async Task AddSelectedItemAsync(TItem item)
    {
        _selectedItems.Add(item);
        await OnSelectAsync(items: _selectedItems).ConfigureAwait(false);
    }

    /// <summary>
    ///     Applies a filter to the grid
    /// </summary>
    /// <param name="columnName"></param>
    /// <param name="value"></param>
    /// <param name="filterOperator"></param>
    public async Task ApplyFilter(string columnName, object value, FilterOperator filterOperator)
    {
        if (DataGrid is { ColumnsCollection: not null })
        {
            var column = (DataGrid.ColumnsCollection).FirstOrDefault(c => c.Property == columnName);
            if (column != null)
            {
                await column.SetFilterValueAsync(value: value).ConfigureAwait(false);
                column.SetFilterOperator(value: FilterOperator.Equals);
                await DataGrid.Reload().ConfigureAwait(false);
            }
            else
            {
                _logger.LogWarning(message: "Column {ColumnName} not found", columnName);
            }
        }
    }

    /// <summary>
    ///     Clears the filter on the grid
    /// </summary>
    /// <returns></returns>
    public Task ClearFilter()
    {
        DataGrid?.ColumnsCollection.Clear();
        return Task.CompletedTask;
    }

    private string GetColumnTitle(PropertyInfo property, UIGridField attribute)
    {
        if (string.IsNullOrEmpty(value: attribute?.Name))
        {
            return property.Name;
        }

        var localizedString = _localizer[name: attribute.Name];
        return localizedString.ResourceNotFound ? property.Name : localizedString.Value;
    }

    /// <summary>
    ///     Gets the localized column title using the TranslationKey from the UiFieldGrid attribute
    ///     or falls back to the property name if no key is specified
    /// </summary>
    private bool IsSortableColumn(PropertyInfo property)
    {
        if (_allowSorting) return false;
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
    ///     Refreshes the grid
    /// </summary>
    public async Task ReloadAsync()
    {
        if (DataGrid != null)
            await DataGrid.Reload().ConfigureAwait(false);
    }

    public void ClearSelection()
    {
        _selectedItems.Clear();
    }

    private async Task OnSelectAsync(IEnumerable<TItem> items)
    {
        if (_selectedItems == null)
            _selectedItems = new List<TItem>();
        _selectedItems.Clear();
        foreach (var item in items)
        {
            _selectedItems.Add(item);
        }

        await OnSelectionChanged.InvokeAsync(arg: _selectedItems).ConfigureAwait(false);
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
                    builder.AddAttribute(sequence: 1, name: "Value", value: value.ToString());

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