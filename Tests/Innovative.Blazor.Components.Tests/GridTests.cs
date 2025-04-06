#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Innovative.Blazor.Components.Components.Grid;
using Innovative.Blazor.Components.Tests.TestBase;
using Innovative.Blazor.Components.Tests.TestModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using Radzen;
using static Xunit.Assert;

#endregion

namespace Innovative.Blazor.Components.Tests;

/// <summary>
///     Unit tests for the InnovativeGrid component.
/// </summary>
public class GridTests : LocalizedTestBase
{
    private readonly Mock<ILogger<InnovativeGrid<TestModel>>> _loggerMock;

    /// <summary>
    ///     Setup common testing infrastructure for all grid tests.
    /// </summary>
    public GridTests()
    {
        // Setup mocks for logging
        _loggerMock = new Mock<ILogger<InnovativeGrid<TestModel>>>();

        // Register services
        Services.AddSingleton(_loggerMock.Object);

        // Configure JS runtime and Radzen components
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddRadzenComponents();
    }

    #region Rendering Tests

    /// <summary>
    ///     Tests that the grid renders correctly with supplied data.
    /// </summary>
    [Fact]
    public void GridRendersWithData()
    {
        // Arrange
        var testData = GetTestData();

        // Act
        var cut = RenderGridComponent(testData, title: "Test Grid");

        // Assert
        cut.Find("div").TextContent.Should().Contain("Test Grid");
        cut.Markup.Should().Contain("TestValue2");
    }

    /// <summary>
    ///     Tests that the grid shows a no data message when there's no data.
    /// </summary>
    [Fact]
    public void GridShowsNoDataMessageWhenEmpty()
    {
        // Arrange & Act
        var cut = RenderGridComponent(new List<TestModel>());

        // Assert
        var content = cut.Find("p").TextContent;
        cut.Find("p").TextContent.Should().Contain("No records to\n                            display.");
    }

    /// <summary>
    ///     Tests that a loading indicator appears when the grid is in loading state.
    /// </summary>
    [Fact]
    public void GridShowsLoadingIndicatorWhenIsLoadingTrue()
    {
        // Arrange
        var testData = GetTestData();

        // Act
        var cut = RenderGridComponent(testData, isLoading: true);

        // Assert
        Contains("""<circle class="innovative-progressbar-circular-background" r="15.91549" fill="none""",
            cut.Markup, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Tests that the grid applies different height settings based on MinHeightOption.
    /// </summary>
    [Fact]
    public void GridUsesCorrectHeightBasedOnMinHeightOption()
    {
        // Arrange
        var testData = GetTestData();

        // Act - Minimal height
        var cutMinimal = RenderGridComponent(testData, minHeightOption: GridHeight.Minimal);

        // Act - Maximum height
        var cutMax = RenderGridComponent(testData, minHeightOption: GridHeight.Max);

        // Assert
        DoesNotContain("--min-height: 1162px", cutMinimal.Markup, StringComparison.Ordinal);
        Contains("--max-height: 1162px", cutMinimal.Markup, StringComparison.Ordinal);

        Contains("--min-height: 1162px", cutMax.Markup, StringComparison.Ordinal);
        Contains("--max-height: 1162px", cutMax.Markup, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Tests that custom component renders correctly within the grid.
    /// </summary>
    [Fact]
    public async Task RenderCustomComponentRendersCorrectly()
    {
        // Arrange
        var testData = new List<TestModel>
        {
            new()
            {
                TestProperty = "Test1",
                CustomProperty = "CustomValue"
            }
        };

        // Act
        var cut = RenderGridComponent(testData);

        // Assert
        await cut.InvokeAsync(() =>
        {
            var markup = cut.Markup;
            Contains("CustomValue", markup, StringComparison.Ordinal);
            Contains("test", markup, StringComparison.Ordinal);
            return Task.CompletedTask;
        });
    }

    #endregion

    #region Selection Tests

    /// <summary>
    ///     Tests that the selection changed event is triggered when items are selected.
    /// </summary>
    [Fact]
    public async Task SelectionChangedEventIsTriggered()
    {
        // Arrange
        var testData = GetTestData();
        var selectedItems = new List<TestModel>();

        // Act
        var cut = RenderGridComponent(
            testData,
            enableRowSelection: true,
            selectionMode: DataGridSelectionMode.Single,
            onSelectionChanged: items => { selectedItems = items.ToList(); }
        );

        // Assert
        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            await gridInstance.SetSelectedItemsAsync(new List<TestModel> { testData.First() }).ConfigureAwait(false);
            await gridInstance.ReloadAsync().ConfigureAwait(false);

            Single(selectedItems);
            Equal("TestValue1", selectedItems.First().TestProperty);
        });
    }

    /// <summary>
    ///     Tests that clearing selection empties the selected items collection.
    /// </summary>
    [Fact]
    public async Task ClearSelectionEmptiesSelectedItems()
    {
        // Arrange
        var testData = GetTestData();
        var cut = RenderGridComponent(testData, enableRowSelection: true);

        // Act & Assert
        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            await gridInstance.SetSelectedItemsAsync(new List<TestModel> { testData.First() }).ConfigureAwait(false);

            gridInstance.ClearSelection();

            Empty(gridInstance.SelectedItems);
            return Task.CompletedTask;
        });
    }

    #endregion

    #region Filter Tests

