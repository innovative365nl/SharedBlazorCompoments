using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Innovative.Blazor.Components.Tests;

/// <summary>
/// Unit tests for the InnovativeDropdown component.
/// </summary>
public class InnovativeDropdownTests : TestContext
{
    public InnovativeDropdownTests()
    {
        Services.AddSingleton<NavigationManager>(new MockNavigationManager());
    }

    [Fact]
    public void InnovativeDropdown_RendersCorrectly()
    {
        // Arrange
        var testData = GetTestData();

        // Act
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Placeholder, "Select an item"));

        // Assert
        Assert.Contains("innovative-dropdown", cut.Markup);
        Assert.Contains("Select an item", cut.Markup);
    }

    [Fact]
    public void InnovativeDropdown_DisplaysSelectedValue()
    {
        // Arrange
        var testData = GetTestData();
        var selectedValue = "value1";

        // Act
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Value, selectedValue));

        // Assert
        Assert.Contains("Item 1", cut.Markup);
    }

    [Fact]
    public async Task InnovativeDropdown_OpensOnClick()
    {
        // Arrange
        var testData = GetTestData();
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value)));

        // Act
        var dropdown = cut.Find(".innovative-dropdown");
        await dropdown.ClickAsync();

        // Assert
        Assert.Contains("innovative-dropdown-open", cut.Markup);
        Assert.Contains("innovative-dropdown-panel", cut.Markup);
    }

    [Fact]
    public async Task InnovativeDropdown_SelectsItemOnClick()
    {
        // Arrange
        var testData = GetTestData();
        string? selectedValue = null;
        
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => selectedValue = value)));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        await dropdown.ClickAsync();

        // Act - Select first item
        var firstItem = cut.Find(".innovative-dropdown-item");
        await firstItem.ClickAsync();

        // Assert
        Assert.Equal("value1", selectedValue);
        Assert.Contains("Item 1", cut.Markup);
    }

    [Fact]
    public async Task InnovativeDropdown_MultipleSelection_WorksCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        List<string>? selectedValues = null;
        
        var cut = RenderComponent<InnovativeDropdown<List<string>>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Multiple, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<List<string>>(this, values => selectedValues = values)));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        await dropdown.ClickAsync();

        // Act - Select multiple items
        var items = cut.FindAll(".innovative-dropdown-item");
        await items[0].ClickAsync();
        await items[1].ClickAsync();

        // Assert
        Assert.NotNull(selectedValues);
        Assert.Equal(2, selectedValues.Count);
        Assert.Contains("value1", selectedValues);
        Assert.Contains("value2", selectedValues);
    }

    [Fact]
    public async Task InnovativeDropdown_FilteringWorksCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.AllowFiltering, true));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        await dropdown.ClickAsync();

        // Act - Filter items
        var filterInput = cut.Find(".innovative-dropdown-filter-input");
        await filterInput.ChangeAsync(new ChangeEventArgs { Value = "Item 1" });

        // Assert
        var items = cut.FindAll(".innovative-dropdown-item");
        Assert.Single(items);
        Assert.Contains("Item 1", items[0].TextContent);
    }

    [Fact]
    public async Task InnovativeDropdown_ClearFunctionality_WorksCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        string? selectedValue = "value1";
        
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Value, selectedValue)
            .Add(p => p.AllowClear, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => selectedValue = value)));

        // Act - Click clear button
        var clearButton = cut.Find(".innovative-dropdown-clear");
        await clearButton.ClickAsync();

        // Assert
        Assert.Null(selectedValue);
    }

    [Fact]
    public async Task InnovativeDropdown_KeyboardNavigation_WorksCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value)));

        var dropdown = cut.Find(".innovative-dropdown");

        // Act - Open with Enter key
        await dropdown.TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "Enter" });

        // Assert dropdown is open
        Assert.Contains("innovative-dropdown-open", cut.Markup);

        // Act - Navigate with Arrow Down
        await dropdown.TriggerEventAsync("onkeydown", new KeyboardEventArgs { Key = "ArrowDown" });

        // Assert first item is highlighted
        Assert.Contains("innovative-dropdown-item-highlighted", cut.Markup);
    }

    [Fact]
    public void InnovativeDropdown_DisabledState_PreventsInteraction()
    {
        // Arrange & Act
        var testData = GetTestData();
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Disabled, true));

        // Assert
        Assert.Contains("innovative-dropdown-disabled", cut.Markup);
    }

    [Fact]
    public void InnovativeDropdown_ChipsMode_DisplaysCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        var selectedValues = new List<string> { "value1", "value2" };
        
        // Act
        var cut = RenderComponent<InnovativeDropdown<List<string>>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Multiple, true)
            .Add(p => p.Chips, true)
            .Add(p => p.Value, selectedValues));

        // Assert
        Assert.Contains("innovative-dropdown-chips", cut.Markup);
        Assert.Contains("innovative-chip", cut.Markup);
        Assert.Contains("Item 1", cut.Markup);
        Assert.Contains("Item 2", cut.Markup);
    }

    [Fact]
    public async Task InnovativeDropdown_SelectAll_WorksCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        List<string>? selectedValues = null;
        
        var cut = RenderComponent<InnovativeDropdown<List<string>>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Multiple, true)
            .Add(p => p.AllowSelectAll, true)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<List<string>>(this, values => selectedValues = values)));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        await dropdown.ClickAsync();

        // Act - Click select all checkbox
        var selectAllCheckbox = cut.Find(".innovative-dropdown-select-all input[type='checkbox']");
        await selectAllCheckbox.ChangeAsync(new ChangeEventArgs { Value = true });

        // Assert
        Assert.NotNull(selectedValues);
        Assert.Equal(3, selectedValues.Count);
    }

    [Fact]
    public void InnovativeDropdown_CustomValueTemplate_RendersCorrectly()
    {
        // Arrange
        var testData = GetTestData();
        var selectedValue = "value1";
        
        RenderFragment<object> valueTemplate = item => builder =>
        {
            builder.OpenElement(0, "strong");
            builder.AddContent(1, $"Custom: {((TestItem)item).Name}");
            builder.CloseElement();
        };

        // Act
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Value, selectedValue)
            .Add(p => p.ValueTemplate, valueTemplate));

        // Assert
        Assert.Contains("<strong>Custom: Item 1</strong>", cut.Markup);
    }

    [Fact]
    public void InnovativeDropdown_EmptyTemplate_DisplaysWhenNoData()
    {
        // Arrange
        RenderFragment emptyTemplate = builder =>
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "No data available");
            builder.CloseElement();
        };

        // Act
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, new List<TestItem>())
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.EmptyTemplate, emptyTemplate));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        dropdown.Click();

        // Assert
        Assert.Contains("No data available", cut.Markup);
    }

    [Fact]
    public void InnovativeDropdown_DisabledItems_AreNotSelectable()
    {
        // Arrange
        var testData = new List<TestItem>
        {
            new() { Name = "Item 1", Value = "value1", Disabled = false },
            new() { Name = "Item 2", Value = "value2", Disabled = true },
            new() { Name = "Item 3", Value = "value3", Disabled = false }
        };

        // Act
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.DisabledProperty, nameof(TestItem.Disabled)));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        dropdown.Click();

        // Assert
        var disabledItem = cut.FindAll(".innovative-dropdown-item")[1];
        Assert.Contains("innovative-dropdown-item-disabled", disabledItem.GetAttribute("class"));
    }

    [Fact]
    public async Task InnovativeDropdown_ChangeEvent_IsTriggered()
    {
        // Arrange
        var testData = GetTestData();
        string? changedValue = null;
        
        var cut = RenderComponent<InnovativeDropdown<string>>(parameters => parameters
            .Add(p => p.Data, testData)
            .Add(p => p.TextProperty, nameof(TestItem.Name))
            .Add(p => p.ValueProperty, nameof(TestItem.Value))
            .Add(p => p.Change, EventCallback.Factory.Create<string>(this, value => changedValue = value)));

        // Open dropdown
        var dropdown = cut.Find(".innovative-dropdown");
        await dropdown.ClickAsync();

        // Act - Select first item
        var firstItem = cut.Find(".innovative-dropdown-item");
        await firstItem.ClickAsync();

        // Assert
        Assert.Equal("value1", changedValue);
    }

    /// <summary>
    /// Test data model.
    /// </summary>
    private sealed class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool Disabled { get; set; }
    }

    /// <summary>
    /// Gets test data for the dropdown.
    /// </summary>
    private static List<TestItem> GetTestData()
    {
        return new List<TestItem>
        {
            new() { Name = "Item 1", Value = "value1", Disabled = false },
            new() { Name = "Item 2", Value = "value2", Disabled = false },
            new() { Name = "Item 3", Value = "value3", Disabled = false }
        };
    }

    /// <summary>
    /// Mock NavigationManager for testing.
    /// </summary>
    private sealed class MockNavigationManager : NavigationManager
    {
        public MockNavigationManager() : base() =>
            Initialize("https://localhost/", "https://localhost/");
    }
}