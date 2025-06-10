using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace Innovative.Blazor.Components.Components;

public partial class InnovativeDetail<TModel> : ComponentBase
{
    private readonly IInnovativeStringLocalizer localizer;
    private const int StartSequenceNumberLoop = 5;
    private const int MaxNumberOfButtonsBesideEachOther = 2;

    public InnovativeDetail(IInnovativeStringLocalizerFactory localizerFactory)
    {
        Debug.Assert(localizerFactory != null, $"{nameof(localizerFactory)} is null");

        var uiClassAttribute = typeof(TModel).GetCustomAttribute<UIGridClass>();
        var resourceType = uiClassAttribute?.ResourceType ?? typeof(TModel);
        localizer = localizerFactory.Create(resourceType);
    }

    [Parameter] public TModel? Model { get; set; }

    [Parameter] public EventCallback<string> OnActionExecuted { get; set; }

    [CascadingParameter] private SidePanelComponent<TModel>? parentDialog { get; set; }

    private IReadOnlyCollection<PropertyInfo> ungroupedProperties { get; set; } = new List<PropertyInfo>();

    private IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>> orderedColumnGroups { get; set; } =
        new List<KeyValuePair<string, List<PropertyInfo>>>();

    protected override void OnParametersSet()
    {
        if (Model != null)
        {
            OrganizePropertiesByGroups();
        }
    }

    private string GetColumnWidthClass(string columnGroup)
    {
        // First check if Model is DisplayFormModel and get column info from there
        if (Model is FormModel formModel)
        {
            var column = formModel.Columns.FirstOrDefault(c => c.Name == columnGroup);
            if (column is { Width: > 0 })
            {
                return $"column-span-{column.Width}";
            }
        }

        return string.Empty;
    }

    private void OrganizePropertiesByGroups()
    {
        var propertiesWithAttributes = GetPropertiesWithUiFormField().ToList();

        // Get column order from DisplayFormModel or attribute
        string[] columnOrder = Model is FormModel formModel
                                    ? formModel.Columns
                                               .Where(col=> !string.IsNullOrEmpty(col.Name))
                                               .OrderBy(col => col.Order)
                                               .Select(col => col.Name!)
                                               .ToArray()
                                    : [];

        // Group properties by column group
        var groupedProperties = propertiesWithAttributes
            .Where(p => !string.IsNullOrEmpty(p.GetCustomAttribute<UIFormField>()?.ColumnGroup))
            .GroupBy(p => p.GetCustomAttribute<UIFormField>()?.ColumnGroup!)
            .ToDictionary(g => g.Key!, g => g.ToList());

        ungroupedProperties = propertiesWithAttributes
                                .Where(p => string.IsNullOrEmpty(p.GetCustomAttribute<UIFormField>()?.ColumnGroup))
                                .ToList();

        // Order the groups based on ColumnOrder if available
        if (columnOrder.Any())
        {
            // Use an ordered dictionary to maintain the exact order specified
            var orderedGroups = new List<KeyValuePair<string, List<PropertyInfo>>>();

            // First add groups that match the column order
            foreach (var columnName in columnOrder)
            {
                if (groupedProperties.ContainsKey(columnName))
                {
                    if (groupedProperties[columnName].Any())
                    {
                        orderedGroups.Add(new KeyValuePair<string, List<PropertyInfo>>(columnName, groupedProperties[columnName]));

                        // Remove the processed group to avoid duplication
                        groupedProperties.Remove(columnName);
                    }
                }
            }

            // Then add any remaining groups not in column order
            orderedGroups.AddRange(groupedProperties.Select(g => g));

            orderedColumnGroups = orderedGroups;
        }
        else
        {
            orderedColumnGroups = groupedProperties.ToList()!;
        }
    }

    private void HandleActionProperty(PropertyInfo property, UIFormViewAction actionAttribute)
    {

        var action = property.GetValue(obj: Model) as Delegate;
        if (action == null)
        {
            return;
        }

        if (actionAttribute.CustomComponent != null)
        {
            if (parentDialog != null)
            {
#pragma warning disable BL0005
                parentDialog.ActionChildContent = builder =>
#pragma warning restore BL0005
                {
                    var (component, parameters, title) = GetActionDetails(propertyName: property.Name);
                    var i = 0;
                    builder.OpenComponent(sequence: i++, componentType: component);
                    foreach (var param in parameters)
                    {
                        builder.AddAttribute(sequence: i++, name: param.Key, value: param.Value);
                    }
                    builder.AddAttribute(sequence: i, name: "ParentDialog", value: parentDialog);
                    builder.CloseComponent();
                };

                parentDialog.OpenCustomDialog();
            }
        }
        else
        {
            var parameters = action.Method.GetParameters();

            if (parameters.Length == 0)
            {
                action.DynamicInvoke();
            }
            else
            {
                var paramType = parameters[0].ParameterType;
                var defaultValue = paramType.IsValueType ? Activator.CreateInstance(type: paramType) : null;
                action.DynamicInvoke(defaultValue);
            }

            OnActionExecuted.InvokeAsync(arg: property.Name);
        }
    }

