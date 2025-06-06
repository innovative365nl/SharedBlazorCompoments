using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class ChildrenDisplayComponent : CustomComponent<IEnumerable<Child>>
{
    private MarkupString ChildrenView { get; set; }

    protected override void OnParametersSet()
    {
        var children = string.Join(separator: ",<br/>"
                                 , values: (Value ?? [])
                                           .Where(predicate: x => !string.IsNullOrEmpty(value: x.Value))
                                           .Select(selector: x => GetReadableLicenseName(name: x.Value)));

        ChildrenView = new MarkupString(value: string.IsNullOrEmpty(value: children)
                                                   ? $"<span>{children}</span>"
                                                   : "<span class=\"text-muted\">No children assigned</span>");
    }

    private static string GetReadableLicenseName(string name)
    {
        const int maxDisplayChars = 20;

        if (name.Length <= maxDisplayChars)
        {
            return name;
        }
        int spaceIndex = name.IndexOf(value: ' ', startIndex: maxDisplayChars);
        return spaceIndex == -1
                   ? $"{name[..maxDisplayChars]}&hellip;"
                   : $"{name[..spaceIndex]}&hellip;";
    }
}
