using FluentAssertions;
using FluentAssertions.Primitives;

namespace DomainTests;

public static class FluentExtensions
{
    /// <summary>
    /// BeEquivalentTo with the additional constraint that concrete types must be the same.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assertions"></param>
    /// <param name="expected"></param>
    /// <param name="because"></param>
    /// <param name="becauseArgs"></param>
    /// <returns></returns>
    public static AndConstraint<ObjectAssertions> BeStrictlyEquivalentTo<T>(
        this ObjectAssertions assertions, 
        T expected, 
        string because = "", 
        params object[] becauseArgs)
    {
        // Check that the actual object is of the same type as the expected object
        assertions.BeOfType(expected.GetType(), because, becauseArgs);

        // Check that the actual object is equivalent to the expected object
        return assertions.BeEquivalentTo(expected, options => options.RespectingRuntimeTypes(), because, becauseArgs);
    }
}