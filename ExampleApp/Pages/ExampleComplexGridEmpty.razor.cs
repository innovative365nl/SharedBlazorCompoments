using System.Diagnostics.CodeAnalysis;
using ExampleApp.Components;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using Radzen;

namespace ExampleApp.Pages;

public partial class ExampleComplexGridEmpty()
{
    public IEnumerable<AttributesGridModel> Items { get; set; } = new List<AttributesGridModel>();
}






