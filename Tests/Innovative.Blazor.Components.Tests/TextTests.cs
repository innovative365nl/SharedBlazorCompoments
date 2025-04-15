using System;
using Moq;
using System.ComponentModel;
using System.Threading.Tasks;
using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Localizer;

namespace Innovative.Blazor.Components.Tests;

public class TextTests : TestContext
{
    public TextTests()
    {
        var mockLocalizerFactory = new Mock<IInnovativeStringLocalizerFactory>();
        Services.AddSingleton(mockLocalizerFactory.Object);
    }
    
    [Fact]
    public void InnovativeTextRendersWithTextParameter()
    {
        // Arrange
        var text = "Hello World";

        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.Text, text));

        // Assert
        cut.MarkupMatches($"<p class=\"innovative-text-body1\">{text}</p>");
    }

    [Fact]
    public void InnovativeTextRendersWithChildContent()
    {
        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .AddChildContent("<span>Child Content</span>"));

        // Assert
        cut.MarkupMatches("<p class=\"innovative-text-body1\"><span>Child Content</span></p>");
    }

    [Fact]
    public void InnovativeTextRendersWithCustomStyle()
    {
        // Arrange
        var style = "color: red; font-size: 18px;";

        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.Style, style)
            .Add(p => p.Text, "Styled Text"));

        // Assert
        cut.MarkupMatches($"<p class=\"innovative-text-body1\" style=\"{style}\">Styled Text</p>");
    }

    [Fact]
    public void InnovativeTextRendersWithCustomClass()
    {
        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.Class, "custom-class")
            .Add(p => p.Text, "Classy Text"));

        // Assert
        cut.MarkupMatches("<p class=\"innovative-text-body1 custom-class\">Classy Text</p>");
    }

    [Fact]
    public void InnovativeTextRendersWithCorrectHeadingTag()
    {
        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.TextStyle, TextStyle.H1)
            .Add(p => p.Text, "Heading 1"));

        // Assert
        cut.MarkupMatches("<h1 class=\"innovative-text-h1\">Heading 1</h1>");
    }

    [Fact]
    public void InnovativeTextRendersWithExplicitTagName()
    {
        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.TagName, TagName.Span)
            .Add(p => p.Text, "Span Text"));

        // Assert
        cut.MarkupMatches("<span class=\"innovative-text-body1\">Span Text</span>");
    }

    [Fact]
    public void InnovativeTextRendersWithTextAlign()
    {
        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.TextAlign, TextAlign.Center)
            .Add(p => p.Text, "Centered Text"));

        // Assert
        cut.MarkupMatches("<p class=\"innovative-text-body1 innovative-text-align-center\">Centered Text</p>");
    }

    [Fact]
    public void InnovativeTextRendersWithAnchor()
    {
        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.Anchor, "section1")
            .Add(p => p.Text, "Section 1"));

        // Assert
        cut.MarkupMatches(@"
            <p class=""innovative-text-body1"">
                Section 1
                <a id=""section1"" href=""#section1"" class=""innovative-link""></a>
            </p>
        ");
    }

    [Fact]
    public async Task InnovativeTextBindsToPropertyValue()
    {
        // Arrange
        var testObject = new TestNotifyObject { Name = "Initial Value" };

        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.For, testObject)
            .Add(p => p.Property, nameof(TestNotifyObject.Name)));

        // Assert
        cut.MarkupMatches("<p class=\"innovative-text-body1\">Initial Value</p>");

        // Update the property using the dispatcher
        await cut.InvokeAsync(() => {
            testObject.Name = "Updated Value";
        });

        // Verify the component updated
        cut.MarkupMatches("<p class=\"innovative-text-body1\">Updated Value</p>");
    }

    [Fact]
    public void InnovativeTextFormatsPropertyValue()
    {
        // Arrange
        var testObject = new TestNotifyObject { Amount = 123.45 };

        // Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.For, testObject)
            .Add(p => p.Property, nameof(TestNotifyObject.Amount))
            .Add(p => p.Format, "{0:C}"));

        // Assert - exact format will depend on current culture, so using Contains
        var html = cut.Markup;
        Assert.Contains("123", html, StringComparison.Ordinal);
        Assert.Contains("45", html, StringComparison.Ordinal);
    }
    [Theory]
    [InlineData(TextStyle.Body1, TextAlign.Left, null, "innovative-text-body1")]
    [InlineData(TextStyle.H1, TextAlign.Left, null, "innovative-text-h1")]
    [InlineData(TextStyle.Body2, TextAlign.Center, null, "innovative-text-body2 innovative-text-align-center")]
    [InlineData(TextStyle.Subtitle1, TextAlign.Right, null, "innovative-text-subtitle1 innovative-text-align-right")]
    [InlineData(TextStyle.DisplayH1, TextAlign.Justify, null, "innovative-text-display-h1 innovative-text-align-justify")]
    [InlineData(TextStyle.Caption, TextAlign.End, null, "innovative-text-caption innovative-text-align-end")]
    [InlineData(TextStyle.H3, TextAlign.Left, "custom-class", "innovative-text-h3 custom-class")]
    [InlineData(TextStyle.Overline, TextAlign.Center, "my-class extra-class", "innovative-text-overline innovative-text-align-center my-class extra-class")]
    public void InnovativeTextDeterminesClassNamesCorrectly(TextStyle textStyle, TextAlign textAlign, string? customClass, string expectedClasses)
    {
        // Arrange & Act
        var cut = RenderComponent<InnovativeText>(parameters => parameters
            .Add(p => p.TextStyle, textStyle)
            .Add(p => p.TextAlign, textAlign)
            .Add(p => p.Class, customClass)
            .Add(p => p.Text, "Test Text"));
    
        // Assert
        cut.MarkupMatches($"<{GetExpectedTagForTextStyle(textStyle)} class=\"{expectedClasses}\">Test Text</{GetExpectedTagForTextStyle(textStyle)}>");
    }
    
    private static string GetExpectedTagForTextStyle(TextStyle style) => style switch
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
    

    private sealed class TestNotifyObject : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private double _amount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
                }
            }
        }

        public double Amount
        {
            get => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }
    }
}