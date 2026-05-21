using ERP.Domain.Entities;

namespace ERP.Application.Interfaces;

public interface IJwtProvider
{
    string Generate(ApplicationUser user);
}