    /// <summary>
    ///     Tests that applying a filter with valid column and operator works correctly.
    /// </summary>
    [Fact]
    public async Task ApplyFilterCallsDataGridMethods()
    {
        // Arrange
        var testData = GetTestData();
        var cut = RenderGridComponent(testData);

        // Act & Assert
        await cut.InvokeAsync(async () =>
        {
            var gridInstance = cut.Instance;
            await gridInstance.ApplyFilter("TestProperty", "TestValue1", FilterOperator.Equals).ConfigureAwait(false);

            // No warning should be logged
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Never);
        });
    }

    /// <summary>
    ///     Tests filter operations with different filter operators.
    /// </summary>
    [Fact]
    public async Task ApplyFilterCallsMethodsForDifferentOperators()
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

        // Test each operator
        foreach (var op in operators)
        {
            // Reset mock and render component for each operator test
            _loggerMock.Reset();
            var cut = RenderGridComponent(testData);

            // Act
            await cut.InvokeAsync(async () =>
            {
                var gridInstance = cut.Instance;
                await gridInstance.ApplyFilter(filterColumnName, filterValue, op).ConfigureAwait(false);
            });

            // Assert - No warnings should be logged
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Never);
        }
    }

    /// <summary>
    ///     Tests that applying a filter with an invalid column logs a warning.
    /// </summary>
    [Fact]
    public async Task ApplyFilterWithInvalidColumnLogsWarning()
    {
        // Arrange
        var testData = GetTestData();
        var invalidColumnName = "NonExistentColumn";
        var cut = RenderGridComponent(testData);

        // Reset mock to ensure clean state
        _loggerMock.Reset();

        // Act
        await cut.InvokeAsync(async () =>
        {
            await cut.Instance.ApplyFilter(invalidColumnName, "Value", FilterOperator.Equals).ConfigureAwait(false);
        });

        // Assert - Warning should be logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Column {invalidColumnName} not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }

    /// <summary>
    ///     Tests that clearing a filter executes without errors.
    /// </summary>
    [Fact]
    public async Task ClearFilterExecutesSuccessfully()
    {
        // Arrange
        var testData = GetTestData();
        var cut = RenderGridComponent(testData);

        // Reset mock
        _loggerMock.Reset();

        // Act
        await cut.InvokeAsync(async () => { await cut.Instance.ClearFilter().ConfigureAwait(false); });

        // Assert - No warnings should be logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Never);
    }

    #endregion

    #region Attribute Tests

    /// <summary>
    ///     Tests that only properties with UIGridField attributes are displayed in the grid.
    /// </summary>
    [Fact]
    public void GetPropertiesWithAttributesReturnsOnlyPropertiesWithUIGridField()
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

        // Act
        var cut = RenderComponent<InnovativeGrid<TestModelWithMixedAttributes>>(parameters => parameters
            .Add(p => p.Data, testData)
        );

        // Assert
        var markup = cut.Markup;
        Contains("WithAttribute", markup, StringComparison.Ordinal);
        DoesNotContain("NoAttribute", markup, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Tests that the grid respects the AllowSorting property from UIGridClass attribute.
    /// </summary>
    [Fact]
    public void GridRespectsUIGridClassAllowSorting()
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

        // Assert - Ensure no sortable columns when AllowSorting is false
        var markup = cut.Markup;
        DoesNotContain("rz-column-sortable", markup, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Tests that the grid uses the ResourceType from UIGridClass for localization.
    /// </summary>
    [Fact]
    public void GridUsesResourceTypeFromUIGridClassForLocalization()
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

        // Assert - Verify that localizer factory used the correct resource type
        LocalizerFactoryMock.Verify(f => f.Create(typeof(TestResources)), Times.Once);
    }

    /// <summary>
    ///     Tests that the grid uses the localizer from the factory.
    /// </summary>
    [Fact]
    public void GridUsesLocalizerFromFactory()
    {
        // Arrange
        var testData = GetTestData();
        var testKey = "TestKey";
        LocalizedString testValue = new LocalizedString(testKey, "Test Value");

        LocalizerMock.Setup(l => l[testKey]).Returns(testValue);

        // Act
        var cut = RenderGridComponent(testData);

        // Assert
        LocalizerFactoryMock.Verify(f => f.Create(It.IsAny<Type>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    ///     Creates a standard test data set.
    /// </summary>
    private static List<TestModel> GetTestData()
    {
        return new List<TestModel>
        {
            new TestModel { TestProperty = "TestValue1" },
            new TestModel { TestProperty = "TestValue2" }
        };
    }

    /// <summary>
    ///     Helper method to render a grid component with common parameters to reduce code duplication.
    /// </summary>
    private IRenderedComponent<InnovativeGrid<TestModel>> RenderGridComponent(
        IEnumerable<TestModel> data,
        string? title = null,
        bool isLoading = false,
        bool enableRowSelection = false,
        DataGridSelectionMode selectionMode = DataGridSelectionMode.Single,
        Action<IEnumerable<TestModel>>? onSelectionChanged = null,
        GridHeight minHeightOption = GridHeight.Minimal)
    {
        return RenderComponent<InnovativeGrid<TestModel>>(parameters =>
        {
            parameters.Add(p => p.Data, data);

            if (title != null)
                parameters.Add(p => p.Title, title);

            if (isLoading)
                parameters.Add(p => p.IsLoading, true);

            if (enableRowSelection)
                parameters.Add(p => p.EnableRowSelection, true);

            parameters.Add(p => p.SelectionMode, selectionMode);

            if (onSelectionChanged != null)
                parameters.Add<IEnumerable<TestModel>>(p => p.OnSelectionChanged, onSelectionChanged);

            parameters.Add(p => p.MinHeightOption, minHeightOption);
        });
    }

    #endregion
}

/// <summary>
///     Custom component for testing rendering of custom components in the grid.
/// </summary>
internal sealed class TestCustomComponent : ComponentBase
{
    [Parameter] public string? Value { get; set; }

    [Parameter] public string? CustomParam { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "class", "test-custom-component");
        builder.AddContent(2, $"Value: {Value}, Param: {CustomParam}");
        builder.CloseElement();
    }
}