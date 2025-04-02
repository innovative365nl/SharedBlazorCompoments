using ExampleApp.Extensions;
using Innovative.Blazor.Components.Components.Grid;

namespace ExampleApp.Pages;

[UIGridClass(AllowSorting = true , DefaultSortField = nameof(Age))]
public class InnovativeTestClass 
{
    [UIGridField(IsSticky = true, Sortable = true)]

    public string? Name { get; set; }

    [UIGridField(showByDefault: true, Sortable = true)] 
    
    public int Age { get; set; }

    [UIGridField] 
    public string? Address { get; set; }

    // [UIGridField(typeof(CustomColorStyle))]
    public string? Status { get; set; }

    // [UIGridField(typeof(CustomColorStyle), Parameters =  ["Color1:orange", "Color2:gray", "Color3:lightgreen"])]
    public string? Status2 { get; set; }
    
     [UIGridField(typeof(CustomBoolStyle))]
    public bool IsActive { get; set; } = true;
}