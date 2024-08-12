using Domain.Ports;

namespace Domain.Adapters;

public class LoggerConsole : ILogger
{
    public void Info(string message)
    {
        Console.WriteLine(message);
    }
}