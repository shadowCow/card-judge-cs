using System;

namespace Domain.Ports;

public interface IGuidService
{
    Guid NewGuid();
}
