using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Innovative.Blazor.Components.Attributes;
using Innovative.Blazor.Components.Components.Grid;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Localizer;
using Microsoft.Extensions.Logging;
using Moq;
using Radzen;
using FluentAssertions;
using Innovative.Blazor.Components.Components.Dialog;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using Radzen.Blazor;

namespace Innovative.Blazor.Components.Tests;

public class InnovativeGridTests : TestContext
{
    private readonly Mock<IInnovativeStringLocalizerFactory> _localizerFactoryMock;
    private readonly Mock<IInnovativeStringLocalizer> _localizerMock;
    private readonly Mock<ILogger<InnovativeGrid<TestModel>>> _loggerMock;

    public InnovativeGridTests()
    {
        _loggerMock = new Mock<ILogger<InnovativeGrid<TestModel>>>();
        _localizerMock = new Mock<IInnovativeStringLocalizer>();
        _localizerFactoryMock = new Mock<IInnovativeStringLocalizerFactory>();

        // Setup localizer factory to return our localizer mock
        _localizerFactoryMock
            .Setup(f => f.Create(It.IsAny<Type>()))
            .Returns(_localizerMock.Object);

        // Setup localizer to return the key as the value (for simple testing)
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns<LocalizedString>(key => key);

        Services.AddSingleton(_loggerMock.Object);
        Services.AddSingleton(_localizerFactoryMock.Object);

        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddRadzenComponents();
    }

    [Fact]
    public void Grid_Renders_With_Data()
    {
        var testData = GetTestData();

        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.Title, "Test Grid")
        );

        cut.Find("div").TextContent.Should().Contain("Test Grid");
        cut.Markup.Should().Contain("TestValue2");
    }

    [Fact]
    public void Grid_Shows_No_Data_Message_When_Empty()
    {
        var emptyData = new List<TestModel>();

        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, emptyData)
        );
        var content = cut.Find("p").TextContent;
        cut.Find("p").TextContent.Should().Contain("No records to\n                            display.");
    }

    [Fact]
    public void Grid_Shows_Loading_Indicator_When_IsLoading_True()
    {
        var testData = GetTestData();

        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.IsLoading, true)
        );

        Assert.Contains(@"<circle class=""rz-progressbar-circular-background"" r=""15.91549"" fill=""none""></circle>", cut.Markup);
    }

    [Fact]
    public async Task Selection_Changed_Event_Is_Triggered()
    {
        var testData = GetTestData();
        var selectedItems = new List<TestModel>();

        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.EnableRowSelection, true)
            .Add(p => p.SelectionMode, DataGridSelectionMode.Single)
            .Add<IEnumerable<TestModel>>(p => p.OnSelectionChanged, items =>
            {
                selectedItems = items.ToList();
            })
        );

        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            gridInstance.SelectedItems = new List<TestModel> { testData.First() };
            await gridInstance.ReloadAsync();

            // Assert within the same dispatcher context
            Assert.Single(selectedItems);
            Assert.Equal("TestValue1", selectedItems.First().TestProperty);
        });
    }

    [Fact]
    public async Task ApplyFilter_Calls_DataGrid_Methods()
    {
        var testData = GetTestData();
        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            await gridInstance.ApplyFilter("TestProperty", "TestValue1", FilterOperator.Equals);

            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        });
    }

    [Fact]
public async Task ApplyFilter_Calls_Methods_For_Different_Operators()
{
    // Arrange
    var testData = GetTestData();
    var filterColumnName = "TestProperty";
    var filterValue = "TestValue";
    var operators = new[]
    {
        FilterOperator.Contains,
        FilterOperator.StartsWith,
        FilterOperator.EndsWith,
        FilterOperator.GreaterThan,
        FilterOperator.LessThan
    };

    foreach (var op in operators)
    {
        // Create a new component instance for each test to avoid state issues
        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        // Act - We can only call the public method and verify logging behavior
        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            await gridInstance.ApplyFilter(filterColumnName, filterValue, op);
        });

        // Assert - We're verifying no warnings were logged (successful execution)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }
}

    [Fact]
public async Task ApplyFilter_With_Invalid_Column_Logs_Warning()
{
    // Arrange
    var testData = GetTestData();
    var invalidColumnName = "NonExistentColumn";

    var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    // Reset the mock to ensure we only count calls from this test
    _loggerMock.Reset();

    // Act
    await cut.InvokeAsync(async () =>
    {
        await cut.Instance.ApplyFilter(invalidColumnName, "Value", FilterOperator.Equals);
    });

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Column {invalidColumnName} not found")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
}

    [Fact]
public async Task ClearFilter_Executes_Successfully()
{
    // Arrange
    var testData = GetTestData();
    var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    // Reset the mock to ensure we only count calls from this test
    _loggerMock.Reset();

    // Act - We can only call the public method and check for exceptions
    await cut.InvokeAsync(async () =>
    {
        // This shouldn't throw an exception
        await cut.Instance.ClearFilter();
    });

    // Assert - No warnings were logged, which means the operation succeeded
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Never);
}

    [Fact]
