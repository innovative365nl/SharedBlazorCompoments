using System.Globalization;

namespace ExampleApp;

internal static class Constants
{
    public const string DateFormat = "yyyy-MM-dd";
    public const string CurrencyFormat = "C0";
}

public static class DateExtensions
{
    public static string ToDateString(this DateOnly instance)
        => instance.ToString(format: Constants.DateFormat, provider: CultureInfo.CurrentCulture);

    public static string ToDateString(this DateTime instance)
        => instance.ToString(format: Constants.DateFormat, provider: CultureInfo.CurrentCulture);

    public static DateOnly ToDate(this string instance)
    {
        return DateOnly.TryParseExact(s: instance, format: Constants.DateFormat, provider: CultureInfo.InvariantCulture, style: DateTimeStyles.None, result: out DateOnly result)
                   ? result
                   : DateOnly.MinValue;
    }
    public static DateTime ToDateTime(this string instance)
    {
        return DateTime.TryParseExact(s: instance, format: Constants.DateFormat, provider: CultureInfo.InvariantCulture, style: DateTimeStyles.None, result: out DateTime result)
                   ? result
                   : DateTime.MinValue;
    }

    public static int Age(this DateOnly dateOfBirth)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        int result = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(value: -result))
        {
            result--;
        }

        return result;
    }

    public static int Age(this DateTime dateOfBirth) => Age(DateOnly.FromDateTime(dateOfBirth));
}

public static class CurrencyExtensions
{
    public static string ToCurrencyString(this decimal instance)
        => instance.ToString(format: Constants.CurrencyFormat, provider: CultureInfo.CurrentCulture);

    public static decimal ToCurrency(this string instance)
    {
        return decimal.TryParse(s: instance, style: NumberStyles.Currency, provider: CultureInfo.CurrentCulture, result: out decimal result)
                   ? result
                   : decimal.Zero;
    }
}
