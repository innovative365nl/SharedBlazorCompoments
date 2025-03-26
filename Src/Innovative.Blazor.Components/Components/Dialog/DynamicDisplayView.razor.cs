using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Innovative.Blazor.Components.Attributes;
using Innovative.Blazor.Components.Components.Grid;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;

namespace Innovative.Blazor.Components.Components.Dialog;

public partial class DynamicDisplayView<TModel> : ComponentBase
{
   public DynamicDisplayView(IInnovativeStringLocalizerFactory localizerFactory)
    {
        var uiClassAttribute = typeof(TModel).GetCustomAttribute<UIGridClass>();
        var resourceType = uiClassAttribute?.ResourceType ?? typeof(TModel);
        Debug.Assert(localizerFactory != null, nameof(localizerFactory) + " != null");
        _localizer = localizerFactory.Create(resourceType);

    }
    private IInnovativeStringLocalizer _localizer;
    [Parameter] public TModel? Model { get; set; }
    [Parameter] public EventCallback<string> OnActionExecuted { get; set; }
    [CascadingParameter] private RightSideDialog<TModel>? ParentDialog { get; set; }

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
    /// Extracts component details from a property with UiFormViewAction attribute
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

    private async Task InvokeAction(MethodInfo method)
    {
        method.Invoke(obj: Model, parameters: null);
        await OnActionExecuted.InvokeAsync(arg: method.Name).ConfigureAwait(false);
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

                if (attribute.ViewComponent != null)
                    builder.OpenComponent(sequence: 0, componentType: attribute.ViewComponent);
                builder.AddAttribute(sequence: 1, name: "Value", value: value);

                if (attribute.ViewParameters?.Length > 0)
                {
                    var index = 4;
                    foreach (var param in attribute.ViewParameters)
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