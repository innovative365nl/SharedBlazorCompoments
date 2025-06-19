using Innovative.Blazor.Components.Components;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Components;

public partial class ComplexComponent : CustomComponent<ComplexModel>
{
    private List<ComplexModel> Items  = new List<ComplexModel>();
    protected override async Task OnInitializedAsync()
    {
        // Simulate an asynchronous data fetch
        await Task.Delay(1000).ConfigureAwait(false);
        Items =
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
}

