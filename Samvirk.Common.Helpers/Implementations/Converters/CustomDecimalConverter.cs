using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Fabillio.Common.Helpers.GlobalHelpers;

namespace Fabillio.Common.Helpers.Implementations.Converters;

public class CustomDecimalConverter : DefaultTypeConverter
{
    public override string ConvertToString(
        object value,
        IWriterRow row,
        MemberMapData memberMapData
    )
    {
        if (value is decimal decimalValue)
        {
            var formattedValue = decimalValue.ToString("F2", CultureHelper.GetCustomCultureInfo());

            return formattedValue;
        }

        return base.ConvertToString(value, row, memberMapData);
    }
}
