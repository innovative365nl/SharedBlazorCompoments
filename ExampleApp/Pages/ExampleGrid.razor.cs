using System.Security.Cryptography;
using Innovative.Blazor.Components.Components.Grid;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ExampleApp.Pages;
 
public partial class ExampleGrid(NavigationManager NavigationManager)
{
    private readonly string[] _cities = ["New York", "London", "Paris", "Tokyo", "Berlin"];
    private readonly string[] _statuses = ["Active", "Pending", "Complete", "Rejected"];
    private readonly string[] _streets = ["Main St", "Oak Ave", "Park Rd", "Cedar Ln", "Maple Dr"];

    private InnovativeGrid<InnovativeTestClass>? _grid;
    List<InnovativeTestClass>? _items;
    protected override async Task OnInitializedAsync()
    {
        _items = Enumerable.Range(1, 40)
            .Select(i => new InnovativeTestClass
            {
                Name = $"Person {i}",
                Age = RandomNumberGenerator.GetInt32(18, 80),
                Address = $"{RandomNumberGenerator.GetInt32(1, 999)} {_streets[RandomNumberGenerator.GetInt32(_streets.Length)]}, {_cities[RandomNumberGenerator.GetInt32(_cities.Length)]}",
                Status = _statuses[RandomNumberGenerator.GetInt32(_statuses.Length)],
                Status2 = _statuses[RandomNumberGenerator.GetInt32(_statuses.Length)]
            })
            .ToList();

        await base.OnInitializedAsync().ConfigureAwait(false);
    }


    private void OnRowSelected(IEnumerable<InnovativeTestClass> obj)
    {
        NotificationService.Notify(NotificationSeverity.Success, $"Selected {obj.First().Name}");
    }
}