using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicasWeb.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int Id { get; set; }

            [Required]
            [Display(Name = "Nombres")]
            public string Nombres { get; set; }

            [Required]
            [Display(Name = "Apellidos")]
            public string Apellidos { get; set; }

            [Required]
            [Display(Name = "Documento")]
            public string Documento { get; set; }

            [Display(Name = "Teléfono")]
            public string Telefono { get; set; }

            [Required, EmailAddress]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; }

            [DataType(DataType.Date)]
            [Display(Name = "Fecha de nacimiento")]
            public DateTime? FechaNacimiento { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Email == user.Email);

            Input = new InputModel
            {
                Id = paciente?.Id ?? 0,
                Nombres = paciente?.Nombres ?? "",
                Apellidos = paciente?.Apellidos ?? "",
                Documento = paciente?.Documento ?? "",
                Telefono = paciente?.Telefono ?? user.PhoneNumber,
                Email = user.Email,
                FechaNacimiento = paciente?.FechaNacimiento
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"No se pudo cargar el usuario con ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // 🔹 Actualizar AspNetUsers
            user.Email = Input.Email;
            user.UserName = Input.Email;
            user.NombreMostrado = Input.Nombres + " " + Input.Apellidos;
            user.PhoneNumber = Input.Telefono;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                StatusMessage = "Error al actualizar el perfil en Identity.";
                return RedirectToPage();
            }

            // 🔹 Actualizar Paciente
            var paciente = await _db.Pacientes.FirstOrDefaultAsync(p => p.Id == Input.Id);
            if (paciente != null)
            {
                paciente.Nombres = Input.Nombres;
                paciente.Apellidos = Input.Apellidos;
                paciente.Documento = Input.Documento;
                paciente.Telefono = Input.Telefono;
                paciente.Email = Input.Email;
                paciente.FechaNacimiento = Input.FechaNacimiento;

                _db.Update(paciente);
                await _db.SaveChangesAsync();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Tu perfil ha sido actualizado correctamente.";
            return RedirectToPage();
        }
    }
}