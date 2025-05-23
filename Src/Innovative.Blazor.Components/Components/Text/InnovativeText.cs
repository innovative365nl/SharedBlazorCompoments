using System.ComponentModel;
using System.Globalization;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Innovative.Blazor.Components.Components;

public sealed class InnovativeText : ComponentBase, IDisposable
{
    private INotifyPropertyChanged? notifyObject;
    private string? propertyName;
    private object? propertyValue;
    private bool disposed;

    [Inject] public required IInnovativeStringLocalizerFactory LocalizerFactory { get; set; }

    /// <summary>
    /// The text that will be displayed.
    /// </summary>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// The child content (markup) that will be displayed. Setting the Text property will override it.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// The style of the text.
    /// </summary>
    [Parameter]
    public TextStyle TextStyle { get; set; } = TextStyle.Body1;

    /// <summary>
    /// The horizontal alignment of the text.
    /// </summary>
    [Parameter]
    public TextAlign TextAlign { get; set; } = TextAlign.Left;

    /// <summary>
    /// The tag name of the element that will be rendered.
    /// </summary>
    [Parameter]
    public TagName TagName { get; set; } = TagName.Auto;

    /// <summary>
    /// Gets or sets the anchor name.
    /// </summary>
    [Parameter]
    public string? Anchor { get; set; }

    /// <summary>
    /// The object implementing INotifyPropertyChanged to bind to.
    /// </summary>
    [Parameter]
    public INotifyPropertyChanged? For { get; set; }

    /// <summary>
    /// The property name of the For object to display.
    /// </summary>
    [Parameter]
    public string? Property { get; set; }

    /// <summary>
    /// Additional attributes to be applied to the rendered element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? Attributes { get; set; }

    /// <summary>
    /// String format to apply to the property value.
    /// </summary>
    [Parameter]
    public string? Format { get; set; }

    /// <summary>
    /// Show the property name as a bold printed abel.
    /// </summary>
    [Parameter]
    public bool ShowPropertyName { get; set; }

    /// <summary>
    /// CSS class for the component.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// CSS style for the component.
    /// </summary>
    [Parameter]
    public string? Style { get; set; }


    protected override void OnParametersSet()
    {
        // Remove previous subscription if object changed
        if (notifyObject != For)
        {
            if (notifyObject != null)
            {
                notifyObject.PropertyChanged -= OnPropertyChanged;
            }

            notifyObject = For;

            if (notifyObject != null)
            {
                notifyObject.PropertyChanged += OnPropertyChanged;
            }
        }

        // Save property name
        propertyName = Property;

        // Update value from bound object
        UpdateValueFromObject();
    }

    private void UpdateValueFromObject()
    {
        if (notifyObject != null && !string.IsNullOrEmpty(propertyName))
        {
            var property = notifyObject.GetType().GetProperty(propertyName);
            if (property != null)
            {
                propertyValue = property.GetValue(notifyObject);
            }
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == propertyName || string.IsNullOrEmpty(e.PropertyName))
        {
            UpdateValueFromObject();
            StateHasChanged();
        }
    }

protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    ArgumentNullException.ThrowIfNull(builder);

    // Determine the tag name and CSS classes based on TextStyle and TagName
    var tagName = DetermineTagName();
    var classNames = DetermineClassNames();

    // If we have a bound property, use its value
    var displayText = Text;
    var displayPropertyName = propertyName;

    if (propertyValue != null)
    {
        displayText = !string.IsNullOrEmpty(Format)
            ? string.Format(CultureInfo.CurrentCulture, Format, propertyValue)
            : propertyValue.ToString();
    }

    // Open the element with the determined tag name
    builder.OpenElement(0, tagName);

    // Add style attribute if provided
    if (!string.IsNullOrEmpty(Style))
    {
        builder.AddAttribute(1, "style", Style);
    }

    // Add class attribute with all calculated classes
    builder.AddAttribute(2, "class", classNames);

    // Add any additional attributes passed to the component
    if (Attributes != null)
    {
        builder.AddMultipleAttributes(3, Attributes);
    }

    // Add the content - either formatted property+value, text, or child content
    if (ShowPropertyName && !string.IsNullOrEmpty(displayPropertyName) && !string.IsNullOrEmpty(displayText))
    {
        // Create property name with bold formatting
        builder.OpenElement(4, "span");
        builder.AddAttribute(5, "class", "innovative-text-property-name");
        builder.AddAttribute(6, "style", "font-weight: bold;");
        builder.AddContent(7, displayPropertyName);
        builder.CloseElement(); // Close property name span

        // Add separator
        builder.AddContent(8, ": ");

        // Add value
        builder.AddContent(9, displayText);
    }
    else if (!string.IsNullOrEmpty(displayText))
    {
        builder.AddContent(10, displayText);
    }
    else if (ChildContent != null)
    {
        builder.AddContent(11, ChildContent);
    }

