#region

using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

#endregion

namespace Innovative.Blazor.Components.Components.TrackInUrl;

/// <summary>
/// Provides functionality to automatically synchronize component state with URL parameters.
/// 
/// <para>
/// This abstract class enables Blazor components to persist their state in the URL, allowing for:
/// - Shareable URLs that restore component state
/// - Browser history integration with back/forward navigation
/// - Bookmarkable component states
/// </para>
/// 
/// <para>
/// Usage:
/// 1. Create a component that inherits from UrlStateTracker
/// 2. Mark properties to be tracked with the [TrackInUrl] attribute
/// 3. Call OnPropertyPossiblyChanged() in property setters or after property values change
/// </para>
/// 
/// <example>
/// <code>
/// public class MyFilterComponent : UrlStateTracker
/// {
///     private string _searchTerm;
///     
///     [TrackInUrl]
///     public string SearchTerm 
///     { 
///         get => _searchTerm; 
///         set 
///         { 
///             _searchTerm = value; 
///             OnPropertyPossiblyChanged();
///         }
///     }
///     
///     public MyFilterComponent(NavigationManager navigationManager) 
///         : base(navigationManager)
///     {
///     }
/// }
/// </code>
/// </example>
/// </summary>
public abstract class UrlStateTracker(NavigationManager navigationManager) : ComponentBase, IDisposable
{
    private readonly Dictionary<PropertyInfo, object?> _trackedProperties = new();
    private bool _disposed;

    private bool _isInitialized;
    private bool _isUpdatingFromUrl;

    /// <summary>
    /// Disposes the component and unregisters the location changed event handler.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes the component by discovering properties marked with [TrackInUrl],
    /// sets up URL change tracking, and loads initial state from the URL.
    /// 
    /// <para>
    /// This method automatically:
    /// - Identifies all properties with the [TrackInUrl] attribute
    /// - Stores their initial values for change tracking
    /// - Subscribes to URL navigation events
    /// - Synchronizes component state with the current URL parameters
    /// </para>
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();

        var trackedProps = GetType()
            .GetProperties(bindingAttr: BindingFlags.Public | BindingFlags.Instance)
            .Where(predicate: p => p.GetCustomAttribute<TrackInUrlAttribute>() != null)
            .ToList();

        foreach (var prop in trackedProps)
        {
            _trackedProperties[key: prop] = prop.GetValue(obj: this)!;
        }

        navigationManager.LocationChanged += HandleLocationChanged!;
        ReadFromUrl();

