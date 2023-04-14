using System.Globalization;

namespace Fabillio.Common.Helpers.GlobalHelpers;

public static class CultureHelper
{
    private const string _cultureInfo = "nb-NO";
    private const string _negativeSign = "-";
    private const string _numberDecimalSeparator = ",";

    public static CultureInfo GetCustomCultureInfo(bool includeNumbersFormat = true)
    {
        var customCulture = (CultureInfo)CultureInfo.GetCultureInfo(_cultureInfo).Clone();

        if (includeNumbersFormat)
        {
            customCulture.NumberFormat.NegativeSign = _negativeSign;
            customCulture.NumberFormat.NumberDecimalDigits = 2;
            customCulture.NumberFormat.CurrencyDecimalDigits = 2;
            customCulture.NumberFormat.NumberDecimalSeparator = _numberDecimalSeparator;
        }

        return customCulture;
    }
}
