# Contributing

# Conventions

## Record type

The `record` type is similar to a `case class` in Scala. They are ideal for creating immutable, value-based types. Provides built-in support for value equality, immutability, and concise syntax.

Example definition:

```CSharp
public record SessionResult(string sessionId, SessionOutcome outcome);
```

## Algebraic Data Types (ADTs)

Use the following pattern for ADTs in C#:

```CSharp
// the abstract record/class is the sum type
public abstract record GameSessionOutcome
{
    // the constructor is private
    // so that only the inner types
    // can derive from it.
    // in other words, all of the possible variants
    // of this type must be inside it.
    private GameSessionOutcome(){}

    // each variant is a public sealed type.
    public sealed record Winner(string PlayerId) : GameSessionOutcome;
    public sealed record Tie : GameSessionOutcome;
    public sealed record Aborted : GameSessionOutcome;
}
```

Based on [this article](https://messerli-informatik-ag.github.io/code-style/algebraic-datatypes.html).
