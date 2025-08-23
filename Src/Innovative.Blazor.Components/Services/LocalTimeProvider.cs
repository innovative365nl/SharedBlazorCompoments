#nullable enable
using System.Diagnostics;
using System.Globalization;

namespace Innovative.Blazor.Components.Services;

public interface ILocalTimeProvider
{
    TimeZoneInfo LocalTimeZone { get; }
    long TimestampFrequency { get; }
    bool IsLocalTimeZoneSet { get; }
    string Format { get; init; }
    event EventHandler? LocalTimeZoneChanged;
    void SetLocalTimeZone(string timeZone);
    ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period);
    TimeSpan GetElapsedTime(long startingTimestamp);
    TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp);
    DateTimeOffset GetLocalNow();
    long GetTimestamp();
    DateTimeOffset GetUtcNow();
}

internal sealed class LocalTimeProvider(string format = "dd-MM-yyyy HH:mm:ss") : TimeProvider, ILocalTimeProvider
{
    private TimeZoneInfo? userLocalTimeZone;

    public string Format { get; init; } = format;

    public override TimeZoneInfo LocalTimeZone => userLocalTimeZone ?? base.LocalTimeZone;

    public bool IsLocalTimeZoneSet => userLocalTimeZone != null;

    public event EventHandler? LocalTimeZoneChanged;

    public void SetLocalTimeZone(string timeZone)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(id: timeZone, timeZoneInfo: out var timeZoneInfo))
        {
            timeZoneInfo = null;
        }

        if (timeZoneInfo != LocalTimeZone)
        {
            userLocalTimeZone = timeZoneInfo;
            LocalTimeZoneChanged?.Invoke(sender: this, e: EventArgs.Empty);
        }
    }
}

public static class LocalTimeProviderExtensions
{
    public static DateTime ToLocalDateTime(this ILocalTimeProvider localTimeProvider, DateTime dateTime)
    {
        Debug.Assert(localTimeProvider != null, nameof(localTimeProvider) + " != null");
        return dateTime.Kind switch
               {
                   DateTimeKind.Local => dateTime, _ => DateTime.SpecifyKind(value: TimeZoneInfo.ConvertTimeFromUtc(dateTime: dateTime, destinationTimeZone: localTimeProvider.LocalTimeZone), kind: DateTimeKind.Local)
               };
    }


    public static DateTime ToLocalDateTime(this ILocalTimeProvider localTimeProvider, DateTimeOffset dateTime)
    {
        Debug.Assert(localTimeProvider != null, nameof(localTimeProvider) + " != null");
        var local = TimeZoneInfo.ConvertTimeFromUtc(dateTime: dateTime.UtcDateTime, destinationTimeZone: localTimeProvider.LocalTimeZone);
        local = DateTime.SpecifyKind(value: local, kind: DateTimeKind.Local);
        return local;
    }

    public static DateTime ToLocalDateTime(this ILocalTimeProvider localTimeProvider, DateOnly dateOnly)
    {
        var dateTime = new DateTime(dateOnly.Year, dateOnly.Month, dateOnly.Day);
        Debug.Assert(localTimeProvider != null, nameof(localTimeProvider) + " != null");
        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(dateTime, destinationTimeZone: localTimeProvider.LocalTimeZone), kind: DateTimeKind.Local);
    }

    public static string ToLocalDateTimeString(this ILocalTimeProvider localTimeProvider, DateTime dateTime, string? format)
    {
        var localDateTime = localTimeProvider.ToLocalDateTime(dateTime);
        Debug.Assert(localTimeProvider != null, nameof(localTimeProvider) + " != null");
        return localDateTime.ToString(string.IsNullOrEmpty(format)
                                          ? localTimeProvider.Format
                                          : format, CultureInfo.InvariantCulture);

    }
    public static string ToLocalDateTimeString(this ILocalTimeProvider localTimeProvider, DateTimeOffset dateTime, string? format)
    {
        var localDateTime = localTimeProvider.ToLocalDateTime(dateTime);
        Debug.Assert(localTimeProvider != null, nameof(localTimeProvider) + " != null");
        return localDateTime.ToString(string.IsNullOrEmpty(format)
                                           ? localTimeProvider.Format
                                           : format, CultureInfo.InvariantCulture) ?? throw new InvalidOperationException();

    }
    public static string ToLocalDateTimeString(this ILocalTimeProvider localTimeProvider, DateOnly dateOnly, string? format)
    {
        var localDateTime = localTimeProvider.ToLocalDateTime(dateOnly);
        Debug.Assert(localTimeProvider != null, nameof(localTimeProvider) + " != null");
        return localDateTime.ToString(string.IsNullOrEmpty(format)
                                          ? localTimeProvider.Format
                                          : format, CultureInfo.InvariantCulture);
    }

}
