#region

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Localizer;
using Innovative.Blazor.Components.Services;
using Innovative.Blazor.Components.Tests.TestBase;
using Innovative.Blazor.Components.Tests.TestModels;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Moq;
using Radzen;
using Xunit.Abstractions;

#endregion

namespace Innovative.Blazor.Components.Tests;

public class DialogTests : LocalizedTestBase
{
    private readonly InnovativeDialogService _dialogService;
    private readonly Mock<ICustomDialogService> _dialogServiceMock;
    private readonly DialogService _radzenDialogService;
    private readonly Mock<DialogService> _radzenDialogServiceMock;

    private readonly ITestOutputHelper _testOutputHelper;


    public DialogTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        // Create a real NavigationManager for testing
        var navigationManager = new TestNavigationManager();
        var iJsRuntime = new Mock<IJSRuntime>();
        // Create a real DialogService
        _radzenDialogService = new DialogService(navigationManager, iJsRuntime.Object);
        _radzenDialogServiceMock = new Mock<DialogService>(navigationManager, iJsRuntime.Object);
        _dialogServiceMock = new Mock<ICustomDialogService>();


        // Setup localizer for specific tests that need it
        LocalizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));

        // Create service instance with real DialogService
        _dialogService = new InnovativeDialogService(_dialogServiceMock.Object, LocalizerFactoryMock.Object);
    }


    [Fact]
    public async Task OpenDynamicFormDialogOpensDialogWithCorrectParameters()
    {
        // Arrange
        var model = new TestDynamicFormModel { TestProperty = "Initial Value" };
        Dictionary<string, object>? capturedParameters = null;
        string? capturedTitle = null;
        var expectedTitle = "Test Form";

        _dialogServiceMock
            .Setup(x => x.OpenSideAsync<RightSideDialog<TestDynamicFormModel>>(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<SideDialogOptions>()))
            .ReturnsAsync((string title, Dictionary<string, object> parameters, SideDialogOptions options) =>
            {
                capturedTitle = title;
                capturedParameters = parameters;
                return new RightSideDialog<TestDynamicFormModel>(_dialogServiceMock.Object);
            });

        // Act
        await _dialogService.OpenDynamicFormDialog<TestDynamicFormModel>(model);

        // Assert
        Assert.NotNull(capturedParameters);
        Assert.Equal(expectedTitle, capturedTitle);
        Assert.Equal(expectedTitle, capturedParameters["Title"]);
    }

    [Fact]
    public void GetFormTitleWithUIFormClassUsesLocalizer()
    {
        // Arrange
        var testKey = "TestFormTitle";
        var expectedTitle = new LocalizedString(testKey, "Custom Title");

        LocalizerMock
            .Setup(l => l[testKey])
            .Returns(expectedTitle);

        LocalizerFactoryMock
            .Setup(f => f.Create(typeof(TestResourcesClass)))
            .Returns(LocalizerMock.Object);

        // Use reflection to call the private method
        var method = typeof(InnovativeDialogService).GetMethod("GetFormTitle",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (method == null)
        {
            throw new InvalidOperationException("Method not found");
        }

        // Create generic method for our test type
        var genericMethod = method.MakeGenericMethod(typeof(TestFormModelWithAttribute));

        // Act
        var title = genericMethod.Invoke(_dialogService, null) as string;

        // Assert
        title.Should().Be("Custom Title");
        LocalizerFactoryMock.Verify(f => f.Create(typeof(TestResourcesClass)), Times.Once);
    }


    [Fact]
    public void ClickingEditButtonShouldRenderEditChildContent()
    {
        // Brief explanation: This test verifies that the edit button switches
        // the dialog to editing mode and renders the EditChildContent.
        var customDialogServiceMock = new Mock<ICustomDialogService>();

        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // var dialogService = new DialogService(navigationManager, jsRuntimeMock.Object);
        ctx.Services.AddSingleton(_dialogService);
        ctx.Services.AddSingleton<InnovativeDialogService>();
        ctx.Services.AddSingleton(customDialogServiceMock.Object);
        ctx.Services.AddSingleton(LocalizerFactoryMock.Object);
        ctx.Services.AddSingleton(LocalizerMock.Object);

        ctx.Services.AddRadzenComponents();

        // Render the component
        var cut = ctx.RenderComponent<RightSideDialog<TestDynamicFormModel>>(parameters =>
        {
            parameters.Add(p => p.ShowEdit, true);
            parameters.Add(p => p.ViewChildContent, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "View content here");
                builder.CloseElement();
            });
            parameters.Add(p => p.EditChildContent, builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddContent(1, "Edit form content here");
                builder.CloseElement();
            });
        });

        // Click the edit button with id rightSideDialogCloseButton
        var editButton = cut.Find("#rightSideDialogEditButton");
        //var editButton = cut.Find("button[title=\"edit\"]"); // Adjust selector to match your HTML
        editButton.Click();

        // Verify edit content is shown
        cut.Markup.Contains("Edit form content here", StringComparison.Ordinal).Should().BeTrue();
        cut.Markup.Contains("View content here", StringComparison.Ordinal).Should().BeFalse();
    }

    [Fact]
    public void RightSideDialogShouldRenderViewAndEditChildContent()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        var customDialogServiceMock = new Mock<ICustomDialogService>();

        // Register the base Microsoft string localizer factory
        var mockStringLocalizerFactory = new Mock<IStringLocalizerFactory>();
        ctx.Services.AddSingleton<IStringLocalizerFactory>(mockStringLocalizerFactory.Object);
        ctx.Services.AddSingleton(customDialogServiceMock.Object);
        // Register custom localizer factory
        ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory>(sp =>
            LocalizerFactoryMock.Object
        );
        ctx.Services.AddRadzenComponents();

        var testModel = new TestDynamicFormModel
        {
            TestProperty = "TestProperty"
        };

        var cut = ctx.RenderComponent<RightSideDialog<TestDynamicFormModel>>(parameters =>
        {
            parameters.Add(p => p.Model, testModel);
            parameters.Add(p => p.ViewChildContent, builder =>
            {
                builder.OpenComponent<DynamicDisplayView<TestDynamicFormModel>>(0);
                builder.AddAttribute(1, "Model", testModel);
                builder.CloseComponent();
            });
            parameters.Add(p => p.EditChildContent, builder =>
            {
                builder.OpenComponent<DynamicFormView<TestModel>>(0);
                builder.AddAttribute(1, "Model", testModel);
                builder.CloseComponent();
            });
        });

        // Verify the rendered view content

        cut.Markup.Contains("TestProperty", StringComparison.Ordinal).Should().BeTrue();
        // Check if edit content is hidden initially, etc.
    }

    [Fact]
    public void ShouldFindTestActionButtonAndClickIt()
    {
        var customDialogServiceMock = new Mock<ICustomDialogService>();
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Register the base Microsoft string localizer factory
        var mockStringLocalizerFactory = new Mock<IStringLocalizerFactory>();
        ctx.Services.AddSingleton<IStringLocalizerFactory>(mockStringLocalizerFactory.Object);

        // Register custom localizer factory
        ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory>(sp =>
            LocalizerFactoryMock.Object
        );

        // Register other required services
        ctx.Services.AddSingleton(_radzenDialogService);
        ctx.Services.AddSingleton(_dialogService);
        ctx.Services.AddSingleton(LocalizerMock.Object);
        ctx.Services.AddSingleton(customDialogServiceMock.Object);
        ctx.Services.AddRadzenComponents();

        // Rest of the test remains the same
        var clicked = false;
        var testModel = new TestDynamicFormModel
        {
            TestProperty = "TestProperty1",
            CustomProperty = "CustomProperty1"
        };

        testModel.TestAction += () =>
        {
            _testOutputHelper.WriteLine("TestAction clicked");
            clicked = true;
        };

        // Render the component
        var cut = ctx.RenderComponent<RightSideDialog<TestDynamicFormModel>>(parameters =>
        {
            parameters.Add(p => p.Model, testModel);
            parameters.Add(p => p.ViewChildContent, builder =>
            {
                builder.OpenComponent<DynamicDisplayView<TestDynamicFormModel>>(0);
                builder.AddAttribute(1, "Model", testModel);
                builder.CloseComponent();
            });
        });

        var button = cut.Find("button:contains('Test Action')");
        button.Click();

        clicked.Should().BeTrue();
    }


    [Theory]
    [InlineData(SideDialogWidth.Normal, "40vw;")]
    [InlineData(SideDialogWidth.Large, "60vw;")]
    [InlineData(SideDialogWidth.ExtraLarge, "80vw;")]
    public void GetWidthReturnsCorrectWidthString(SideDialogWidth width, string expected)
    {
        // Arrange
        var method = typeof(InnovativeDialogService).GetMethod("GetWidth",
            BindingFlags.NonPublic | BindingFlags.Static);

        // Act
        var result = method?.Invoke(null, [width]) as string;

        // Assert
        result.Should().Be(expected);
    }


    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dialogService.Dispose();
            _radzenDialogService.Dispose();
        }

        base.Dispose(disposing);
    }

// Add this helper class to your test project
    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager()
        {
            Initialize("https://test.com/", "https://test.com/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NotifyLocationChanged(false);
        }
    }
}

[UIFormClass(title: "Test Form", ResourceType = typeof(TestResources))]
internal sealed class TestDynamicFormModel
{
    [UIFormField(name:"Test Property")] public string? TestProperty { get; set; }

    public string? CustomProperty { get; set; }

    [UIFormViewAction(Name = "Test Action")]
    public Action? TestAction { get; set; }
}

internal sealed class TestResourcesClass
{
}

[UIFormClass(title: "TestFormTitle", ResourceType = typeof(TestResourcesClass))]
internal sealed class TestFormModelWithAttribute
{
    public string? Name { get; set; }
}