namespace ExampleApi.Common.Results;

/// <summary>
/// The outcome of an operation: either success, or failure carrying an <see cref="Error"/>.
/// Handlers return this instead of throwing for expected business failures.
/// </summary>
public class Result
{
    /// <summary>Initialises a new <see cref="Result"/>.</summary>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failing result must carry an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error, or <see cref="Error.None"/> when successful.</summary>
    public Error Error { get; }

    /// <summary>Creates a successful result with no value.</summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>Creates a failed result.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>Creates a successful result carrying a value.</summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>Creates a failed result of the given value type.</summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

/// <summary>
/// A <see cref="Result"/> that carries a value on success.
/// </summary>
/// <typeparam name="TValue">The success value type.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>Initialises a new <see cref="Result{TValue}"/>.</summary>
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error) => _value = value;

    /// <summary>Gets the success value; throws when the result is a failure.</summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    /// <summary>Implicitly wraps a value into a successful result.</summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}
