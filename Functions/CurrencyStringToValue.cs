using System;
using System.Globalization;

namespace FCS.Utilities;

public static partial class Function
{
    /// <summary>
    /// Convert A String In Currency Format To A Decimal Value
    /// </summary>
    /// <param name="currency">Currency string to parse</param>
    /// <returns>Decimal value rounded to 2 decimal places</returns>
    public static decimal CurrencyStringToValue(this string currency)
    {
        if (string.IsNullOrWhiteSpace(currency)) return 0m;

        if (decimal.TryParse(currency,
            NumberStyles.AllowCurrencySymbol | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
            new CultureInfo("en-US"),
            out decimal amount))
        {
            return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        return 0m;
    }
}