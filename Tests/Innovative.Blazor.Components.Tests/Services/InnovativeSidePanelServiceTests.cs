using System;
using Innovative.Blazor.Components.Localizer;
using Innovative.Blazor.Components.Services;
using Moq;

namespace Innovative.Blazor.Components.Tests.Services;

public class InnovativeSidePanelServiceTests
{
    private readonly Mock<ISidepanelService> sidePanelServiceMock;

    private readonly InnovativeSidePanelService subject;

    public InnovativeSidePanelServiceTests()
    {
        sidePanelServiceMock = new Mock<ISidepanelService>();

        var localizer = new Mock<IInnovativeStringLocalizer>();
        var localizerFactory = new Mock<IInnovativeStringLocalizerFactory>();

        localizerFactory
            .Setup(f => f.Create(It.IsAny<Type>()))
            .Returns(localizer.Object);

        subject = new InnovativeSidePanelService(sidePanelServiceMock.Object, localizerFactory.Object);
    }

    [Fact]
    public void WhenClosePanel_WhileOpen_ItShouldCloseTheSidePanel()
    {
        // Arrange
        sidePanelServiceMock
            .Setup(expression: x => x.IsVisible)
            .Returns(value: true);

        // Act
        subject.ClosePanel(model: new TestFormModel());

        // Assert
        sidePanelServiceMock.Verify(expression: x => x.CloseSidepanel(It.IsAny<TestFormModel>()), times: Times.Once);
    }

    [Fact]
    public void WhenClosePanel_WhileClosed_ItShouldDoNothing()
    {
        // Arrange
        sidePanelServiceMock
            .Setup(expression: x => x.IsVisible)
            .Returns(value: false);

        // Act
        subject.ClosePanel(new TestFormModel());

        // Assert
        sidePanelServiceMock.Verify(x => x.CloseSidepanel(It.IsAny<TestFormModel>()), Times.Never);
    }

}
