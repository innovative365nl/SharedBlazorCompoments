using ExampleApp.Pages;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Components;

public partial class ChildrenDisplayComponent : CustomComponent<IEnumerable<Child>>
{
    private string? result;

    protected override void OnParametersSet()
    {
        var items = Value?
                             .Where(predicate: x => !string.IsNullOrEmpty(value: x.Value))
                             .Select(selector: x => GetReadableName(name: x.Value))
                             .OrderBy(x=>x)
                             .ToHashSet()
                          ?? [];

        if (items.Count > 0)
        {
            result = string.Join(",<br/>", items);
        }
    }

    private static string GetReadableName(string name)
    {
        const int maxDisplayChars = 18;

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
