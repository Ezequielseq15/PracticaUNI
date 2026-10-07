using GraduacionUni.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GraduacionUni.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Traduce un error de negocio al código HTTP correspondiente.</summary>
    protected IActionResult ToErrorResult<T>(ServiceResult<T> result) => result.Error switch
    {
        ServiceError.NotFound => NotFound(new { message = result.Message }),
        ServiceError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),
        ServiceError.Invalid => BadRequest(new { message = result.Message }),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
