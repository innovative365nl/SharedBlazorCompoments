using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
    using Innovative.Blazor.Components.Attributes;
    using Innovative.Blazor.Components.Components.Grid;
    using Innovative.Blazor.Components.Localizer;
    using Microsoft.AspNetCore.Components;
    using Microsoft.Extensions.Localization;
    using Radzen;
    using Radzen.Blazor;
    
    namespace Innovative.Blazor.Components.Components.Dialog;
    
    public partial class DynamicFormView<TModel>(IInnovativeStringLocalizerFactory localizerFactory) : ComponentBase, IDynamicBaseComponent
    {
        private readonly Dictionary<string, object?> _formValues = new Dictionary<string, object?>();
        private IInnovativeStringLocalizer _localizer = null!;
        [Parameter] public required TModel Model { get; set; }
        [Parameter] public EventCallback<TModel> OnSave { get; set; }
        [Parameter] public EventCallback OnCancel { get; set; }
        [CascadingParameter] public required RightSideDialog<TModel> ParentDialog { get; set; }

        private IReadOnlyCollection<PropertyInfo> UngroupedProperties { get; set; } = new List<PropertyInfo>();

        protected IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>> OrderedColumnGroups { get; private set; } = new List<KeyValuePair<string, List<PropertyInfo>>>();

        public async Task OnSubmitPressed()
        {
            foreach (var entry in _formValues)
            {
                var prop = typeof(TModel).GetProperty(name: entry.Key);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(obj: Model, value: entry.Value);
                }
            }
    
            await OnSave.InvokeAsync().ConfigureAwait(false);
        }

        public async Task OnCancelPressed()
        {
            await OnCancel.InvokeAsync().ConfigureAwait(false);
        }

        protected string GetColumnWidthClass(string columnGroup)
        {
            var formClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
            if (formClassAttribute?.ColumnWidthNames != null && 
                formClassAttribute.ColumnWidthValues != null)
            {
                for (int i = 0; i < formClassAttribute.ColumnWidthNames.Length; i++)
                {
                    if (formClassAttribute.ColumnWidthNames[i] == columnGroup)
                    {
                        int width = formClassAttribute.ColumnWidthValues[i];
                        return width == 0 ? string.Empty : $"column-span-{width}";
                    }
                }
            }
    
            return string.Empty;
        }

        protected override void OnInitialized()
        {
            if (ParentDialog != null)
            {
                ParentDialog.SetFormComponent(formComponent: this);
            }

            var uiClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
            var resourceType = uiClassAttribute?.ResourceType ?? typeof(TModel);
            _localizer = localizerFactory.Create(resourceType);
    
            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            if (Model != null)
            {
                foreach (var prop in DynamicFormView<TModel>.GetPropertiesWithUiFormField())
                {
                    _formValues[key: prop.Name] = prop.GetValue(obj: Model);
                }
                
                OrganizePropertiesByGroups();
            }
            
            base.OnParametersSet();
        }

        private void OrganizePropertiesByGroups()
        {
            var propertiesWithAttributes = DynamicFormView<TModel>.GetPropertiesWithUiFormField().ToList();
            var formClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
            var columnOrder = formClassAttribute?.ColumnOrder;
    
            var groupedProperties = propertiesWithAttributes
                .Where(p => p.GetCustomAttribute<UIFormFieldAttribute>()?.ColumnGroup != null)
                .GroupBy(p => p.GetCustomAttribute<UIFormFieldAttribute>()?.ColumnGroup)
                .ToDictionary(g => g.Key!, g => g.ToList());
    
            UngroupedProperties = propertiesWithAttributes
                .Where(p => p.GetCustomAttribute<UIFormFieldAttribute>()?.ColumnGroup == null)
                .ToList();
    
            // Order the groups based on ColumnOrder if available
            if (columnOrder != null && columnOrder.Any())
            {
                OrderedColumnGroups = columnOrder
                    .Where(columnGroup => groupedProperties.ContainsKey(columnGroup))
                    .Select(columnGroup => new KeyValuePair<string, List<PropertyInfo>>(
                        columnGroup,
                        groupedProperties[columnGroup]))
                    .Concat(groupedProperties
                        .Where(g => !columnOrder.Contains(g.Key))
                        .Select(g => g)!)
                    .ToList();
            }
            else
            {
                OrderedColumnGroups = groupedProperties.ToList()!;
            }
        }
        
        [ExcludeFromCodeCoverage]

        protected RenderFragment RenderPropertyField(PropertyInfo property) => builder =>
        {
            var fieldAttribute = property.GetCustomAttribute<UIFormFieldAttribute>();
            var propName = property.Name;
    
            builder.OpenComponent<RadzenLabel>(0);
            builder.AddAttribute(1, "Component", propName);
            if (fieldAttribute?.Name != null) builder.AddAttribute(2, "Text", _localizer.GetString(fieldAttribute.Name));
            builder.CloseComponent();
    
            if (property.PropertyType == typeof(string))
            {
                var value = GetStringValue(propertyName: propName);
    
                if (fieldAttribute is { UseWysiwyg: true })
                {
                    builder.OpenComponent<RadzenHtmlEditor>(3);
                    builder.AddAttribute(4, "Value", value);
                    builder.AddAttribute(5, "ValueChanged", EventCallback.Factory.Create<string>(this,
                        val => SetValue(propertyName: propName, value: val)));
                    builder.AddAttribute(6, "Style", "height: 250px;");
                    builder.AddAttribute(7, "Name", propName);
                }
                else
                {
                    builder.OpenComponent<RadzenTextBox>(8);
                    builder.AddAttribute(9, "Value", value);
                    builder.AddAttribute(10, "ValueChanged", EventCallback.Factory.Create<string>(this,
                        val => SetValue(propertyName: propName, value: val)));
                    builder.AddAttribute(11, "Name", propName);
                }
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
            {
                var readOnly = !property.CanWrite;
                var value = GetIntValue(propertyName: propName);
    
                builder.OpenComponent(12, typeof(RadzenNumeric<int?>));
                builder.AddAttribute(13, "Value", value);
                builder.AddAttribute(14, "ValueChanged", EventCallback.Factory.Create<int?>(this,
                    val => SetValue(propertyName: propName, value: val)));
                builder.AddAttribute(15, "Name", propName);
                builder.AddAttribute(16, "ReadOnly", readOnly);
            }
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
            {
                var value = GetBoolValue(propertyName: propName);
    
                builder.OpenComponent(17, typeof(RadzenCheckBox<bool?>));
                builder.AddAttribute(18, "Value", value);
                builder.AddAttribute(19, "ValueChanged", EventCallback.Factory.Create<bool?>(this,
                    val => SetValue(propertyName: propName, value: val)));
                builder.AddAttribute(20, "Name", propName);
            }
            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
            {
                var value = GetDateTimeValue(propertyName: propName);
    
                builder.OpenComponent(21, typeof(RadzenDatePicker<DateTime?>));
                builder.AddAttribute(22, "Value", value);
                builder.AddAttribute(23, "ValueChanged", EventCallback.Factory.Create<DateTime?>(this,
                    val => SetValue(propertyName: propName, value: val)));
                builder.AddAttribute(24, "Name", propName);
            }
            builder.CloseComponent();
        };

        private string GetStringValue(string propertyName)
        {
            if (_formValues.TryGetValue(key: propertyName, value: out var value))
            {
                return value?.ToString() ?? string.Empty;
            }
            return null!;
        }

        private int? GetIntValue(string propertyName)
        {
            if (_formValues.TryGetValue(key: propertyName, value: out var value) && value != null)
            {
                return Convert.ToInt32(value: value, CultureInfo.InvariantCulture);
            }
            return null;
        }

        private bool? GetBoolValue(string propertyName)
        {
            if (_formValues.TryGetValue(key: propertyName, value: out var value) && value != null)
            {
                return Convert.ToBoolean(value: value, CultureInfo.InvariantCulture);
            }
            return null;
        }

        private DateTime? GetDateTimeValue(string propertyName)
        {
            if (_formValues.TryGetValue(key: propertyName, value: out var value) && value != null)
            {
                return Convert.ToDateTime(value: value, CultureInfo.InvariantCulture);
            }
            return null;
        }

        private void SetValue(string propertyName, object? value)
        {
            _formValues[key: propertyName] = value;
        }

        private static PropertyInfo[] GetPropertiesWithUiFormField()
        {
            return typeof(TModel).GetProperties()
                .Where(predicate: p => p.GetCustomAttribute<UIFormFieldAttribute>() != null)
                .ToArray();
        }
    }