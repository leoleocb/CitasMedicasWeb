using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace CitasMedicasWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MedicosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Medicos
        public async Task<IActionResult> Index()
            => View(await _context.Medicos.Include(m => m.Especialidad).ToListAsync());

        // GET: Medicos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medico = await _context.Medicos
                .Include(m => m.Especialidad)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico == null) return NotFound();

            return View(medico);
        }

        // GET: Medicos/Create
        public IActionResult Create()
        {
            ViewData["EspecialidadId"] = new SelectList(_context.Especialidades, "Id", "Nombre");
            return View();
        }

        // POST: Medicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombresCompletos,CMP,Email,EspecialidadId")] Medico medico)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EspecialidadId"] = new SelectList(_context.Especialidades, "Id", "Nombre", medico.EspecialidadId);
                return View(medico);
            }

            // 1. Crear usuario en Identity
            var user = new ApplicationUser
            {
                UserName = medico.Email,
                Email = medico.Email,
                EmailConfirmed = true,
                NombreMostrado = medico.NombresCompletos
            };

            var result = await _userManager.CreateAsync(user, "Medico*123"); // contraseña por defecto
            if (result.Succeeded)
            {
                // 2. Asignar rol "Medico"
                await _userManager.AddToRoleAsync(user, "Medico");

                // 3. Guardar en tabla Medicos
                _context.Add(medico);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Manejo de errores si falla la creación de usuario
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewData["EspecialidadId"] = new SelectList(_context.Especialidades, "Id", "Nombre", medico.EspecialidadId);
            return View(medico);
        }

        // GET: Medicos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var medico = await _context.Medicos.FindAsync(id);
            if (medico == null) return NotFound();

            ViewData["EspecialidadId"] = new SelectList(_context.Especialidades, "Id", "Nombre", medico.EspecialidadId);
            return View(medico);
        }

        // POST: Medicos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombresCompletos,CMP,Email,EspecialidadId")] Medico medico)
        {
            if (id != medico.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["EspecialidadId"] = new SelectList(_context.Especialidades, "Id", "Nombre", medico.EspecialidadId);
                return View(medico);
            }

            try
            {
                // 🔹 Actualizar tabla Medicos
                _context.Update(medico);
                await _context.SaveChangesAsync();

                // 🔹 Buscar usuario Identity asociado
                var user = await _userManager.FindByEmailAsync(medico.Email);
                if (user != null)
                {
                    user.Email = medico.Email;
                    user.UserName = medico.Email;
                    user.NombreMostrado = medico.NombresCompletos;

                    await _userManager.UpdateAsync(user);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Medicos.Any(e => e.Id == medico.Id))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Medicos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medico = await _context.Medicos
                .Include(m => m.Especialidad)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medico == null) return NotFound();

            return View(medico);
        }

        // POST: Medicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico != null)
            {
                // 🔹 Eliminar también el usuario Identity
                var user = await _userManager.FindByEmailAsync(medico.Email);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Medicos.Remove(medico);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}