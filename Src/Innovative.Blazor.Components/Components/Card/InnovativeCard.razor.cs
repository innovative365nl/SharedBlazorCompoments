#region
    
using Microsoft.AspNetCore.Components;

#endregion

namespace Innovative.Blazor.Components.Components.Card;

public partial class InnovativeCard
{
    /// <summary>
    /// Indicates whether the card is in a loading state. When true, displays a loading indicator.
    /// </summary>
    [Parameter] public bool IsLoading { get; set; }

    /// <summary>
    /// The title text displayed at the top of the card.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// The content to be displayed inside the card. Required parameter.
    /// </summary>
    [Parameter] public required RenderFragment ChildContent { get; set; }
}