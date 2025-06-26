# InnovativeDropdown Component

The `InnovativeDropdown` component is a feature-rich dropdown selector built without dependencies on Radzen components. It provides comprehensive functionality for single and multiple item selection with customizable templates and advanced features.

## Basic Usage

### Simple Dropdown

```html
<InnovativeDropdown TValue="string" 
                   Data="@items" 
                   @bind-Value="@selectedValue"
                   TextProperty="Name"
                   ValueProperty="Id"
                   Placeholder="Select an item" />
```

### Multiple Selection

```html
<InnovativeDropdown TValue="List<string>" 
                   Data="@items" 
                   @bind-Value="@selectedValues"
                   TextProperty="Name"
                   ValueProperty="Id"
                   Multiple="true"
                   Chips="true"
                   Placeholder="Select multiple items" />
```

### With Filtering

```html
<InnovativeDropdown TValue="string" 
                   Data="@items" 
                   @bind-Value="@selectedValue"
                   TextProperty="Name"
                   ValueProperty="Id"
                   AllowFiltering="true"
                   FilterPlaceholder="Type to filter..." />
```

## Features

- **Single and Multiple Selection**: Support for both single item selection and multiple item selection
- **Filtering**: Built-in search functionality to filter items
- **Chips Display**: Display selected items as removable chips in multiple selection mode
- **Custom Templates**: Support for custom item display templates and value templates
- **Keyboard Navigation**: Full keyboard support with arrow keys, Enter, Escape, etc.
- **Select All**: Option to select/deselect all items in multiple mode
- **Disabled Items**: Support for disabling specific items
- **Clear Functionality**: Option to clear selected values
- **Accessibility**: Full ARIA support and keyboard navigation
- **Responsive Design**: Mobile-friendly with responsive popup positioning
- **Dark Mode**: Built-in dark mode support

## Parameters

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `TValue` | `default(TValue)` | The current selected value(s) |
| `ValueChanged` | `EventCallback<TValue>` | - | Callback when value changes |
| `Data` | `IEnumerable` | `null` | The data source for dropdown items |
| `TextProperty` | `string` | `""` | Property name for display text |
| `ValueProperty` | `string` | `""` | Property name for item value |
| `DisabledProperty` | `string` | `""` | Property name to check if item is disabled |

### Display Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Placeholder` | `string` | `""` | Placeholder text when no value selected |
| `CssClass` | `string` | `""` | Additional CSS classes |
| `Style` | `string` | `""` | Inline styles |
| `Disabled` | `bool` | `false` | Whether the dropdown is disabled |
| `ReadOnly` | `bool` | `false` | Whether the dropdown is read-only |
| `Visible` | `bool` | `true` | Whether the dropdown is visible |

### Behavior Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Multiple` | `bool` | `false` | Enable multiple selection |
| `AllowFiltering` | `bool` | `false` | Enable filtering/searching |
| `AllowClear` | `bool` | `false` | Show clear button |
| `AllowSelectAll` | `bool` | `false` | Show select all option (multiple mode) |
| `OpenOnFocus` | `bool` | `false` | Open dropdown when focused |
| `Chips` | `bool` | `false` | Display selected items as chips |

### Multiple Selection Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `MaxSelectedLabels` | `int` | `4` | Max number of labels to show before "X items selected" |
| `Separator` | `string` | `", "` | Separator for multiple items display |
| `SelectedItemsText` | `string` | `"items selected"` | Text for item count display |
| `SelectAllText` | `string` | `"Select All"` | Text for select all option |

### Template Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `ItemTemplate` | `RenderFragment<object>` | Template for rendering individual items |
| `ValueTemplate` | `RenderFragment<object>` | Template for rendering selected value |
| `HeaderTemplate` | `RenderFragment` | Template for dropdown header |
| `EmptyTemplate` | `RenderFragment` | Template when no items found |

### Event Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Change` | `EventCallback<TValue>` | Triggered when selection changes |
| `SearchTextChanged` | `EventCallback<string>` | Triggered when search text changes |
| `OnOpen` | `Action` | Triggered when dropdown opens |
| `OnClose` | `Action` | Triggered when dropdown closes |

## Examples

### Custom Item Template

```html
<InnovativeDropdown TValue="string" 
                   Data="@people" 
                   @bind-Value="@selectedPersonId"
                   ValueProperty="Id">
    <ItemTemplate>
        <div class="person-item">
            <img src="@context.Avatar" alt="@context.Name" class="avatar" />
            <div>
                <div class="name">@context.Name</div>
                <div class="email">@context.Email</div>
            </div>
        </div>
    </ItemTemplate>
</InnovativeDropdown>
```

### Custom Value Display

```html
<InnovativeDropdown TValue="string" 
                   Data="@items" 
                   @bind-Value="@selectedValue"
                   TextProperty="Name"
                   ValueProperty="Id">
    <ValueTemplate>
        <span class="selected-item">
            <i class="icon-@context.Type"></i>
            @context.Name
        </span>
    </ValueTemplate>
</InnovativeDropdown>
```

### With Events

```html
<InnovativeDropdown TValue="string" 
                   Data="@items" 
                   @bind-Value="@selectedValue"
                   TextProperty="Name"
                   ValueProperty="Id"
                   Change="@OnSelectionChanged"
                   OnOpen="@OnDropdownOpened"
                   OnClose="@OnDropdownClosed" />

@code {
    private void OnSelectionChanged(string newValue)
    {
        // Handle selection change
        Console.WriteLine($"Selected: {newValue}");
    }
    
    private void OnDropdownOpened()
    {
        Console.WriteLine("Dropdown opened");
    }
    
    private void OnDropdownClosed()
    {
        Console.WriteLine("Dropdown closed");
    }
}
```

## Data Models

The dropdown works with any data type. Here's an example model:

```csharp
public class SelectItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDisabled { get; set; }
    public string Category { get; set; }
}
```

## Styling

The component includes comprehensive CSS classes for customization:

- `.innovative-dropdown` - Main container
- `.innovative-dropdown-display` - Display area
- `.innovative-dropdown-panel` - Dropdown panel
- `.innovative-dropdown-item` - Individual items
- `.innovative-dropdown-chip` - Chips in multiple mode
- And many more for specific states and elements

## Accessibility

The component follows ARIA guidelines and includes:

- Proper ARIA labels and roles
- Keyboard navigation support
- Screen reader compatibility
- Focus management
- High contrast support

## Keyboard Shortcuts

- **Enter/Space**: Open dropdown or select highlighted item
- **Escape**: Close dropdown
- **Arrow Up/Down**: Navigate items
- **Tab**: Close dropdown and move to next element

## Browser Support

The component supports all modern browsers:
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Migration from RadzenDropDown

This component provides similar functionality to RadzenDropDown with these key differences:

1. **No Radzen dependency**: Completely standalone implementation
2. **Enhanced styling**: Modern, accessible CSS design
3. **Improved performance**: Optimized rendering and event handling
4. **Better accessibility**: Full ARIA support and keyboard navigation
5. **Mobile-friendly**: Responsive design with mobile optimizations

To migrate from RadzenDropDown, simply replace the component name and update any Radzen-specific properties to the equivalent InnovativeDropdown properties.