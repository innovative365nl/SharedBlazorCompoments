using System.Reflection;
using Innovative.Blazor.Components.Attributes;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Radzen;
using Radzen.Blazor;

namespace Innovative.Blazor.Components.Components.Grid;

public partial class InnovativeGrid<TItem> : ComponentBase
{
    private bool _allowSorting;
    private string? _defaultSortField;
    private IInnovativeStringLocalizer _localizer;
    private ILogger<InnovativeGrid<TItem>> _logger;

    private Type _resourceType;
    private IList<TItem> _selectedItems = new List<TItem>();
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

    private  IInnovativeStringLocalizerFactory LocalizerFactory { get; init; }

    /// <summary>
    ///     Data to be displayed in the grid
    /// </summary>
    [Parameter]
    public IEnumerable<TItem> Data { get; set; }

    /// <summary>
    ///     Set selection mode for the grid
    /// </summary>
    [Parameter]
    public DataGridSelectionMode SelectionMode { get; init; }

    /// <summary>
    ///     Parameter to enable row selection
    /// </summary>
    [Parameter]
    public bool EnableRowSelection { get; set; }

    [Parameter] public GridHeight MinHeightOption { get; set; } = GridHeight.Minimal;

    /// <summary>
    ///     Enable or disable the grid loading screen
    /// </summary>
    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter] public string Title { get; set; }

    /// <summary>
    ///     The event that is triggered when the row selection changes
    /// </summary>
    [Parameter]
    public EventCallback<IEnumerable<TItem>> OnSelectionChanged { get; set; }

    /// <summary>
    ///     Optional resource type to use for localization instead of TItem
    /// </summary>
    [Parameter]
    public Type ResourceType { get; init; }

    private RadzenDataGrid<TItem> DataGrid { get; set; }

    /// <summary>
    ///     The items that are selected in the grid
    /// </summary>
    public IList<TItem> SelectedItems
    {
        get => _selectedItems;
        set
        {
            _selectedItems = value;
            _ = OnSelect(items: value);
        }
    }

    /// <summary>
/// Applies a filter to the grid
/// </summary>
/// <param name="columnName"></param>
/// <param name="value"></param>
/// <param name="filterOperator"></param>
    public async Task ApplyFilter(string columnName, object value, FilterOperator filterOperator)
    {
        var column = DataGrid.ColumnsCollection.Where(predicate: c => c.Property == columnName).FirstOrDefault();
        if (column != null)
        {
            await column.SetFilterValueAsync(value: value);
            column.SetFilterOperator(value: FilterOperator.Equals);
            await DataGrid.Reload();
        }
        else
        {
            _logger.LogWarning(message: "Column {columnName} not found", columnName);
        }
    }

    /// <summary>
    /// Clears the filter on the grid
    /// </summary>
    /// <returns></returns>
    public Task ClearFilter()
    {
        DataGrid.ColumnsCollection.Clear();
        return Task.CompletedTask;
    }

    private string GetColumnTitle(PropertyInfo property, UIGridField attribute)
    {
        if (_localizer == null || string.IsNullOrEmpty(value: attribute?.Name))
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
        if (!_allowSorting) return false;
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

    private bool HasCustomComponent(PropertyInfo property)
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
            await DataGrid.Reload();
    }

    public void ClearSelection()
    {
        SelectedItems?.Clear();
    }

    private async Task OnSelect(IEnumerable<TItem> items)
    {
        _selectedItems = items.ToList();
        await OnSelectionChanged.InvokeAsync(arg: _selectedItems);
    }

    private IEnumerable<PropertyWithAttribute> GetPropertiesWithAttributes()
    {
        return typeof(TItem).GetProperties()
            .Select(selector: p => new {Property = p, Attribute = p.GetCustomAttribute<UIGridField>()})
            .Where(predicate: x => x.Attribute != null) // Only include properties with the UiFieldGrid attribute
            .Select(selector: x => new PropertyWithAttribute(PropertyInfo: x.Property, Name: x.Property.Name, GridField: x.Attribute));
    }

    private string GetGridStyle()
    {
        var minHeight = MinHeightOption == GridHeight.Max ? "1162px" : "";
        if (minHeight == "")
            return "--max-height: 1162px;";
        return $"--max-height: 1162px; --min-height: {minHeight};";
    }

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
                        builder.OpenComponent(sequence: index++, componentType: gridField.CustomComponentType);
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
            catch (Exception ex)
            {
                builder.AddMarkupContent(sequence: 0, markupContent: $"<span class=\"text-danger\">Error: {ex.Message}</span>");
            }
        };
    }

    private record PropertyWithAttribute(PropertyInfo PropertyInfo, string Name, UIGridField GridField);
}