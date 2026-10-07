namespace GraduacionUni.Application.Services;

public enum ServiceError { None, NotFound, Forbidden, Invalid }

/// <summary>Resultado de una operación de negocio; el controlador lo traduce a HTTP.</summary>
public sealed record ServiceResult<T>(T? Value, ServiceError Error = ServiceError.None, string? Message = null)
{
    public bool Succeeded => Error == ServiceError.None;

    public static ServiceResult<T> Ok(T value) => new(value);
    public static ServiceResult<T> Fail(ServiceError error, string message) => new(default, error, message);
}
