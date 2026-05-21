using Microsoft.AspNetCore.Identity;

namespace ERP.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
