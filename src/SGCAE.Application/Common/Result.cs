namespace SGCAE.Application.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}

/// <summary>
/// Resultado de un caso de uso: valor en caso de éxito o error tipado en caso de fallo.
/// Evita usar excepciones para el flujo de control esperado.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;

    private Result(T value) { _value = value; IsSuccess = true; }
    private Result(Error error) { Error = error; }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
