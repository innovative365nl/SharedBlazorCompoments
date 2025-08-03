using Innovative.Blazor.Components.Components;
using Innovative.Blazor.Components.Services;
using Microsoft.AspNetCore.Components;

namespace ExampleApp.Pages;

public partial class ExampleDialogService5(
    IInnovativeSidePanelService sidePanelService) : ComponentBase
{
    private DecimalModel TestClass1 { get; set; } = new DecimalModel()
                                                    {
                                                        FirstValue = 123.4512314m,
                                                        SecondValue = 123.45999m,
                                                        ThirdValue = 333
                                                    };

    public async Task OpenDecimalDialog() => await sidePanelService.OpenInEditMode(TestClass1).ConfigureAwait(false);
}

internal sealed class DecimalModel : FormModel
{
    [UIFormField(name: "Decimal Value", DisplayParameters = ["Format=0.##"], FormParameters = ["Format=0.##"])]
    public decimal? FirstValue { get; set; }
    [UIFormField(name: "Decimal Value 2 (no format)")]
    public decimal? SecondValue { get; set; }
    [UIFormField(name: "Int Value 3 (no format)")]
    public int? ThirdValue { get; set; }
}
