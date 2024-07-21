using System;

namespace Domain.Services;

public interface IGuidService
{
    Guid NewGuid();
}

public class GuidService : IGuidService
{
    public GuidService()
    {

    }

    public Guid NewGuid()
    {
        return Guid.NewGuid();
    }
}
