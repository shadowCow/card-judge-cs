using Domain.Ports;

namespace Domain.Adapters;

public class LoggerNoOp : ILogger
{
    public void Info(string message)
    {
        // no-op
    }
}