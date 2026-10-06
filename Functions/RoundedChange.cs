using System;
using System.Globalization;

namespace FCS.Utilities;

public static partial class Function
{
    #region Public Methods

    /// <summary>
    /// Round the amount entered to the nearest nickel (5 cents)
    /// </summary>
    /// <param name="change"></param>
    public static decimal RoundToNickel(this decimal? change)
    {
        if (!change.HasValue || change.Value < .01M) return 0;                      // No Change

        decimal value = change.Value;
        int Dollars = (int)value;                                                   // Calculate Full Dollars
        int Cents = (int)(value * 100 % 100);                                       // Calculate Cents
        int Remaining = Cents % 5;                                                  // Calculate Module Of 5

        Cents = Remaining <= 2 ? Cents -= Remaining : Cents += 5 - Remaining;       // Adjust As Required

        return Dollars + (decimal)Cents / 100;                                      // Return Adjusted Amount
    }

    /// <summary>
    /// Round the amount entered to the nearest nickel (5 cents)
    /// </summary>
    /// <param name="change"></param>
    public static decimal RoundToNickel(this decimal change) => RoundToNickel((decimal?)change);

    /// <summary>
    /// Round the amount entered to the nearest nickel (5 cents)
    /// </summary>
    /// <param name="amount">Amount as a string</param>
    /// <returns>Rounded amount as a decimal</returns>
    public static decimal RoundToNickel(this string amount) => RoundToNickel(Convert.ToDecimal(amount));

    /// <summary>
    /// Calculate the amount of "Change" required to give back to the customer based on the amount charged and the amount tendered.
    /// </summary>
    /// <param name="charge">decimal</param>
    /// <param name="tendered">decimal</param>
    public static decimal CashChange(decimal? charge = 0, decimal? tendered = 0)
    {
        decimal chargeValue = charge ?? 0;                                          // Default To 0 If Null
        decimal tenderedValue = tendered ?? 0;                                      // Default To 0 If Null

        if (chargeValue < 0 || tenderedValue < 0) return 0;                         // Make Sure We Have Positive Amounts

        return (tenderedValue - chargeValue).RoundToNickel();                       // Return Change
    }

    /// <summary>
    /// Calculate the amount of "Change" required to give back to the customer based on the amount charged and the amount tendered.
    /// </summary>
    /// <param name="charge">string</param>
    /// <param name="tendered">string</param>
    public static decimal CashChange(string charge, string tendered)
    {
        decimal.TryParse(charge, Style, Culture, out decimal Charged);              // Try To Parse Charge, Default To 0 If Invalid
        decimal.TryParse(tendered, Style, Culture, out decimal Tendered);           // Try To Parse Tendered, Default To 0 If Invalid

        return CashChange(Charged, Tendered);                                       // Return Change
    }

    /// <summary>
    /// Calculate the amount of "Change" required to give back to the customer based on the amount charged and the amount tendered.
    /// </summary>
    /// <param name="charge">decimal</param>
    /// <param name="tendered">string</param>
    public static decimal CashChange(decimal? charge, string tendered)
    {
        decimal.TryParse(tendered, Style, Culture, out decimal Tendered);           // Try To Parse Tendered, Default To 0 If Invalid

        return CashChange(charge, Tendered);                                        // Return Change
    }

    /// <summary>
    /// Calculate the amount of "Change" required to give back to the customer based on the amount charged and the amount tendered.
    /// </summary>
    /// <param name="charge">string</param>
    /// <param name="tendered">decimal</param>
    public static decimal CashChange(string charge, decimal? tendered)
    {
        decimal.TryParse(charge, Style, Culture, out decimal Charged);              // Try To Parse Charge, Default To 0 If Invalid

        return CashChange(Charged, tendered);                                        // Return Change
    }

    /// <summary>
    /// Calculate the amount of "Change" required to give back to the customer based on the amount charged and the amount tendered.
    /// </summary>
    /// <param name="charge">float</param>
    /// <param name="tendered">float</param>
    public static decimal CashChange(float charge, float tendered) => CashChange(Convert.ToDecimal(charge), Convert.ToDecimal(tendered));

    /// <summary>
    /// Calculate the amount of "Change" required to give back to the customer based on the amount charged and the amount tendered.
    /// </summary>
    /// <param name="charge">float</param>
    /// <param name="tendered">decimal</param>
    public static decimal CashChange(float charge, decimal? tendered) => CashChange(Convert.ToDecimal(charge), tendered);

    /// <summary>
    /// Calculate Change To Be Paid Out Based On Supplied
    /// Charge And Amount Tendered
    /// </summary>
    /// <param name="charge">decimal</param>
    /// <param name="tendered">float</param>
    public static decimal CashChange(decimal charge, float tendered) => CashChange(charge, Convert.ToDecimal(tendered));

    #endregion Public Methods

    #region Private Properties

    private const NumberStyles Style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;  // Define Number Styles For Parsing, Allowing Currency Symbols
    private static readonly CultureInfo Culture = CultureInfo.CreateSpecificCulture("en-US");   // Define Culture For Parsing, Using en-US To Ensure Consistent Decimal Separator (.)

    #endregion Private Properties
}