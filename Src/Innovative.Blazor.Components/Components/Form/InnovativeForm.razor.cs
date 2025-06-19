using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Reflection;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Radzen.Blazor;

namespace Innovative.Blazor.Components.Components;

public partial class InnovativeForm<TModel> : ComponentBase, IFormComponent
{
    private readonly Dictionary<string, object?> formValues = new Dictionary<string, object?>();
    private readonly IInnovativeStringLocalizer localizer;

    [Parameter] public required TModel Model { get; set; }

    [CascadingParameter] public required SidePanelComponent<TModel>? ParentDialog { get; set; }

    private IReadOnlyCollection<PropertyInfo> ungroupedProperties { get; set; } = new List<PropertyInfo>();

    private IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>> OrderedColumnGroups { get; set; } =
        new List<KeyValuePair<string, List<PropertyInfo>>>();

    public InnovativeForm(IInnovativeStringLocalizerFactory localizerFactory)
    {
        Debug.Assert(localizerFactory != null, $"{nameof(localizerFactory)} is null!");

        var uiClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
        var resourceType = uiClassAttribute?.ResourceType ?? typeof(TModel);
        localizer = localizerFactory.Create(resourceType);
    }

    protected override void OnParametersSet()
    {
        if (ParentDialog != null && Model is FormModel)
        {
            ParentDialog.SetFormComponent(this);
        }

        foreach (var prop in GetPropertiesWithUiFormField())
        {
            formValues[key: prop.Name] = prop.GetValue(obj: Model);
        }

        OrganizePropertiesByGroups();
    }

