using Innovative.Blazor.Components.Components.TrackInUrl;

namespace ExampleApp.Pages;

public partial class ExampleChangeTracker
{
    [TrackInUrl]
    public string Name { get; set; } = "Alice";

    [TrackInUrl]
    public int Age { get; set; } = 25;

    [TrackInUrl]
    public string Color { get; set; } = "Red";

    private void ChangeColor()
    {
        Color = "Blue";
        OnPropertyPossiblyChanged();
    }
}