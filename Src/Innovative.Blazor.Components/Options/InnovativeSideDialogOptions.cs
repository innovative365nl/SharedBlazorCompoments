namespace Innovative.Blazor.Components.Options;

public class InnovativeSideDialogOptions
{
    public bool IsFilteringEnabled { get; set; }
    public FilterType FilterType { get; set; }
    public bool IsSortingEnabled { get; set; }
    
    
}
public enum FilterType
{
    None,
    Simple,
    Adavanced
}