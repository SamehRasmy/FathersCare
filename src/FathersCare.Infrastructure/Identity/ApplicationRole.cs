using Microsoft.AspNetCore.Identity;

namespace FathersCare.Infrastructure.Identity;

public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
}
