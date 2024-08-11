using Domain.Ports;

namespace Domain.Adapters;

public class GuidServiceFixed(string value) : IGuidService
{
    public Guid NewGuid()
    {
        return Guid.Parse(value);
    }

    public const string allZeroGuid = "00000000-0000-0000-0000-000000000000";

    public static GuidServiceFixed AllZero()
    {
        return new GuidServiceFixed(allZeroGuid);
    }
}