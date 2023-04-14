using System.Diagnostics.CodeAnalysis;

namespace Fabillio.Common.Helpers.GlobalHelpers;

public static class CountryHelper
{
    public static string GetSoftrigCountryFor([AllowNull] string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode)) return string.Empty;
        return IsoNames.CountryNames.GetName(CultureHelper.GetCustomCultureInfo(false), countryCode);
    }
}