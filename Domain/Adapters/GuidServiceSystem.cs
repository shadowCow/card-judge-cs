using Domain.Ports;

namespace Domain.Adapters;

public class GuidServiceSystem : IGuidService
{
    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}