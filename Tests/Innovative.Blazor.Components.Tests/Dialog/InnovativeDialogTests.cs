using System;
using System.Linq;
using System.Threading.Tasks;
using Innovative.Blazor.Components.Components.Dialog;
using Innovative.Blazor.Components.Localizer;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Innovative.Blazor.Components.Services;
using Innovative.Blazor.Components.Enumerators;
using Innovative.Blazor.Components.Tests.Dialog;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using Radzen.Blazor.Rendering;

namespace Innovative.Blazor.Components.Tests;

public class InnovativeDialogTests : TestContext
{
    private readonly Mock<IInnovativeStringLocalizerFactory> _localizerFactoryMock;
    private readonly Mock<IInnovativeStringLocalizer> _localizerMock;
    private readonly Mock<ILogger<InnovativeDialogService>> _loggerMock;
    private readonly Mock<DialogService> _radzenDialogServiceMock;
    private InnovativeDialogService _dialogService;

    public InnovativeDialogTests()
    {
        _loggerMock = new Mock<ILogger<InnovativeDialogService>>();
        _localizerMock = new Mock<IInnovativeStringLocalizer>();
        _localizerFactoryMock = new Mock<IInnovativeStringLocalizerFactory>();
    
        // Create a real NavigationManager for testing
        var navigationManager = new TestNavigationManager();
        var iJsRuntime = new Mock<IJSRuntime>();
        // Create a real DialogService
        var dialogService = new DialogService(navigationManager, iJsRuntime.Object );
    
        // Setup localizer factory to return our localizer mock
        _localizerFactoryMock
            .Setup(f => f.Create(It.IsAny<Type>()))
            .Returns(_localizerMock.Object);

        // Setup localizer to return the key as the value
        _localizerMock
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string name) => new LocalizedString(name, name));
        

        // Create service instance with real DialogService
        _dialogService = new InnovativeDialogService(dialogService, _localizerFactoryMock.Object);
    }
    // [Fact]
    // public async Task OpenPersonDialog_ShouldCloseWhenCloseButtonClicked()
    // {
    //     using var ctx = new TestContext();
    //     ctx.JSInterop.Mode = JSRuntimeMode.Loose;
    //
    //     // Create a real NavigationManager and JSRuntime
    //     var navigationManager = new TestNavigationManager();
    //     var jsRuntimeMock = new Mock<IJSRuntime>();
    //     var dialogService = new DialogService(navigationManager, jsRuntimeMock.Object);
    //
    //     // Register required services
    //     ctx.Services.AddSingleton(dialogService);
    //     ctx.Services.AddSingleton<InnovativeDialogService>(new InnovativeDialogService(dialogService, _localizerFactoryMock.Object));
    //     ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory>(_localizerFactoryMock.Object);
    //     ctx.Services.AddSingleton<IInnovativeStringLocalizer>(_localizerMock.Object);
    //     ctx.Services.AddRadzenComponents();
    //
    //     // Render the component
    //     var component = ctx.RenderComponent<InnovativeDialogServiceTest>();
    //
    //     // Open the dialog
    //     var openDialogButton = component.Find("button");
    //     openDialogButton.Click();
    //     await Task.Delay(100); // Wait for rendering
    //
    //     // Verify the close button is present
    //     var closeButton = component.FindAll("#rightSideDialogCloseButton").FirstOrDefault();
    //     closeButton.Should().NotBeNull("The rightSideDialogCloseButton should appear after opening the dialog.");
    //
    //     // Close the dialog
    //     closeButton.Click();
    //     await Task.Delay(100); // Wait for rendering
    //
    //     // Ensure the close button is gone
    //     var buttonAfterClose = component.FindAll("#rightSideDialogCloseButton").FirstOrDefault();
    //     buttonAfterClose.Should().BeNull("The close button should no longer be present after closing the dialog.");
    // }

    [Fact]
    public void GetFormTitle_WithUIFormClass_UsesLocalizer()
    {
        // Arrange
        var testKey = "TestFormTitle";
        LocalizedString expectedTitle = new LocalizedString(testKey, "Custom Title");

        _localizerMock.Setup(l => l["TestFormTitle"]).Returns(expectedTitle);

        // Use reflection to call the private method
        var method = typeof(InnovativeDialogService).GetMethod("GetFormTitle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // Create generic method for our test type
        var genericMethod = method.MakeGenericMethod(typeof(TestFormModelWithAttribute));

        // Act
        var title = genericMethod.Invoke(_dialogService, null) as string;

        // Assert
        title.Should().Be(expectedTitle);
        _localizerFactoryMock.Verify(f => f.Create(typeof(TestResources)), Times.Once);
    }

    [Fact]
    public void GetFormTitle_WithoutUIFormClass_UsesTypeName()
    {
        // Arrange & Act
        var method = typeof(InnovativeDialogService).GetMethod("GetFormTitle", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var genericMethod = method.MakeGenericMethod(typeof(TestFormModel));
        var title = genericMethod.Invoke(_dialogService, null) as string;

        // Assert
        title.Should().Be("TestFormModel");
        _localizerFactoryMock.Verify(f => f.Create(It.IsAny<Type>()), Times.Never);
    }


    [Fact]
    public void ClickingEditButton_ShouldRenderEditChildContent()
    {
        // Brief explanation: This test verifies that the edit button switches
        // the dialog to editing mode and renders the EditChildContent.

        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        // Register required services
        var navigationManager = new TestNavigationManager();
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var dialogService = new DialogService(navigationManager, jsRuntimeMock.Object);
        ctx.Services.AddSingleton(dialogService);
        ctx.Services.AddSingleton<InnovativeDialogService>(
            new InnovativeDialogService(dialogService, _localizerFactoryMock.Object)
        );
        ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory>(_localizerFactoryMock.Object);
        ctx.Services.AddSingleton<IInnovativeStringLocalizer>(_localizerMock.Object);
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
        cut.Markup.Contains("Edit form content here").Should().BeTrue();
        cut.Markup.Contains("View content here").Should().BeFalse();
    }
    [Fact]
    public void RightSideDialog_ShouldRenderViewAndEditChildContent()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Create a real NavigationManager and JSRuntime
        var navigationManager = new TestNavigationManager();
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var dialogService = new DialogService(navigationManager, jsRuntimeMock.Object);
        
        // Register required services
        ctx.Services.AddSingleton(dialogService);
        ctx.Services.AddSingleton<InnovativeDialogService>(new InnovativeDialogService(dialogService, _localizerFactoryMock.Object));
        ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory>(_localizerFactoryMock.Object);
        ctx.Services.AddSingleton<IInnovativeStringLocalizer>(_localizerMock.Object);
        ctx.Services.AddRadzenComponents();
        // Register necessary services, e\.g\.:
        // ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory, InnovativeStringLocalizerFactory>();

        var testModel = new TestDynamicFormModel();
        testModel.TestProperty = "TestProperty";

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
        
        cut.Markup.Contains("TestProperty").Should().BeTrue();
        // Check if edit content is hidden initially, etc.
    }

    [Fact]
    public void Should_Find_TestActionButton_AndClickIt()
    {
        using var ctx = new TestContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        
        // Create a real NavigationManager and JSRuntime
        var navigationManager = new TestNavigationManager();
        var jsRuntimeMock = new Mock<IJSRuntime>();
        var dialogService = new DialogService(navigationManager, jsRuntimeMock.Object);
        
        // Register required services
        ctx.Services.AddSingleton(dialogService);
        ctx.Services.AddSingleton<InnovativeDialogService>(new InnovativeDialogService(dialogService, _localizerFactoryMock.Object));
        ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory>(_localizerFactoryMock.Object);
        ctx.Services.AddSingleton<IInnovativeStringLocalizer>(_localizerMock.Object);
        ctx.Services.AddRadzenComponents();
        // Register necessary services, e\.g\.:
        // ctx.Services.AddSingleton<IInnovativeStringLocalizerFactory, InnovativeStringLocalizerFactory>();

        bool clicked = false;
        var testModel = new TestDynamicFormModel
            {
                TestProperty = "TestProperty1",
                CustomProperty = "CustomProperty1"
            };

        testModel.TestAction += () =>
        {
            Console.WriteLine("TestAction clicked");
            clicked = true;
            
        };
        
        // Render the right side dialog with DynamicDisplayView
        var cut = ctx.RenderComponent<RightSideDialog<TestDynamicFormModel>>(parameters =>
        {
            parameters.Add(p => p.Model, new TestDynamicFormModel());
            parameters.Add(p => p.ViewChildContent, builder =>
            {
                builder.OpenComponent<DynamicDisplayView<TestDynamicFormModel>>(0);
                builder.AddAttribute(1, "Model",  testModel);
                builder.CloseComponent();
            });
        });

        // Verify the button text and click
        var button = cut.Find("button:contains('Test Action')");
        button.Click();

        // Add any assertions related to the action performed
        clicked.Should().BeTrue();
        
    }

    [Theory]
    [InlineData(SideDialogWidth.Normal, "40vw;")]
    [InlineData(SideDialogWidth.Large, "60vw;")]
    [InlineData(SideDialogWidth.ExtraLarge, "80vw;")]
    public void GetWidth_ReturnsCorrectWidthString(SideDialogWidth width, string expected)
    {
        // Arrange
        var method = typeof(InnovativeDialogService).GetMethod("GetWidth", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        // Act
        var result = method.Invoke(null, new object[] { width }) as string;

        // Assert
        result.Should().Be(expected);
    }

// Add this helper class to your test project
    public class TestNavigationManager : NavigationManager
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

    // [Fact]
    // public async Task OpenDynamicFormDialog_WithCustomOptions_UsesOptions()
    // {
    //     // Arrange
    //     var testModel = new TestFormModel { Name = "Test" };
    //     var options = new SideDialogOptions
    //     {
    //         Width = "50vw",
    //         ShowTitle = true,
    //         ShowMask = true,
    //         CloseDialogOnOverlayClick = true
    //     };
    //
    //     // Act
    //     var result = await _dialogService.OpenDynamicFormDialog(testModel, options);
    //
    //     // Assert
    //     result.Should().NotBeNull();
    //     result.Should().BeSameAs(testModel);
    // }
}

[UIFormClass(title: "Test Form", ResourceType = typeof(TestResources))]
public class TestDynamicFormModel
{
    [UIFormField(Name = "Test Property")]
    public string TestProperty { get; set; }

    public string CustomProperty { get; set; }

    [UIFormViewAction(Name = "Test Action")]
    public Action TestAction { get; set; }
}

public class TestFormModel
{
    public string Name { get; set; }
}

public class TestResourcesClass {}

[UIFormClass(title:  "TestFormTitle", columns: 1, ResourceType = typeof(TestResourcesClass))]
public class TestFormModelWithAttribute
{
    public string Name { get; set; }
}