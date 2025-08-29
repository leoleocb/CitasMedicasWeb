using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicasWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HorariosMedicosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HorariosMedicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: HorariosMedicos
        public async Task<IActionResult> Index()
        {
            var horarios = _context.HorariosMedicos
                .Include(h => h.Medico)
                .ThenInclude(m => m.Especialidad);
            return View(await horarios.ToListAsync());
        }

        // GET: HorariosMedicos/Create
        public IActionResult Create()
        {
            ViewData["MedicoId"] = new SelectList(_context.Medicos.Include(m => m.Especialidad),
                                                  "Id", "NombresCompletos");
            return View();
        }

        // POST: HorariosMedicos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedicoId,DiaSemana,HoraInicio,HoraFin")] HorarioMedico horario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(horario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedicoId"] = new SelectList(_context.Medicos, "Id", "NombresCompletos", horario.MedicoId);
            return View(horario);
        }

        // GET: HorariosMedicos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var horario = await _context.HorariosMedicos.FindAsync(id);
            if (horario == null) return NotFound();

            ViewData["MedicoId"] = new SelectList(_context.Medicos, "Id", "NombresCompletos", horario.MedicoId);
            return View(horario);
        }

        // POST: HorariosMedicos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MedicoId,DiaSemana,HoraInicio,HoraFin")] HorarioMedico horario)
        {
            if (id != horario.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(horario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedicoId"] = new SelectList(_context.Medicos, "Id", "NombresCompletos", horario.MedicoId);
            return View(horario);
        }

        // GET: HorariosMedicos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var horario = await _context.HorariosMedicos
                .Include(h => h.Medico)
                .ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (horario == null) return NotFound();

            return View(horario);
        }

        // POST: HorariosMedicos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var horario = await _context.HorariosMedicos.FindAsync(id);
            if (horario != null)
            {
                _context.HorariosMedicos.Remove(horario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}