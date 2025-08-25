using Microsoft.AspNetCore.Identity;

namespace CitasMedicasWeb.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreMostrado { get; internal set; }
    }
}
