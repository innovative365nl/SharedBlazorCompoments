using ExampleApp.Extensions;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Pages;

[UIGridClass(AllowSorting = true, DefaultSortField = nameof(Age))]
public class InnovativeTestClass
{
    [UIGridField(IsSticky = true, IsSortable = true)]
    public string? Name { get; set; }

    [UIGridField(IsSortable = true)]
    public int Age { get; set; }

    [UIGridField]
    public string? Address { get; set; }

    // [UIGridField(typeof(CustomColorStyle))]
    public string? Status { get; set; }

    // [UIGridField(typeof(CustomColorStyle), Parameters =  ["Color1:orange", "Color2:gray", "Color3:lightgreen"])]
    public string? Status2 { get; set; }

    [UIGridField(CustomComponentType = typeof(CustomBooleanStyle))]
    public bool IsActive { get; set; } = true;
}
