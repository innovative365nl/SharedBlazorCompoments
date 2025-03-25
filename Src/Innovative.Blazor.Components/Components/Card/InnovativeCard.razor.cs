using Microsoft.AspNetCore.Components;

namespace Innovative.Blazor.Components.Components.Card;

public partial class InnovativeCard
{
    [Parameter] public bool IsLoading { get; set; }

    [Parameter] public string Title { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }
}