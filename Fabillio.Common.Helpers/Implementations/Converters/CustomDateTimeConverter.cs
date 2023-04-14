using System;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Fabillio.Common.Helpers.Implementations.Converters;

public class CustomDateTimeConverter : DefaultTypeConverter
{
    private const string _dateTimeFormat = "dd.MM.yyyy";

    public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
    {
        if (value is DateTime dateTimeValue)
        {
            return dateTimeValue.ToString(_dateTimeFormat);
        }
        return base.ConvertToString(value, row, memberMapData);
    }
}