using System.Diagnostics.CodeAnalysis;
using ExampleApp.Components;
using Innovative.Blazor.Components.Components;

namespace ExampleApp.Pages;

public partial class ExampleComplexGrid2
(
    IAttributeState state
)
{
    // This is the datasource for the grid
    private IEnumerable<ParentGridModel>? _parentsView;

    protected override async Task OnInitializedAsync() => await OnRefreshData().ConfigureAwait(true);

    // RadzenButton 🔄 Click
    internal protected async Task OnRefreshData()
    {
            await state
                .RefreshDataAsync()
                .ConfigureAwait(true);

            var parents = state.AttributeTypes
                                .Select(at=> new Parent(Id: at.Id, Value: at.Value, state.Attributes
                                                                                         .Where(x => x.Type.Id == at.Id)
                                                                                         .Select(x => new Child(x.Id, x.Name))
                                                                                         .ToList()))
                                .ToList();

            _parentsView = parents.Select(ParentGridModel.ToGridModel);
            StateHasChanged();
    }
}


// This type defines the viewmodel for the grid.
[UIGridClass(AllowSorting = true)]
public sealed class ParentGridModel
{
    public required int Id { get; set; }

    [UIGridField(Name = "Parent")]
    public required string Name { get; set; }

    [UIGridField(Name = "Children", CustomComponentType = typeof(ChildrenDisplayComponent))]
    public required IEnumerable<Child> Children { get; set; }

    public static ParentGridModel ToGridModel([NotNull] Parent instance)
    {
        return new ParentGridModel
        {
            Name = instance.Value,
            Id = instance.Id,
            Children = instance.Children.ToList(),
        };
    }
}

public record Parent(int Id, string Value, IEnumerable<Child> Children);

public record Child(Guid Id, string Value);
