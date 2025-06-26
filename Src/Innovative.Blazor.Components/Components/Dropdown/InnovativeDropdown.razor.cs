using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Innovative.Blazor.Components.Components;

/// <summary>
/// InnovativeDropdown component for selecting items from a list.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public partial class InnovativeDropdown<TValue> : InnovativeDropdownBase<TValue>
{
    private ElementReference FilterInput;
    private bool _isOpen;
    private int _highlightedIndex = -1;
    private string _searchText = string.Empty;
    private List<object> _selectedItems = new();
    private object? _selectedItem;
    private IEnumerable<object>? _filteredData;
    private bool _hasInitialized;

    /// <summary>
    /// Gets or sets a value indicating whether the dropdown is open.
    /// </summary>
    protected bool IsOpen
    {
        get => _isOpen;
        set
        {
            if (_isOpen != value)
            {
                _isOpen = value;
                if (value)
                {
                    OnOpen?.Invoke();
                }
                else
                {
                    OnClose?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the highlighted index for keyboard navigation.
    /// </summary>
    protected int HighlightedIndex
    {
        get => _highlightedIndex;
        set => _highlightedIndex = value;
    }

    /// <summary>
    /// Gets or sets the search text for filtering.
    /// </summary>
    protected string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                FilterData();
                SearchTextChanged.InvokeAsync(value);
            }
        }
    }

    /// <summary>
    /// Gets the selected items for multiple selection.
    /// </summary>
    protected List<object> SelectedItems => _selectedItems;

    /// <summary>
    /// Gets the selected item for single selection.
    /// </summary>
    protected object? SelectedItem => _selectedItem;

    /// <summary>
    /// Gets the filtered data based on search text.
    /// </summary>
    protected IEnumerable<object>? FilteredData => _filteredData;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        if (!string.IsNullOrEmpty(Id))
        {
            // Use provided Id
        }
        else
        {
            Id = $"innovative-dropdown-{Guid.NewGuid():N}";
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        // Initialize selected items/item from Value
        InitializeSelectedFromValue();
        
        // Filter data
        FilterData();
        
        _hasInitialized = true;
    }

    /// <summary>
    /// Initializes selected items from the current Value.
    /// </summary>
    private void InitializeSelectedFromValue()
    {
        if (Multiple)
        {
            _selectedItems.Clear();
            if (Value is IEnumerable<object> enumerable)
            {
                _selectedItems.AddRange(enumerable);
            }
            else if (Value != null)
            {
                // Handle case where Value is a single item but Multiple is true
                var matchingItem = FindItemByValue(Value);
                if (matchingItem != null)
                {
                    _selectedItems.Add(matchingItem);
                }
            }
        }
        else
        {
            _selectedItem = FindItemByValue(Value);
        }
    }

    /// <summary>
    /// Finds an item in Data that matches the given value.
    /// </summary>
    private object? FindItemByValue(object? value)
    {
        if (value == null || Data == null) return null;

        foreach (var item in Data)
        {
            if (GetItemValue(item)?.Equals(value) == true)
            {
                return item;
            }
        }
        return null;
    }

    /// <summary>
    /// Filters the data based on search text.
    /// </summary>
    private void FilterData()
    {
        if (Data == null)
        {
            _filteredData = null;
            return;
        }

        if (string.IsNullOrEmpty(SearchText))
        {
            _filteredData = Data.Cast<object>();
        }
        else
        {
            _filteredData = Data.Cast<object>().Where(item =>
            {
                var text = GetItemText(item);
                return text?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true;
            });
        }

        // Reset highlighted index when data changes
        _highlightedIndex = -1;
    }

    /// <summary>
    /// Gets the display text for an item.
    /// </summary>
    protected string GetItemText(object item)
    {
        if (item == null) return string.Empty;

        if (!string.IsNullOrEmpty(TextProperty))
        {
            var prop = item.GetType().GetProperty(TextProperty);
            return prop?.GetValue(item)?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Gets the value for an item.
    /// </summary>
    private object? GetItemValue(object item)
    {
        if (item == null) return null;

        if (!string.IsNullOrEmpty(ValueProperty))
        {
            var prop = item.GetType().GetProperty(ValueProperty);
            return prop?.GetValue(item);
        }

        return item;
    }

    /// <summary>
    /// Checks if an item is disabled.
    /// </summary>
    protected bool IsItemDisabled(object item)
    {
        if (item == null || string.IsNullOrEmpty(DisabledProperty)) return false;

        var prop = item.GetType().GetProperty(DisabledProperty);
        var value = prop?.GetValue(item);
        return value is bool disabled && disabled;
    }

    /// <summary>
    /// Checks if an item is selected.
    /// </summary>
    protected bool IsItemSelected(object item)
    {
        if (Multiple)
        {
            return _selectedItems.Contains(item);
        }
        else
        {
            return item == _selectedItem;
        }
    }

    /// <summary>
    /// Checks if all items are selected (for multiple selection).
    /// </summary>
    protected bool IsAllSelected()
    {
        if (!Multiple || FilteredData == null) return false;
        
        var availableItems = FilteredData.Where(item => !IsItemDisabled(item)).ToList();
        return availableItems.Any() && availableItems.All(IsItemSelected);
    }

    /// <summary>
    /// Gets CSS class for a dropdown item.
    /// </summary>
    protected string GetItemCssClass(bool isSelected, bool isHighlighted, bool isDisabled)
    {
        var classes = new List<string> { "innovative-dropdown-item" };
        
        if (isSelected) classes.Add("innovative-dropdown-item-selected");
        if (isHighlighted) classes.Add("innovative-dropdown-item-highlighted");
        if (isDisabled) classes.Add("innovative-dropdown-item-disabled");

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the CSS class for the dropdown component.
    /// </summary>
    protected string GetCssClass()
    {
        var classes = new List<string> { "innovative-dropdown" };
        
        if (Disabled) classes.Add("innovative-dropdown-disabled");
        if (IsOpen) classes.Add("innovative-dropdown-open");
        if (Multiple) classes.Add("innovative-dropdown-multiple");
        if (AllowClear) classes.Add("innovative-dropdown-clearable");
        if (!string.IsNullOrEmpty(CssClass)) classes.Add(CssClass);

        return string.Join(" ", classes);
    }

    /// <summary>
    /// Gets the value as string for form submission.
    /// </summary>
    private string GetValueAsString()
    {
        if (Multiple)
        {
            return string.Join(",", _selectedItems.Select(item => GetItemValue(item)?.ToString() ?? string.Empty));
        }
        else
        {
            return GetItemValue(_selectedItem)?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Toggles the dropdown open/closed state.
    /// </summary>
    protected async Task ToggleDropdown()
    {
        if (Disabled || ReadOnly) return;

        IsOpen = !IsOpen;
        
        if (IsOpen && AllowFiltering)
        {
            // Focus the filter input when opening
            await Task.Delay(1); // Small delay to ensure DOM is updated
            await FilterInput.FocusAsync();
        }
    }

    /// <summary>
    /// Handles key down events.
    /// </summary>
    protected async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (Disabled) return;

        switch (e.Key)
        {
            case "Enter":
            case " ": // Space
                if (!IsOpen)
                {
                    IsOpen = true;
                    StateHasChanged();
                }
                else if (HighlightedIndex >= 0 && FilteredData != null)
                {
                    var item = FilteredData.ElementAtOrDefault(HighlightedIndex);
                    if (item != null)
                    {
                        await SelectItem(item);
                    }
                }
                break;

            case "Escape":
                if (IsOpen)
                {
                    IsOpen = false;
                    StateHasChanged();
                }
                break;

            case "ArrowDown":
                if (!IsOpen)
                {
                    IsOpen = true;
                }
                else
                {
                    NavigateHighlight(1);
                }
                StateHasChanged();
                break;

            case "ArrowUp":
                if (IsOpen)
                {
                    NavigateHighlight(-1);
                    StateHasChanged();
                }
                break;

            case "Tab":
                if (IsOpen)
                {
                    IsOpen = false;
                    StateHasChanged();
                }
                break;
        }
    }

    /// <summary>
    /// Navigates the highlighted item by the specified direction.
    /// </summary>
    private void NavigateHighlight(int direction)
    {
        if (FilteredData == null) return;

        var items = FilteredData.ToList();
        if (!items.Any()) return;

        var newIndex = HighlightedIndex + direction;
        
        // Wrap around
        if (newIndex < 0)
            newIndex = items.Count - 1;
        else if (newIndex >= items.Count)
            newIndex = 0;

        HighlightedIndex = newIndex;
    }

    /// <summary>
    /// Sets the highlighted index.
    /// </summary>
    protected void SetHighlightedIndex(int index)
    {
        HighlightedIndex = index;
    }

    /// <summary>
    /// Handles focus events.
    /// </summary>
    protected void OnFocus()
    {
        if (OpenOnFocus && !IsOpen)
        {
            IsOpen = true;
        }
    }

    /// <summary>
    /// Handles blur events.
    /// </summary>
    protected void OnBlur()
    {
        // Close dropdown when focus is lost (with a small delay to allow for clicks on items)
        Task.Delay(150).ContinueWith(_ =>
        {
            if (IsOpen)
            {
                InvokeAsync(() =>
                {
                    IsOpen = false;
                    StateHasChanged();
                });
            }
        });
    }

    /// <summary>
    /// Handles filter input changes.
    /// </summary>
    protected void OnFilterInput(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;
        StateHasChanged();
    }

    /// <summary>
    /// Handles key down events in the filter input.
    /// </summary>
    protected async Task OnFilterKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                NavigateHighlight(1);
                StateHasChanged();
                break;

            case "ArrowUp":
                NavigateHighlight(-1);
                StateHasChanged();
                break;

            case "Enter":
                if (HighlightedIndex >= 0 && FilteredData != null)
                {
                    var item = FilteredData.ElementAtOrDefault(HighlightedIndex);
                    if (item != null)
                    {
                        await SelectItem(item);
                    }
                }
                break;

            case "Escape":
                IsOpen = false;
                StateHasChanged();
                break;
        }
    }

    /// <summary>
    /// Selects an item.
    /// </summary>
    protected async Task SelectItem(object item)
    {
        if (IsItemDisabled(item)) return;

        if (Multiple)
        {
            if (_selectedItems.Contains(item))
            {
                _selectedItems.Remove(item);
            }
            else
            {
                _selectedItems.Add(item);
            }

            // Update Value with selected values
            var selectedValues = _selectedItems.Select(GetItemValue).ToList();
            await UpdateValue(selectedValues);
        }
        else
        {
            _selectedItem = item;
            var value = GetItemValue(item);
            await UpdateValue(value);
            
            // Close dropdown for single selection
            IsOpen = false;
        }

        await Change.InvokeAsync(Value);
        StateHasChanged();
    }

    /// <summary>
    /// Removes a selected item (for multiple selection with chips).
    /// </summary>
    protected async Task RemoveSelectedItem(object item)
    {
        if (!Multiple || IsItemDisabled(item)) return;

        _selectedItems.Remove(item);
        var selectedValues = _selectedItems.Select(GetItemValue).ToList();
        await UpdateValue(selectedValues);
        await Change.InvokeAsync(Value);
        StateHasChanged();
    }

    /// <summary>
    /// Toggles select all (for multiple selection).
    /// </summary>
    protected async Task ToggleSelectAll(ChangeEventArgs e)
    {
        if (!Multiple || FilteredData == null) return;

        var selectAll = e.Value is bool b && b;
        var availableItems = FilteredData.Where(item => !IsItemDisabled(item)).ToList();

        if (selectAll)
        {
            // Add all available items that aren't already selected
            foreach (var item in availableItems)
            {
                if (!_selectedItems.Contains(item))
                {
                    _selectedItems.Add(item);
                }
            }
        }
        else
        {
            // Remove all available items from selection
            foreach (var item in availableItems)
            {
                _selectedItems.Remove(item);
            }
        }

        var selectedValues = _selectedItems.Select(GetItemValue).ToList();
        await UpdateValue(selectedValues);
        await Change.InvokeAsync(Value);
        StateHasChanged();
    }

    /// <summary>
    /// Clears the selected value(s).
    /// </summary>
    protected async Task ClearValue()
    {
        if (Multiple)
        {
            _selectedItems.Clear();
            await UpdateValue(new List<object>());
        }
        else
        {
            _selectedItem = null;
            await UpdateValue(default(TValue));
        }

        await Change.InvokeAsync(Value);
        StateHasChanged();
    }

    /// <summary>
    /// Updates the Value property and triggers ValueChanged.
    /// </summary>
    private async Task UpdateValue(object? newValue)
    {
        if (newValue is TValue typedValue)
        {
            Value = typedValue;
        }
        else if (newValue == null)
        {
            Value = default(TValue);
        }
        else
        {
            // Try to convert the value
            try
            {
                Value = (TValue)Convert.ChangeType(newValue, typeof(TValue));
            }
            catch
            {
                Value = default(TValue);
            }
        }

        await ValueChanged.InvokeAsync(Value);
    }

    /// <summary>
    /// Gets whether the dropdown has a value.
    /// </summary>
    protected bool HasValue
    {
        get
        {
            if (Multiple)
            {
                return _selectedItems.Count > 0;
            }
            else
            {
                return _selectedItem != null;
            }
        }
    }
}