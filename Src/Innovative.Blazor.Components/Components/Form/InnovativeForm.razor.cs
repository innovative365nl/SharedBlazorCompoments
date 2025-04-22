#region

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Innovative.Blazor.Components.Components.Common;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen.Blazor;

#endregion

namespace Innovative.Blazor.Components.Components;

public partial class InnovativeForm<TModel> : ComponentBase, Common.IDynamicBaseComponent
{
    private readonly Dictionary<string, object?> formValues = new Dictionary<string, object?>();
    private readonly IInnovativeStringLocalizer localizer;
    private const int StartSequenceNumberLoop = 4;

    public InnovativeForm(IInnovativeStringLocalizerFactory localizerFactory)
    {
        var uiClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
        var resourceType = uiClassAttribute?.ResourceType ?? typeof(TModel);
        Debug.Assert(localizerFactory != null, nameof(localizerFactory) + " != null");
        localizer = localizerFactory.Create(resourceType);
    }

    [Parameter] public required TModel Model { get; set; }
    [Parameter] public EventCallback<TModel> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [CascadingParameter] public required SidePanelComponent<TModel>? ParentDialog { get; set; }

    private IReadOnlyCollection<PropertyInfo> ungroupedProperties { get; set; } = new List<PropertyInfo>();

    protected IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>> OrderedColumnGroups { get; private set; } =
        new List<KeyValuePair<string, List<PropertyInfo>>>();

    protected override void OnInitialized()
    {
        ParentDialog?.SetFormComponent((IDynamicBaseComponent)this);
        base.OnInitialized();
    }

    public async Task OnSubmitPressed()
    {
        foreach (var entry in formValues)
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

    protected override void OnParametersSet()
    {
        if (Model != null)
        {
            foreach (var prop in GetPropertiesWithUiFormField())
            {
                formValues[key: prop.Name] = prop.GetValue(obj: Model);
            }

            OrganizePropertiesByGroups();
        }

        base.OnParametersSet();
    }

    private void OrganizePropertiesByGroups()
    {
        var propertiesWithAttributes = GetPropertiesWithUiFormField().ToList();
        var formClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
        var columnOrder = formClassAttribute?.ColumnOrder;

        var groupedProperties = propertiesWithAttributes
            .Where(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup != null)
            .GroupBy(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup)
            .ToDictionary(g => g.Key!, g => g.ToList());

        ungroupedProperties = propertiesWithAttributes
            .Where(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup == null)
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
        var fieldAttribute = property.GetCustomAttribute<UIFormField>();
        var propName = property.Name;

        builder.OpenComponent<RadzenLabel>(0);
        builder.AddAttribute(1, "Component", propName);
        if (fieldAttribute?.Name != null) builder.AddAttribute(2, "Text", localizer.GetString(fieldAttribute.Name));
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

    [ExcludeFromCodeCoverage]
    private static RenderFragment RenderFormComponent(object? value, UIFormField attribute)
    {
        return builder =>
        {
            try
            {
                if (value == null)
                {
                    builder.AddMarkupContent(sequence: 0, markupContent: "<span class=\"text-muted\">-</span>");
                    return;
                }

                if (attribute.FormComponent != null)
                {
                    builder.OpenComponent(sequence: 0, componentType: attribute.FormComponent);
                    builder.AddAttribute(sequence: 1, name: "Value", value: value);

                    if (attribute.DisplayParameters?.Length > 0)
                    {
                        var index = StartSequenceNumberLoop;
                        if (attribute.FormParameters != null)
                            foreach (var param in attribute.FormParameters)
                            {
                                var parts = param.Split(separator: '=', count: 2);
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


    private string GetStringValue(string propertyName)
    {
        if (formValues.TryGetValue(key: propertyName, value: out var value))
        {
            return value?.ToString() ?? string.Empty;
        }

        return null!;
    }

    private int? GetIntValue(string propertyName)
    {
        if (formValues.TryGetValue(key: propertyName, value: out var value) && value != null)
        {
            return Convert.ToInt32(value: value, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private bool? GetBoolValue(string propertyName)
    {
        if (formValues.TryGetValue(key: propertyName, value: out var value) && value != null)
        {
            return Convert.ToBoolean(value: value, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private DateTime? GetDateTimeValue(string propertyName)
    {
        if (formValues.TryGetValue(key: propertyName, value: out var value) && value != null)
        {
            return Convert.ToDateTime(value: value, CultureInfo.InvariantCulture);
        }

        return null;
    }

    private void SetValue(string propertyName, object? value) => formValues[key: propertyName] = value;

    private static PropertyInfo[] GetPropertiesWithUiFormField()
    {
        return typeof(TModel).GetProperties()
            .Where(predicate: p => p.GetCustomAttribute<UIFormField>() != null)
            .ToArray();
    }
}
