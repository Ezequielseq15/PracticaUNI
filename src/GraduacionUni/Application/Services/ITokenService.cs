using GraduacionUni.Domain.Entities;

namespace GraduacionUni.Application.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
