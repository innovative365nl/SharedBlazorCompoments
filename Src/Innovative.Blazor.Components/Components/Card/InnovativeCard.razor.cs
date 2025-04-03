#region
    
using Microsoft.AspNetCore.Components;

#endregion

namespace Innovative.Blazor.Components.Components.Card;
/// <summary>
/// Indicates whether the card is in a loading state. When true, displays a loading indicator
/// to inform users that content is being processed or fetched.
/// </summary>
public partial class InnovativeCard
{
    /// <summary>
    /// Indicates whether the card is in a loading state. When true, displays a loading indicator
    /// to inform users that content is being processed or fetched.
    /// </summary>
    [Parameter] public bool IsLoading { get; set; }

    /// <summary>
    /// The title text displayed at the top of the card. Provides context about the contained content.
    /// When null or empty, no title section will be rendered.
    /// </summary>
    [Parameter] public string? Title { get; set; }

    /// <summary>
    /// The content to be displayed inside the card body. This is the main content area of the card
    /// and is required for the component to render properly.
    /// </summary>
    [Parameter] public required RenderFragment ChildContent { get; set; }
}