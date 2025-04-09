#region

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Components.Form;
using Innovative.Blazor.Components.Components.Grid;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;

#endregion

namespace Innovative.Blazor.Components.Components.Detail;

public partial class InnovativeDetail<TModel> : ComponentBase
{
    private readonly IInnovativeStringLocalizer _localizer;
    private const int StartSequenceNumberLoop = 4;
    private const int MaxNumberOfButtonsBesideEachOther = 2;

    public InnovativeDetail(IInnovativeStringLocalizerFactory localizerFactory)
    {
        var uiClassAttribute = typeof(TModel).GetCustomAttribute<UIGridClass>();
        var resourceType = uiClassAttribute?.ResourceType ?? typeof(TModel);
        Debug.Assert(localizerFactory != null, nameof(localizerFactory) + " != null");
        _localizer = localizerFactory.Create(resourceType);
    }

    [Parameter] public TModel? Model { get; set; }
    [Parameter] public EventCallback<string> OnActionExecuted { get; set; }
    
    [CascadingParameter] private RightSideDialog<TModel>? ParentDialog { get; set; }

    private IReadOnlyCollection<PropertyInfo> UngroupedProperties { get; set; } = new List<PropertyInfo>();

    private IReadOnlyCollection<KeyValuePair<string, List<PropertyInfo>>> OrderedColumnGroups { get; set; } =
        new List<KeyValuePair<string, List<PropertyInfo>>>();

    protected override void OnParametersSet()
    {
        if (Model != null)
        {
            OrganizePropertiesByGroups();
        }

        base.OnParametersSet();
    }

    private string GetColumnWidthClass(string columnGroup)
    {
        // First check if Model is DisplayFormModel and get column info from there
        if (Model is DisplayFormModel displayModel)
        {
            var column = displayModel.ViewColumns.FirstOrDefault(c => c.Name == columnGroup);
            if (column is { Width: > 0 })
            {
                return $"column-span-{column.Width}";
            }
        }
        
        // Fallback to class attribute
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

private void OrganizePropertiesByGroups()
{
    var propertiesWithAttributes = GetPropertiesWithUiFormField().ToList();
    
    // Get column order from DisplayFormModel or attribute
    var customColumnOrder = Model is DisplayFormModel displayModel
        ? displayModel.ViewColumns.Select(c => c.Name).Where(n => n != null).ToArray()
        : null;
    
    var formClassAttribute = typeof(TModel).GetCustomAttribute<UIFormClass>();
    var attributeColumnOrder = formClassAttribute?.ColumnOrder;
    var columnOrder = customColumnOrder?.Any() == true ? customColumnOrder : attributeColumnOrder;

    // Group properties by column group
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
        // Use an ordered dictionary to maintain the exact order specified
        var orderedGroups = new List<KeyValuePair<string, List<PropertyInfo>>>();
        
        // First add groups that match the column order
        foreach (var columnName in columnOrder)
        {
            if (columnName != null && groupedProperties.ContainsKey(columnName))
            {
                if (groupedProperties[columnName].Any())
                {
                    orderedGroups.Add(new KeyValuePair<string, List<PropertyInfo>>(
                        columnName,
                        groupedProperties[columnName]));

                    // Remove the processed group to avoid duplication
                    groupedProperties.Remove(columnName);
                }
            }
                
        }
        
        // Then add any remaining groups not in column order
        orderedGroups.AddRange(groupedProperties.Select(g => g));
        
        OrderedColumnGroups = orderedGroups;
    }
    else
    {
        OrderedColumnGroups = groupedProperties.ToList()!;
    }
}

    private static PropertyInfo[] GetPropertiesWithViewAction()
    {
        return typeof(TModel).GetProperties()
            .Where(predicate: p => p.GetCustomAttribute<UIFormViewAction>() != null)
            .ToArray();
    }

    private void HandleActionProperty(PropertyInfo property, UIFormViewAction actionAttribute)
    {
        var action = property.GetValue(obj: Model) as Delegate;
        if (action == null)
            return;

        if (actionAttribute.CustomComponent != null)
        {
            var (component, parameters, title) = GetActionDetails(propertyName: property.Name);


            if (ParentDialog != null)
            {
#pragma warning disable BL0005
                ParentDialog.ActionChildContent = builder =>
#pragma warning restore BL0005
                {
                    builder.OpenComponent(sequence: 0, componentType: component);
                    {
                        var i = 1;
                        foreach (var param in parameters)
                        {
                            builder.AddAttribute(sequence: i++, name: param.Key, value: param.Value);
                        }

                        builder.AddAttribute(sequence: i++, name: "ParentDialog", value: ParentDialog);
                    }

                    builder.CloseComponent();
                };
                //todo: add title to dialog header with all links in document to return title. Or remove this. i think low business value
                //  ParentDialog.ActionTitle = title;
                ParentDialog.SetCustomDialog(isCustom: true);
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
            return (null, null, null)!;

        var actionAttribute = property.GetCustomAttribute<UIFormViewAction>();
        if (actionAttribute == null)
            return (null, null, null)!;

        var parameters = new Dictionary<string, object?>
        {
            { "Model", Model },
            { "ActionProperty", property.Name }
        };

        return (actionAttribute.CustomComponent, parameters, actionAttribute.Name ?? property.Name)!;
    }


    private static PropertyInfo[] GetPropertiesWithUiFormField()
    {
        return typeof(TModel).GetProperties()
            .Where(predicate: p => p.GetCustomAttribute<UIFormFieldAttribute>() != null)
            .ToArray();
    }

    [ExcludeFromCodeCoverage]
    private static RenderFragment RenderViewComponent(object? value, UIFormFieldAttribute attribute)
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
                    builder.OpenComponent(sequence: 0, componentType: attribute.DisplayComponent);
                builder.AddAttribute(sequence: 1, name: "Value", value: value);

                if (attribute.DisplayParameters?.Length > 0)
                {
                    var index = StartSequenceNumberLoop;
                    foreach (var param in attribute.DisplayParameters)
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
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                builder.AddMarkupContent(sequence: 0,
                    markupContent: $"<span class=\"text-danger\">Error: {ex.Message}</span>");
            }
        };
    }
}