    private void OrganizePropertiesByGroups()
    {
        string[] columnOrder = Model is FormModel model
                                   ? model.Columns
                                          .Where(predicate: col => !string.IsNullOrEmpty(value: col.Name))
                                          .OrderBy(keySelector: col => col.Order)
                                          .Select(selector: col => col.Name!)
                                          .ToArray()
                                   : [];

        var propertiesWithAttributes = GetPropertiesWithUiFormField().ToList();
        var groupedProperties = propertiesWithAttributes
                                .Where(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup != null)
                                .GroupBy(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup)
                                .ToDictionary(g => g.Key!, g => g.ToList());

        ungroupedProperties = propertiesWithAttributes
                              .Where(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup == null)
                              .ToList();

        // Order the groups based on ColumnOrder if available
        if (columnOrder.Any())
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

    public Task OnFormSubmit()
    {
        foreach (var entry in formValues)
        {
            var prop = typeof(TModel).GetProperty(name: entry.Key);
            if (prop?.CanWrite ?? false)
            {
                prop.SetValue(obj: Model, value: entry.Value);
            }
        }

        return Task.CompletedTask;
    }

    public Task OnFormReset()
    {
        ParentDialog?.CloseCustomDialog();
        return Task.CompletedTask;
    }

    protected string GetColumnWidthClass(string columnGroup)
    {
        const int none = 0; // 0 width should return empty string
        int width = Model is FormModel model
                        ? model.Columns.SingleOrDefault(predicate: col => col.Name == columnGroup)?.Width ?? none
                        : none;

        return width == none ? string.Empty : $"column-span-{width}";
    }

    [ExcludeFromCodeCoverage]
    protected RenderFragment RenderPropertyField(PropertyInfo property) => builder =>
    {
        var fieldAttribute = property.GetCustomAttribute<UIFormField>();
        var propName = property.Name;
        int sequence = 0;

        builder.OpenComponent<RadzenLabel>(sequence++);
        builder.AddAttribute(sequence++, "Component", propName);


        if (fieldAttribute?.Name != null)
        {
            builder.AddAttribute(sequence++, "Text", localizer.GetString(fieldAttribute.Name));
        }

        builder.CloseComponent();

        if (property.PropertyType == typeof(string))
        {
            var value = GetStringValue(propertyName: propName);

            if (fieldAttribute is { UseWysiwyg: true })
            {
                builder.OpenComponent<RadzenHtmlEditor>(sequence++);
                builder.AddAttribute(sequence++, nameof(RadzenHtmlEditor.Value), value);
                builder.AddAttribute(sequence++,"data-test-id", fieldAttribute.DataTestId);

                builder.AddAttribute(sequence++, nameof(RadzenHtmlEditor.ValueChanged),
                                     EventCallback.Factory.Create<string>(this, val => SetValue(propertyName: propName, value: val)));
                builder.AddAttribute(sequence++, "Style", "height: max-content; min-height: 250px; max-height: 400px;");
                builder.AddAttribute(sequence, "Name", propName);
                builder.CloseComponent();
            }
            else
            {
                builder.OpenComponent<RadzenTextBox>(8);
                builder.AddAttribute(sequence++, nameof(RadzenTextBox.Value), value);
                builder.AddAttribute(sequence++,"data-test-id", fieldAttribute?.DataTestId);

                builder.AddAttribute(sequence++, nameof(RadzenTextBox.ValueChanged),

                                     EventCallback.Factory.Create<string>(this, val => SetValue(propertyName: propName, value: val)));
                builder.AddAttribute(sequence, "Name", propName);

                builder.CloseComponent();
            }
        }
        else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
        {
            var isReadOnly = !property.CanWrite;
            var value = GetIntValue(propertyName: propName);

            builder.OpenComponent(sequence++, typeof(RadzenNumeric<int?>));
            builder.AddAttribute(sequence++, nameof(RadzenNumeric<int?>.Value), value);
            builder.AddAttribute(sequence++,"data-test-id", fieldAttribute?.DataTestId);
            builder.AddAttribute(sequence++, nameof(RadzenNumeric<int?>.ValueChanged),
                                 EventCallback.Factory.Create<int?>(this, val => SetValue(propertyName: propName, value: val)));
            builder.AddAttribute(sequence++, "Name", propName);
            builder.AddAttribute(sequence, "ReadOnly", isReadOnly);
            builder.CloseComponent();
        }
        else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
        {
            var value = GetBoolValue(propertyName: propName);

            builder.OpenComponent(sequence++, typeof(RadzenCheckBox<bool?>));
            builder.AddAttribute(sequence++, nameof(RadzenCheckBox<bool?>.Value), value);
            builder.AddAttribute(sequence++,"data-test-id", fieldAttribute?.DataTestId);
            builder.AddAttribute(sequence++, nameof(RadzenCheckBox<bool?>.ValueChanged),
                                 EventCallback.Factory.Create<bool?>(this, val => SetValue(propertyName: propName, value: val)));
            builder.AddAttribute(sequence, "Name", propName);
            builder.CloseComponent();
        }
        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
        {
            var value = GetDateTimeValue(propertyName: propName);

            builder.OpenComponent(sequence++, typeof(RadzenDatePicker<DateTime?>));
            builder.AddAttribute(sequence++, nameof(RadzenDatePicker<DateTime?>.Value), value);
            builder.AddAttribute(sequence++,"data-test-id", fieldAttribute?.DataTestId);
            builder.AddAttribute(sequence++, nameof(RadzenDatePicker<DateTime?>.ValueChanged),
                                 EventCallback.Factory.Create<DateTime?>(this, val => SetValue(propertyName: propName, value: val)));
            builder.AddAttribute(sequence, "Name", propName);
            builder.CloseComponent();
        }
    };

    [ExcludeFromCodeCoverage]
    private RenderFragment RenderFormComponent(object? value, UIFormField attribute, PropertyInfo property)
    {
        var propName = property.Name;
        return builder =>
        {
            try
            {

                if (attribute.FormComponent != null)
                {
                    // Add label for component
                    int sequence = 0;
                    builder.OpenComponent<RadzenLabel>(sequence++);
                    builder.AddAttribute(sequence++, "Component", attribute.Name);
                    builder.AddAttribute(sequence, "Text", localizer.GetString(attribute.Name));
                    builder.CloseComponent();

                    // Add custom component
                    sequence = 0;
                    builder.OpenComponent(sequence: sequence++, componentType: attribute.FormComponent);
                    builder.AddAttribute(sequence: sequence++, name: "Value", value: value);
                    builder.AddAttribute(sequence++, "DataTestId", attribute.DataTestId);
                    builder.AddAttribute(sequence++, "ValueChanged",
                                         EventCallback.Factory.Create(this, val => SetValue(propertyName: propName, value: val)));

                    if (attribute.DisplayParameters?.Length > 0)
                    {
                        if (attribute.FormParameters != null)
                        {
                            foreach (string parameter in attribute.FormParameters ?? [])
                            {
                                int equalIndex = parameter.IndexOf('=', StringComparison.InvariantCultureIgnoreCase);
                                if (equalIndex > 0 && equalIndex < parameter.Length - 1)
                                {
                                    string paramName = parameter[..equalIndex];
                                    string paramValue = parameter[(equalIndex + 1)..];
                                    builder.AddAttribute(sequence: sequence++, name: paramName, value: paramValue);
                                }
                            }
                        }
                    }

                    builder.CloseComponent();
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                builder.AddMarkupContent(sequence: 0, markupContent: $"<span class=\"text-danger\">Error: {ex.Message}</span>");
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