        _isInitialized = true;
    }

    /// <summary>
    /// Event handler for NavigationManager's LocationChanged event.
    /// Reads and updates tracked properties from the new URL.
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="e">The event arguments</param>
    private void HandleLocationChanged(object sender, LocationChangedEventArgs e)
    {
        ReadFromUrl();
    }

    /// <summary>
    /// Call this method whenever a tracked property value changes to update the URL.
    /// 
    /// <para>
    /// This method should be called in property setters of properties marked with [TrackInUrl].
    /// It will detect changes in tracked properties and update the URL accordingly.
    /// </para>
    /// 
    /// <para>
    /// This method will not trigger URL updates when:
    /// - The component is still initializing
    /// - The property change is being made as a result of reading from the URL
    /// </para>
    /// </summary>
    protected void OnPropertyPossiblyChanged()
    {
        if (!_isInitialized || _isUpdatingFromUrl)
            return;

        UpdateUrlIfNeeded();
    }

    /// <summary>
    /// Compares current property values with stored values and updates the URL if any tracked property has changed.
    /// 
    /// <para>
    /// The method:
    /// 1. Detects which properties have changed since last check
    /// 2. Creates a serialized representation of all tracked properties
    /// 3. Updates the URL with the serialized state as a Base64-encoded query parameter
    /// </para>
    /// </summary>
    private void UpdateUrlIfNeeded()
    {
        var anyChange = false;
        var updatedState = new Dictionary<string, object?>();

        foreach (var kvp in _trackedProperties)
        {
            var prop = kvp.Key;
            var oldValue = kvp.Value;
            var newValue = prop.GetValue(obj: this);

            updatedState[key: prop.Name] = newValue;

            if (!Equals(objA: oldValue, objB: newValue))
            {
                anyChange = true;
                _trackedProperties[key: prop] = newValue;
            }
        }

        if (!anyChange)
            return;

        var json = JsonSerializer.Serialize(value: updatedState);
        var base64 = Convert.ToBase64String(inArray: Encoding.UTF8.GetBytes(s: json));

        var currentUri = navigationManager.Uri;
        var newUri = UpdateQueryStringParameter(url: currentUri, key: "dataset", value: base64);

        navigationManager.NavigateTo(uri: newUri, forceLoad: false);
    }

    /// <summary>
    /// Reads the component state from the URL and updates tracked properties accordingly.
    /// 
    /// <para>
    /// This method:
    /// 1. Extracts the "dataset" query parameter from the URL
    /// 2. Decodes the Base64-encoded JSON state
    /// 3. Updates component properties with values from the URL
    /// 4. Triggers a component re-render with StateHasChanged
    /// </para>
    /// 
    /// <para>
    /// Error handling is included to ensure the component doesn't break if the URL contains
    /// invalid data or if deserialization fails.
    /// </para>
    /// </summary>
    private void ReadFromUrl()
    {
        _isUpdatingFromUrl = true;
        try
        {
            var uri = navigationManager.ToAbsoluteUri(relativeUri: navigationManager.Uri);
            var query = QueryHelpers.ParseQuery(queryString: uri.Query);

            if (query.TryGetValue(key: "dataset", value: out var base64Values))
            {
                var base64 = base64Values.ToString();
                var json = Encoding.UTF8.GetString(bytes: Convert.FromBase64String(s: base64));
                var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json: json);

                if (dict != null)
                {
                    foreach (var prop in _trackedProperties.Keys.ToList())
                    {
                        if (dict.TryGetValue(key: prop.Name, value: out var jsonElement))
                        {
                            var typedValue = JsonSerializer.Deserialize(json: jsonElement.GetRawText(),
                                returnType: prop.PropertyType);
                            prop.SetValue(obj: this, value: typedValue);
                            _trackedProperties[key: prop] = typedValue;
                        }
                    }

                    StateHasChanged();
                }
            }
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Console.WriteLine(value: $"Error reading state from URL: {ex.Message}");
        }
        finally
        {
            _isUpdatingFromUrl = false;
        }
    }

    /// <summary>
    /// Updates a query string parameter in a URL, preserving other parameters.
    /// 
    /// <para>
    /// This helper method will:
    /// 1. Parse the existing URL and its query parameters
    /// 2. Add or replace the specified parameter
    /// 3. Rebuild the URL with the updated query string
    /// </para>
    /// </summary>
    /// <param name="url">The original URL</param>
    /// <param name="key">The query parameter key to update</param>
    /// <param name="value">The new value for the query parameter</param>
    /// <returns>The updated URL</returns>
    private static string UpdateQueryStringParameter(string url, string key, string value)
    {
        if (!Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, result: out var uri))
            return url;

        var baseUri = uri.GetLeftPart(part: UriPartial.Path);
        var queryParams = QueryHelpers.ParseQuery(queryString: uri.Query);
        var updatedParams = new Dictionary<string, StringValues>();

        foreach (var qp in queryParams)
        {
            if (qp.Key.Equals(value: key, comparisonType: StringComparison.OrdinalIgnoreCase))
                continue;

            updatedParams[key: qp.Key] = qp.Value;
        }

        updatedParams[key: key] = value;

        var newQuery = QueryString.Create(parameters: updatedParams);

        return baseUri + newQuery;
    }

    /// <summary>
    /// Disposes managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources, false if finalizing</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                navigationManager.LocationChanged -= HandleLocationChanged!;
            }

            // Dispose unmanaged resources (none in this case)
            _disposed = true;
        }
    }
}
