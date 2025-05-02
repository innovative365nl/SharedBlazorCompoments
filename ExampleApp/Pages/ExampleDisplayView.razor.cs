using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDisplayView : ComponentBase
{
    private readonly SimplePersonModel person = new SimplePersonModel
    {
        FirstName = "John",
        LastName = "Doe",
        IsActive = true,
        BirthDate = new DateTime(1993, 5, 12),
        Biography = "This is a biography of John Doe. He is a software engineer with over 10 years of experience in the industry."
    };
}