    // Add anchor if specified
    if (!string.IsNullOrEmpty(Anchor))
    {
        builder.OpenElement(12, "a");
        builder.AddAttribute(13, "id", Anchor);
        builder.AddAttribute(14, "href", $"#{Anchor}");
        builder.AddAttribute(15, "class", "innovative-link");
        builder.CloseElement();
    }

    // Close the main element
    builder.CloseElement();
}
    private string DetermineTagName()
    {
        // First check if TagName is explicitly set (and not Auto)
        if (TagName != TagName.Auto)
        {
            return TagName.ToString().ToUpperInvariant();
        }

        // Otherwise determine tag based on TextStyle
        return TextStyle switch
        {
            TextStyle.DisplayH1 or TextStyle.H1 => "h1",
            TextStyle.DisplayH2 or TextStyle.H2 => "h2",
            TextStyle.DisplayH3 or TextStyle.H3 => "h3",
            TextStyle.DisplayH4 or TextStyle.H4 => "h4",
            TextStyle.DisplayH5 or TextStyle.H5 => "h5",
            TextStyle.DisplayH6 or TextStyle.H6 or TextStyle.Subtitle1 or TextStyle.Subtitle2 => "h6",
            TextStyle.Body1 or TextStyle.Body2 => "p",
            _ => "span"
        };
    }

    private string DetermineClassNames()
    {
        var classes = new List<string>();

        // Add TextStyle-specific class
        var styleClass = TextStyle switch
        {
            TextStyle.DisplayH1 => "innovative-text-display-h1",
            TextStyle.DisplayH2 => "innovative-text-display-h2",
            TextStyle.DisplayH3 => "innovative-text-display-h3",
            TextStyle.DisplayH4 => "innovative-text-display-h4",
            TextStyle.DisplayH5 => "innovative-text-display-h5",
            TextStyle.DisplayH6 => "innovative-text-display-h6",
            TextStyle.H1 => "innovative-text-h1",
            TextStyle.H2 => "innovative-text-h2",
            TextStyle.H3 => "innovative-text-h3",
            TextStyle.H4 => "innovative-text-h4",
            TextStyle.H5 => "innovative-text-h5",
            TextStyle.H6 => "innovative-text-h6",
            TextStyle.Subtitle1 => "innovative-text-subtitle1",
            TextStyle.Subtitle2 => "innovative-text-subtitle2",
            TextStyle.Body1 => "innovative-text-body1",
            TextStyle.Body2 => "innovative-text-body2",
            TextStyle.Button => "innovative-text-button",
            TextStyle.Caption => "innovative-text-caption",
            TextStyle.Overline => "innovative-text-overline",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(styleClass))
        {
            classes.Add(styleClass);
        }

        // Add alignment class if not Left (default)
        if (TextAlign != TextAlign.Left)
        {
            var alignClass = TextAlign switch
            {
                TextAlign.Center => "innovative-text-align-center",
                TextAlign.End => "innovative-text-align-end",
                TextAlign.Justify => "innovative-text-align-justify",
                TextAlign.Start => "innovative-text-align-start",
                TextAlign.Right => "innovative-text-align-right",
                TextAlign.JustifyAll => "innovative-text-align-justify-all",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(alignClass))
            {
                classes.Add(alignClass);
            }
        }

        // Add custom class if specified
        if (!string.IsNullOrEmpty(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                if (notifyObject != null)
                {
                    notifyObject.PropertyChanged -= OnPropertyChanged;
                    notifyObject = null;
                }
            }

            disposed = true;
        }
    }
}

// Put these enums in their own files or nest them inside the namespace
public enum TextStyle
{
    DisplayH1,
    DisplayH2,
    DisplayH3,
    DisplayH4,
    DisplayH5,
    DisplayH6,
    H1,
    H2,
    H3,
    H4,
    H5,
    H6,
    Subtitle1,
    Subtitle2,
    Body1,
    Body2,
    Button,
    Caption,
    Overline
}

public enum TextAlign
{
    Left,
    Center,
    Right,
    Start,
    End,
    Justify,
    JustifyAll
}

public enum TagName
{
    Div,
    Span,
    P,
    H1,
    H2,
    H3,
    H4,
    H5,
    H6,
    A,
    Button,
    Pre,
    Auto
}
