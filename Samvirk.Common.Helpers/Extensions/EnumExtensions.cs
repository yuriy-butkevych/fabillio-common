using System;
using System.ComponentModel;

namespace Fabillio.Common.Helpers.Extensions;

public static class EnumExtensions
{
    public static string GetEnumDescription(this Enum enumValue)
    {
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

        var descriptionAttributes = (DescriptionAttribute[])
            fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return descriptionAttributes.Length > 0
            ? descriptionAttributes[0].Description
            : enumValue.ToString();
    }

    public static TEnum ReadEnumValue<TEnum>(string strValue) where TEnum : struct
    {
        if (TryReadEnumValue(strValue, out TEnum value))
        {
            return value;
        }

        throw new ArgumentException("Failed to parse value to enum");
    }

    public static bool TryReadEnumValue<TEnum>(string strValue, out TEnum value)
        where TEnum : struct
    {
        var fields = typeof(TEnum).GetFields();
        value = new TEnum();

        foreach (var field in fields)
        {
            var attributes = (DescriptionAttribute[])
                field.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (
                field.Name == strValue
                || (attributes.Length > 0 && attributes[0].Description == strValue)
            )
            {
                value = (TEnum)Enum.Parse(typeof(TEnum), field.Name);
                return true;
            }
        }

        return false;
    }
}
