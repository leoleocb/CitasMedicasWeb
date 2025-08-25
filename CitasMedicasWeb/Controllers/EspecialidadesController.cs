using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace CitasMedicasWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EspecialidadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EspecialidadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Especialidades
        public async Task<IActionResult> Index()
            => View(await _context.Especialidades.ToListAsync());

        // GET: Especialidades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var especialidad = await _context.Especialidades.FirstOrDefaultAsync(m => m.Id == id);
            if (especialidad == null) return NotFound();

            return View(especialidad);
        }

        // GET: Especialidades/Create
        public IActionResult Create() => View();

        // POST: Especialidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion")] Especialidad especialidad)
        {
            if (!ModelState.IsValid) return View(especialidad);

            _context.Add(especialidad);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Especialidades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad == null) return NotFound();

            return View(especialidad);
        }

        // POST: Especialidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion")] Especialidad especialidad)
        {
            if (id != especialidad.Id) return NotFound();

            if (!ModelState.IsValid) return View(especialidad);

            try
            {
                _context.Update(especialidad);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Especialidades.Any(e => e.Id == especialidad.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Especialidades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var especialidad = await _context.Especialidades.FirstOrDefaultAsync(m => m.Id == id);
            if (especialidad == null) return NotFound();

            return View(especialidad);
        }

        // POST: Especialidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var especialidad = await _context.Especialidades.FindAsync(id);
            if (especialidad != null)
                _context.Especialidades.Remove(especialidad);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}