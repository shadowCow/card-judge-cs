using Domain.Ports;

namespace Domain.Adapters;

public class LoggerInMemory : ILogger
{
    private readonly List<string> _messages;

    public LoggerInMemory()
    {
        _messages = [];
    }

    public void Info(string message)
    {
        _messages.Add(message);
    }
}