    /// <summary>
    ///     Extracts component details from a property with UiFormViewAction attribute
    /// </summary>
    /// <param name="propertyName">The name of the property to get action details from</param>
    /// <returns>A tuple with the Component type, Parameters dictionary, and Title</returns>
    public (Type Component, Dictionary<string, object> Parameters, string Title) GetActionDetails(string propertyName)
    {
        var property = typeof(TModel).GetProperty(name: propertyName);
        if (property == null)
        {
            return (null, null, null)!;
        }

        var actionAttribute = property.GetCustomAttribute<UIFormViewAction>();
        if (actionAttribute == null)
        {
            return (null, null, null)!;
        }

        var parameters = new Dictionary<string, object?>
                         {
                            { "Model", Model },
                            { "ActionProperty", property.Name }
                         };

        return (actionAttribute.CustomComponent, parameters, actionAttribute.Name)!;
    }

    private static PropertyInfo[] GetPropertiesWithUiFormField()
    {
        return typeof(TModel).GetProperties()
                             .Where(predicate: p => p.GetCustomAttribute<UIFormField>() != null)
                             .ToArray();
    }

    [ExcludeFromCodeCoverage]
    private static RenderFragment RenderViewComponent(object? value, UIFormField attribute)
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

                       if (attribute.DisplayComponent != null)
                       {
                           builder.OpenComponent(sequence: 0, componentType: attribute.DisplayComponent);
                       }
                       builder.AddAttribute(sequence: 1, name: "Value", value: value);
                       builder.AddAttribute(sequence: sequence++, "DataTestId", attribute.DataTestId);

                       if (attribute!.DisplayParameters?.Length > 0)
                       {
                            var index = StartSequenceNumberLoop;
                            foreach (var param in attribute.DisplayParameters)
                            {
                                var parts = param.Split(separator: ':', count: 2);
                                if (parts.Length == 2)
                                {
                                    builder.AddAttribute(sequence: index++, name: parts[0], value: parts[1]);
                                }
                            }
                       }

                       builder.CloseComponent();
                   }
#pragma warning disable CA1031
                   catch (Exception ex)
#pragma warning restore CA1031
                   {
                       builder.AddMarkupContent(sequence: 0, markupContent: $"<span class=\"text-danger\">Error: {ex.Message}</span>");
                   }
               };
    }

    internal ButtonDefinition? GetSplitButtonDefinition()
    {
        List<PropertyInfo> actionProperties = GetActionProperties();

        if (actionProperties.Count <= MaxNumberOfButtonsBesideEachOther)
        {
            return null;
        }

        var property = actionProperties.First();
        var attribute = property.GetCustomAttribute<UIFormViewAction>()!;
        var name = attribute?.Name ?? property.Name;
        var actionName = localizer.GetString(name);

        return new ButtonDefinition { ActionName = actionName, Property = property, ActionAttribute = attribute! };
    }

    internal ButtonDefinition[] GetSplitButtonDefinitionItems()
    {
        var result = new List<ButtonDefinition>();

        List<PropertyInfo> actionProperties = GetActionProperties();

        if (actionProperties.Count <= MaxNumberOfButtonsBesideEachOther)
        {
            return result.ToArray();
        }

        foreach (var property in actionProperties.Skip(1))
        {
            var actionAttribute = property.GetCustomAttribute<UIFormViewAction>()!;
            var actionName = actionAttribute?.Name ?? property.Name;
            var translatedActionName = localizer.GetString(actionName);

            result.Add(new ButtonDefinition { ActionName = translatedActionName, Property = property, ActionAttribute = actionAttribute! });
        }

        return result.ToArray();
    }

    internal ButtonDefinition[] GetButtonDefinitions()
    {
        var result = new List<ButtonDefinition>();

        List<PropertyInfo> actionProperties = GetActionProperties();

        if (actionProperties.Count <= MaxNumberOfButtonsBesideEachOther)
        {
            foreach (var property in actionProperties)
            {
                var prop = property;
                var actionAttribute = prop.GetCustomAttribute<UIFormViewAction>();
                if(actionAttribute != null)
                {
                    var actionName = localizer.GetString(actionAttribute.Name);
                    result.Add(new ButtonDefinition { ActionName = actionName, Property = prop, ActionAttribute = actionAttribute });
                }
            }
        }
        return result.ToArray();
    }

    private List<PropertyInfo> GetActionProperties()
    {
        List<PropertyInfo> result = typeof(TModel).GetProperties()
                                                  .Where(predicate: x =>
                                                                        x.GetCustomAttribute<UIFormViewAction>() != null
                                                                     && (x.PropertyType == typeof(Action) ||
                                                                         x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Action<>))
                                                                     && x.Name                              != nameof(FormModel.SaveFormAction)
                                                                     && x.Name                              != nameof(FormModel.CancelFormAction)
                                                                     && x.Name                              != nameof(FormModel.DeleteFormAction)
                                                                     && x.GetValue(obj: Model, index: null) != null)
                                                  .OrderBy(keySelector: p => p.GetCustomAttribute<UIFormViewAction>()!.Order)
                                                  .ToList();

        return result;
    }
}

internal record ButtonDefinition
{
    public required string ActionName { get; init; }
    public required PropertyInfo Property { get; init; }
    public required UIFormViewAction ActionAttribute { get; init; }
}
