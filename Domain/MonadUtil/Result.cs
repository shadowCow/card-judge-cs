namespace Domain.MonadUtil;

public abstract record Result<TSuccess, TError>
{
    private Result() {}

    public sealed record SuccessResult(TSuccess Value) : Result<TSuccess, TError>;
    public sealed record ErrorResult(TError Err) : Result<TSuccess, TError>;

    public static Result<TSuccess, TError> Success(TSuccess value) => new SuccessResult(value);
    public static Result<TSuccess, TError> Error(TError error) => new ErrorResult(error);
}
