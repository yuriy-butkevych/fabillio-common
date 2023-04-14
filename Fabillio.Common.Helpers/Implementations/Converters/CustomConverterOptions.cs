namespace Fabillio.Common.Helpers.Implementations.Converters;

public class CustomConverterOptions
{
    public bool DateTime { get; set; }
    public bool Decimal { get; set; }

    public CustomConverterOptions IncludeAll()
    {
        DateTime = true;
        Decimal = true;

        return this;
    }
}