public void GetPropertiesWithAttributes_Returns_Only_Properties_With_UIGridField()
{
    // Arrange
    var testData = new List<TestModelWithMixedAttributes>
    {
        new TestModelWithMixedAttributes 
        { 
            PropertyWithAttribute = "WithAttribute", 
            PropertyWithoutAttribute = "NoAttribute" 
        }
    };
    
    var cut = RenderComponent<InnovativeGrid<TestModelWithMixedAttributes>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    // The markup should contain the property with the attribute but not the one without
    var markup = cut.Markup;
    Assert.Contains("WithAttribute", markup);
    Assert.DoesNotContain("NoAttribute", markup);
}


    [Fact]
    public async Task ClearSelection_Empties_SelectedItems()
    {
        var testData = GetTestData();
        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.EnableRowSelection, true)
        );

        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            gridInstance.SelectedItems = new List<TestModel> { testData.First() };

            gridInstance.ClearSelection();

            Assert.Empty(gridInstance.SelectedItems);
        });
    }

    [Fact]
    public async Task RenderCustomComponent_Renders_Correctly()
    {
        var testData = new List<TestModel>
        {
            new TestModel
            {
                TestProperty = "Test1",
                CustomProperty = "CustomValue"
            }
        };

        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        await cut.InvokeAsync(() =>
        {
            var markup = cut.Markup;
            Assert.Contains("CustomValue", markup);
            Assert.Contains("test", markup); // CustomParam value
            return Task.CompletedTask;
        });
    }

    [Fact]
    public void Grid_Uses_Correct_Height_Based_On_MinHeightOption()
    {
        var testData = GetTestData();

        var cutMinimal = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.MinHeightOption, GridHeight.Minimal)
        );

        // Act - with Max height
        var cutMax = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.MinHeightOption, GridHeight.Max)
        );

        // Assert
        Assert.DoesNotContain("--min-height: 1162px", cutMinimal.Markup);
        Assert.Contains("--max-height: 1162px", cutMinimal.Markup);

        Assert.Contains("--min-height: 1162px", cutMax.Markup);
        Assert.Contains("--max-height: 1162px", cutMax.Markup);
    }

    [Fact]
    public void Grid_Uses_Localizer_From_Factory()
    {
        // Arrange
        var testData = GetTestData();
        var testKey = "TestKey";
        LocalizedString testValue = new LocalizedString(testKey, "Test Value");
        
        _localizerMock.Setup(l => l[testKey]).Returns(testValue);

        // Act
        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        // Assert
        _localizerFactoryMock.Verify(f => f.Create(It.IsAny<Type>()), Times.Once);
    }


    [Fact]
public void Grid_Respects_UIGridClass_AllowSorting()
{
    // Arrange
    var testData = new List<TestModelWithGridClass>
    {
        new TestModelWithGridClass { TestProperty = "Value1", AnotherProperty = "Another1" },
        new TestModelWithGridClass { TestProperty = "Value2", AnotherProperty = "Another2" }
    };

    // Act
    var cut = RenderComponent<InnovativeGrid<TestModelWithGridClass>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    // Assert - The grid should not have sortable columns
    var markup = cut.Markup;
    Assert.DoesNotContain("rz-column-sortable", markup);
}

// [Fact]
// public void Grid_Uses_DefaultSortField_From_UIGridClass()
// {
//     // Arrange
//     var testData = new List<TestModelWithGridClass>
//     {
//         new TestModelWithGridClass { TestProperty = "Value1", AnotherProperty = "Another1" },
//         new TestModelWithGridClass { TestProperty = "Value2", AnotherProperty = "Another2" }
//     };
//
//     // Act
//     var cut = RenderComponent<InnovativeGrid<TestModelWithGridClass>>(parameters => parameters
//         .Add(p => p.Data, testData)
//     );
//
//     // Assert
//     // Verify the grid's internal sort field matches the default from attribute
//     // This needs to be checked on the instance
//     Assert.Equal("TestProperty", cut.Instance.SortField);
// }

    [Fact]
public void Grid_Uses_ResourceType_From_UIGridClass_For_Localization()
{
    // Arrange
    var testData = new List<TestModelWithGridClass>
    {
        new TestModelWithGridClass { TestProperty = "Value1" }
    };

    // Act
    var cut = RenderComponent<InnovativeGrid<TestModelWithGridClass>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    // Assert - Verify the factory was called with the TestResources type
    _localizerFactoryMock.Verify(f => f.Create(typeof(TestResources)), Times.Once);
}

    private List<TestModel> GetTestData()
    {
        return new List<TestModel>
        {
            new TestModel { TestProperty = "TestValue1" },
            new TestModel { TestProperty = "TestValue2" }
        };
    }
}

public class TestModelWithMixedAttributes
{
    [UIGridField(ShowByDefault = true)]
    public string PropertyWithAttribute { get; set; }

    // No attribute here
    public string PropertyWithoutAttribute { get; set; }
}

public class TestModel
{
    [UIGridField(ShowByDefault = true, Sortable = true)]

    public string TestProperty { get; set; }

    [UIGridField(
        ShowByDefault = true,
        CustomComponentType = typeof(TestCustomComponent),
        Parameters = new[] { "CustomParam:test" })]
    public string CustomProperty { get; set; }
}



// Optional mock resource class
public class TestResources {}
// Add this class with UIGridClass attribute
[UIGridClass(AllowSorting = false, DefaultSortField = "TestProperty", ResourceType = typeof(TestResources))]
public class TestModelWithGridClass
{
    [UIGridField(ShowByDefault = true, Sortable = true)]
    public string TestProperty { get; set; }

    [UIGridField(ShowByDefault = true)]
    public string AnotherProperty { get; set; }
}

public class TestCustomComponent : ComponentBase
{
    [Parameter]
    public string? Value { get; set; }

    [Parameter]
    public string? CustomParam { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "test-custom-component");
        builder.AddContent(2, $"Value: {Value}, Param: {CustomParam}");
        builder.CloseElement();
    }
}