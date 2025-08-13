using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class ComplexEditComponent : CustomComponent<ComplexModel>
{
    private List<ComplexModel> _items  = [];
    
    protected override async Task OnInitializedAsync()
    {
        // Simulate an asynchronous data fetch
        await Task.Delay(1000).ConfigureAwait(false);
        _items =
        [
            new ComplexModel
            {
                Name = "Item 1"
              , Description = "Description for Item 1"
            }
          , new ComplexModel
            {
                Name = "Item 2"
              , Description = "Description for Item 2"
            }
          , new ComplexModel
            {
                Name = "Item 3"
              , Description = "Description for Item 3"
            }
        ];
    }

    // This parameter is used to control the display of the label in the component.
    // The only requirements are: it must be a parameter and its name must be "DisplayLabel".
    // The property type should be as open as possible, so "object?" will do the job.
    [Parameter]
    public object? DisplayLabel { get; set; }
}
