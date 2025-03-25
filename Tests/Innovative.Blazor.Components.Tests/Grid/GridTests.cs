using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Innovative.Blazor.Components.Components.Grid;
using Innovative.Blazor.Components.Localizer;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Radzen;

namespace Innovative.Blazor.Components.Tests.Grid;

public class GridTests : TestContext
{
    private readonly Mock<IInnovativeStringLocalizerFactory> _localizerFactoryMock;
    private readonly Mock<IInnovativeStringLocalizer> _localizerMock;
    private readonly Mock<ILogger<InnovativeGrid<TestModel>>> _loggerMock;

    public GridTests()
    {
        _loggerMock = new Mock<ILogger<InnovativeGrid<TestModel>>>();
        _localizerMock = new Mock<IInnovativeStringLocalizer>();
        _localizerFactoryMock = new Mock<IInnovativeStringLocalizerFactory>();

        _localizerFactoryMock
            .Setup(f => f.Create(It.IsAny<Type>()))
            .Returns(_localizerMock.Object);

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
        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            await gridInstance.ApplyFilter(filterColumnName, filterValue, op);
        });

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
    var testData = GetTestData();
    var invalidColumnName = "NonExistentColumn";

    var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    _loggerMock.Reset();

    await cut.InvokeAsync(async () =>
    {
        await cut.Instance.ApplyFilter(invalidColumnName, "Value", FilterOperator.Equals);
    });

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
    var testData = GetTestData();
    var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

    _loggerMock.Reset();

    await cut.InvokeAsync(async () =>
    {
        await cut.Instance.ClearFilter();
    });

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
            Assert.Contains("test", markup); 
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

        var cutMax = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.MinHeightOption, GridHeight.Max)
        );

        Assert.DoesNotContain("--min-height: 1162px", cutMinimal.Markup);
        Assert.Contains("--max-height: 1162px", cutMinimal.Markup);

        Assert.Contains("--min-height: 1162px", cutMax.Markup);
        Assert.Contains("--max-height: 1162px", cutMax.Markup);
    }

    [Fact]
    public void Grid_Uses_Localizer_From_Factory()
    {
        var testData = GetTestData();
        var testKey = "TestKey";
        LocalizedString testValue = new LocalizedString(testKey, "Test Value");
        
        _localizerMock.Setup(l => l[testKey]).Returns(testValue);

        var cut = RenderComponent<InnovativeGrid<TestModel>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        _localizerFactoryMock.Verify(f => f.Create(It.IsAny<Type>()), Times.Once);
    }


    [Fact]
public void Grid_Respects_UIGridClass_AllowSorting()
{
    var testData = new List<TestModelWithGridClass>
    {
        new TestModelWithGridClass { TestProperty = "Value1", AnotherProperty = "Another1" },
        new TestModelWithGridClass { TestProperty = "Value2", AnotherProperty = "Another2" }
    };

    var cut = RenderComponent<InnovativeGrid<TestModelWithGridClass>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

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
    var testData = new List<TestModelWithGridClass>
    {
        new TestModelWithGridClass { TestProperty = "Value1" }
    };

    var cut = RenderComponent<InnovativeGrid<TestModelWithGridClass>>(parameters => parameters
        .Add(p => p.Data, testData)
    );

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



public class TestResources {}
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