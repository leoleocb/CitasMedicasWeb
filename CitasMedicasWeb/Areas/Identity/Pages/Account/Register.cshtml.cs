using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CitasMedicasWeb.Models;
using CitasMedicasWeb.Data;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;

    public RegisterModel(UserManager<ApplicationUser> userManager,
                         SignInManager<ApplicationUser> signInManager,
                         ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;
    public string? ReturnUrl { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

    public class InputModel
    {
        [Required, Display(Name = "Nombres")]
        public string Nombres { get; set; } = string.Empty;

        [Required, Display(Name = "Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Required, StringLength(12), Display(Name = "Documento")]
        public string Documento { get; set; } = string.Empty;

        [StringLength(15), Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [DataType(DataType.Date), Display(Name = "Fecha de Nacimiento")]
        public DateTime? FechaNacimiento { get; set; }

        [Required, EmailAddress, Display(Name = "Correo")]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, ErrorMessage = "La {0} debe tener al menos {2} y como máximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password), Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password), Display(Name = "Confirmar contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y la confirmación no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
            return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            EmailConfirmed = true,
            NombreMostrado = Input.Nombres + " " + Input.Apellidos
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (result.Succeeded)
        {
            // Rol por defecto: Paciente
            await _userManager.AddToRoleAsync(user, "Paciente");

            // 🔹 Agregar también a la tabla Pacientes
            var nuevoPaciente = new Paciente
            {
                Nombres = Input.Nombres,
                Apellidos = Input.Apellidos,
                Documento = Input.Documento,
                Telefono = Input.Telefono,
                Email = Input.Email,
                FechaNacimiento = Input.FechaNacimiento
            };
            _db.Pacientes.Add(nuevoPaciente);
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return Page();
    }
}