using System.Security.Cryptography;
using Innovative.Blazor.Components.Components.Grid;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ExampleApp.Pages;

public partial class ExampleGrid(NotificationService notificationService)
{
    private readonly string[] cities = ["New York", "London", "Paris", "Tokyo", "Berlin"];
    private readonly string[] statuses = ["Active", "Pending", "Complete", "Rejected"];
    private readonly string[] streets = ["Main St", "Oak Ave", "Park Rd", "Cedar Ln", "Maple Dr"];

    private Innovative.Blazor.Components.Components.InnovativeGrid<InnovativeTestClass>? grid;
    List<InnovativeTestClass>? items;
    protected override async Task OnInitializedAsync()
    {
        items = Enumerable.Range(1, 40)
            .Select(i => new InnovativeTestClass
            {
                Name = $"Person {i}",
                Age = RandomNumberGenerator.GetInt32(18, 80),
                Address = $"{RandomNumberGenerator.GetInt32(1, 999)} {streets[RandomNumberGenerator.GetInt32(streets.Length)]}, {cities[RandomNumberGenerator.GetInt32(cities.Length)]}",
                Status = statuses[RandomNumberGenerator.GetInt32(statuses.Length)],
                Status2 = statuses[RandomNumberGenerator.GetInt32(statuses.Length)]
            })
            .ToList();

        await base.OnInitializedAsync().ConfigureAwait(false);
    }


    private void OnRowSelected(IEnumerable<InnovativeTestClass> obj)
    {
        notificationService.Notify(NotificationSeverity.Success, $"Selected {obj.First().Name}");
    }
}
