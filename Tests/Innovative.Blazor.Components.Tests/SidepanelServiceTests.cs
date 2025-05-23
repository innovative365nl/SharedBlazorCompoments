#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Services;
using Innovative.Blazor.Components.Tests.TestBase;
using Microsoft.AspNetCore.Components;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Innovative.Blazor.Components.Tests;

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class SidepanelServiceTests : TestBase.TestBaseService
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly SidepanelService _sidepanelService;

    public SidepanelServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _sidepanelService = new SidepanelService();
    }

    [Fact]
    public void InitialState_ShouldBeEmpty()
    {
        // Assert
        _sidepanelService.IsVisible.Should().BeFalse();
        _sidepanelService.CurrentComponentType.Should().BeNull();
        _sidepanelService.CurrentParameters.Should().BeNull();
        _sidepanelService.CurrentOptions.Should().BeNull();
    }

    [Fact]
    public async Task OpenSidepanel_ShouldSetCorrectProperties()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { { "Param1", "Value1" } };
        var options = new SidepanelOptions
        {
            Title = "Test Panel",
            Width = "500px",
            SideDialogWidth = SideDialogWidth.Large
        };

        // Act
        var openTask = _sidepanelService.OpenSidepanelAsync<TestComponent>(parameters, options);

        // Assert
        _sidepanelService.IsVisible.Should().BeTrue();
        _sidepanelService.CurrentComponentType.Should().Be<TestComponent>();
        _sidepanelService.CurrentParameters.Should().BeSameAs(parameters);
        _sidepanelService.CurrentOptions.Should().BeSameAs(options);

        // Cleanup - complete the task
        _sidepanelService.CloseSidepanel();
        await openTask.ConfigureAwait(true);
    }

    [Fact]
    public void OpenSidepanel_WhenAlreadyOpen_ShouldThrowException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();
        var options = new SidepanelOptions();

        // Open the first panel
        var _ = _sidepanelService.OpenSidepanelAsync<TestComponent>(parameters, options);

        // Act & Assert
        var action = () => _sidepanelService.OpenSidepanelAsync<TestComponent>(parameters, options);
        action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A sidepanel is already open.");

        // Cleanup
        _sidepanelService.CloseSidepanel();
    }

    [Fact]
    public async Task CloseSidepanel_ShouldResetProperties()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();
        var options = new SidepanelOptions();
        var openTask = _sidepanelService.OpenSidepanelAsync<TestComponent>(parameters, options);

        // Act
        _sidepanelService.CloseSidepanel();

        // Assert
        _sidepanelService.IsVisible.Should().BeFalse();
        _sidepanelService.CurrentComponentType.Should().BeNull();
        _sidepanelService.CurrentParameters.Should().BeNull();
        _sidepanelService.CurrentOptions.Should().BeNull();

        // The task should complete
        var result = await openTask.ConfigureAwait(true);
        result.Should().BeNull();
    }

    [Fact]
    public async Task CloseSidepanel_WithResult_ShouldReturnResult()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();
        var options = new SidepanelOptions();
        var expectedResult = new { Success = true };
        var openTask = _sidepanelService.OpenSidepanelAsync<TestComponent>(parameters, options);

        // Act
        _sidepanelService.CloseSidepanel(expectedResult);

        // Assert
        var result = await openTask.ConfigureAwait(true);
        result.Should().BeSameAs(expectedResult);
    }

    [Fact]
    public void CloseSidepanel_WhenNotOpen_ShouldDoNothing()
    {
        // Act
        _sidepanelService.CloseSidepanel();

        // Assert
        _sidepanelService.IsVisible.Should().BeFalse();
        _sidepanelService.CurrentComponentType.Should().BeNull();
        _sidepanelService.CurrentParameters.Should().BeNull();
        _sidepanelService.CurrentOptions.Should().BeNull();
    }

    [Fact]
    public void StateChanged_ShouldFireWhenOpeningAndClosing()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();
        var options = new SidepanelOptions();
        int eventCount = 0;
        _sidepanelService.OnStateChanged += () => eventCount++;

        // Act - Open
        var openTask = _sidepanelService.OpenSidepanelAsync<TestComponent>(parameters, options);

        // Assert
        eventCount.Should().Be(1);

        // Act - Close
        _sidepanelService.CloseSidepanel();

        // Assert
        eventCount.Should().Be(2);
    }

    // Test component for the sidepanel
    private sealed class TestComponent : ComponentBase
    {
    }
}
