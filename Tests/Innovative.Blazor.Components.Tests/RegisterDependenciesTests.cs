#region

using System;
using Innovative.Blazor.Components.Common.Composer;
using Innovative.Blazor.Components.Localizer;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;

#endregion

namespace Innovative.Blazor.Components.Tests;

public class RegisterDependenciesTests : TestContext
{
    [Fact]
    public void RegisterInnovativeComponentsShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Create a mock NavigationManager since it's required by RadzenDialogService
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        var mockIJSRuntime = new Mock<IJSRuntime>();
        var InnovativeStringLocalizerFactoryMock = new Mock<IInnovativeStringLocalizerFactory>();
        services.AddSingleton<NavigationManager>(navigationManager);
        services.AddSingleton<IJSRuntime>(mockIJSRuntime.Object);
        services.AddSingleton<IInnovativeStringLocalizerFactory>(InnovativeStringLocalizerFactoryMock.Object);


        // Act
        services.RegisterInnovativeComponents();
        var provider = services.BuildServiceProvider();

        // Assert
        var dialogService = provider.GetService<IInnovativeDialogService>();
        var customDialogService = provider.GetService<ICustomDialogService>();

        Assert.NotNull(dialogService);
        Assert.NotNull(customDialogService);
        Assert.IsType<InnovativeDialogService>(dialogService);
        Assert.IsType<RadzenDialogServiceAdapter>(customDialogService);
    }

    [Fact]
    public void AddCustomLocalizerShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCustomLocalizer<FallbackType>();
        var provider = services.BuildServiceProvider();

        // Assert
        var localizerFactory = provider.GetService<IInnovativeStringLocalizerFactory>();
        var fallbackType = provider.GetService<Type>();

        Assert.NotNull(localizerFactory);
        Assert.IsType<InnovativeStringLocalizerFactory>(localizerFactory);
        Assert.Equal(typeof(FallbackType), fallbackType);
    }

    // Dummy class for testing the fallback type
    private sealed class FallbackType
    {
    }
}