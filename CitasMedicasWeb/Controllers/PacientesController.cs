using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace CitasMedicasWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PacientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PacientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pacientes
        public async Task<IActionResult> Index()
            => View(await _context.Pacientes.ToListAsync());

        // GET: Pacientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(m => m.Id == id);
            if (paciente == null) return NotFound();

            return View(paciente);
        }

        // GET: Pacientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente == null) return NotFound();

            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombres,Apellidos,Documento,Telefono,Email,FechaNacimiento")] Paciente paciente)
        {
            if (id != paciente.Id) return NotFound();

            // Validar duplicados
            if (await _context.Pacientes.AnyAsync(p => p.Documento == paciente.Documento && p.Id != paciente.Id))
                ModelState.AddModelError("Documento", "El documento ya está registrado.");

            if (await _context.Pacientes.AnyAsync(p => p.Email == paciente.Email && p.Id != paciente.Id))
                ModelState.AddModelError("Email", "El correo ya está registrado.");

            if (!ModelState.IsValid) return View(paciente);

            try
            {
                _context.Update(paciente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Pacientes.Any(e => e.Id == paciente.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Pacientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var paciente = await _context.Pacientes.FirstOrDefaultAsync(m => m.Id == id);
            if (paciente == null) return NotFound();

            return View(paciente);
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);
            if (paciente != null)
                _context.Pacientes.Remove(paciente);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}