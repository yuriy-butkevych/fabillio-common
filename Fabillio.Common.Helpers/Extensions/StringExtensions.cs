using System.Linq;
using System.Text.RegularExpressions;

namespace Fabillio.Common.Helpers.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Get only integer from string
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetInteger(this string value, out int? result)
    {
        result = null;

        if (string.IsNullOrEmpty(value))
            return false;

        var numbers = value.GetOnlyDigitsAsString();

        if (int.TryParse(numbers, out var number))
        {
            result = number;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Get integers as string by lenght (optional, 0 - get all)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static bool TryGetIntegersAsStringByLenght(this string value, int lenght, out string result)
    {
        result = null;

        if (string.IsNullOrEmpty(value))
            return false;

        var digits = value.GetOnlyDigitsAsString();

        if (digits.Length == default)
        {
            result = digits;
            return true;
        }

        if (digits.Length == lenght)
        {
            result = digits;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Get in range from 0 to 9 digits represented as string
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetOnlyDigitsAsString(this string value)
        => new(value.Where(InRangeFromZeroToNineInteger).ToArray());

    /// <summary>
    /// Check if symbol in range of digits from 0 to 9
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool InRangeFromZeroToNineInteger(char value)
        => value >= '0' && value <= '9';

    public static string ToKebabCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        var result = Regex.Replace(input, "(?<!^)([A-Z])", "-$1").ToLower();

        return result;
    }
}
