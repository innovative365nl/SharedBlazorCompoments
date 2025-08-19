namespace Innovative.Blazor.Components.Common;

public static class ListExtensions
{
    public static void AddRange<T>(this IList<T> instance, IEnumerable<T> items)
    {
        foreach (T item in items.ToList())
        {
            instance?.Add(item: item);
        }
    }
}
