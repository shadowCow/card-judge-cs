using System.Diagnostics;
using ServiceTests.Util;

namespace ServiceTests.Gwt;

public class Then
{
    public static Within Within(TimeSpan timeSpan)
    {
        return new(timeSpan);
    }

    public static Within WithinAShortTime()
    {
        return new(Time.AShortTime);
    }

    public static Within WithinADebugTime()
    {
        return new(Time.ADebugTime);
    }
}

public class Within(TimeSpan timeSpan)
{
    public void Validate(Action validation)
    {
        var stopwatch = Stopwatch.StartNew();
        Exception? exception;

        do
        {
            try
            {
                validation();
                exception = null;
                break;
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                Thread.Sleep(10);
            }
        }
        while (stopwatch.Elapsed < timeSpan);

        if (exception is not null)
        {
            throw new TimeoutException(
                message: "Validation timed out.",
                innerException: exception
            );
        }
    }
}