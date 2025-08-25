using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicasWeb.Controllers
{
    [Authorize(Roles = "Admin,Paciente,Medico")]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public CitasController(ApplicationDbContext ctx, UserManager<ApplicationUser> userManager)
        {
            _ctx = ctx;
            _userManager = userManager;
        }

        // GET: /Citas
        public async Task<IActionResult> Index(string? filtroMedico, string? filtroPaciente, DateTime? fecha)
        {
            var q = _ctx.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico).ThenInclude(m => m.Especialidad)
                .AsQueryable();

            var usuario = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Paciente"))
            {
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente != null)
                    q = q.Where(c => c.PacienteId == paciente.Id);
            }
            else if (User.IsInRole("Medico"))
            {
                var medico = await _ctx.Medicos.FirstOrDefaultAsync(m => m.Email == usuario.Email);
                if (medico != null)
                    q = q.Where(c => c.MedicoId == medico.Id);
            }

            if (!string.IsNullOrWhiteSpace(filtroMedico))
                q = q.Where(c => c.Medico.NombresCompletos.Contains(filtroMedico));

            if (!string.IsNullOrWhiteSpace(filtroPaciente))
                q = q.Where(c => c.Paciente.Nombres.Contains(filtroPaciente)
                              || c.Paciente.Apellidos.Contains(filtroPaciente));

            if (fecha.HasValue)
                q = q.Where(c => c.FechaHora.Date == fecha.Value.Date);

            var lista = await q.OrderBy(c => c.FechaHora).ToListAsync();

            ViewBag.FiltroMedico = filtroMedico;
            ViewBag.FiltroPaciente = filtroPaciente;
            ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");

            return View(lista);
        }

        // GET: /Citas/Crear
        [Authorize(Roles = "Admin,Paciente")]
        public async Task<IActionResult> Crear()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Pacientes = await _ctx.Pacientes.OrderBy(p => p.Apellidos).ToListAsync();
            }
            else if (User.IsInRole("Paciente"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente != null)
                    ViewBag.PacienteId = paciente.Id;
            }

            // Lista de especialidades para el combo
            ViewBag.Especialidades = await _ctx.Especialidades
                                              .OrderBy(e => e.Nombre)
                                              .ToListAsync();

            // Inicialmente sin médicos
            ViewBag.Medicos = new List<Medico>();

            return View();
        }

        // POST: /Citas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Paciente")]
        public async Task<IActionResult> Crear(int MedicoId, DateTime FechaHora, string? Motivo)
        {
            int pacienteId = 0;

            if (User.IsInRole("Paciente"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente == null) return Unauthorized();

                pacienteId = paciente.Id;
            }
            else if (User.IsInRole("Admin"))
            {
                if (int.TryParse(Request.Form["PacienteId"], out int idForm))
                    pacienteId = idForm;
            }

            bool ocupado = await _ctx.Citas.AnyAsync(c =>
                c.MedicoId == MedicoId &&
                c.FechaHora == FechaHora &&
                c.Estado != "Cancelada");

            if (ocupado)
            {
                ModelState.AddModelError(nameof(FechaHora), "El médico ya tiene una cita en ese horario.");
            }

            if (!ModelState.IsValid)
            {
                if (User.IsInRole("Admin"))
                    ViewBag.Pacientes = await _ctx.Pacientes.OrderBy(p => p.Apellidos).ToListAsync();

                ViewBag.Especialidades = await _ctx.Especialidades.OrderBy(e => e.Nombre).ToListAsync();
                return View();
            }

            _ctx.Citas.Add(new Cita
            {
                PacienteId = pacienteId,
                MedicoId = MedicoId,
                FechaHora = FechaHora,
                Motivo = Motivo,
                Estado = "Pendiente"
            });

            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // AJAX: obtener médicos según especialidad
        [HttpGet]
        public async Task<IActionResult> GetMedicosByEspecialidad(int especialidadId)
        {
            var medicos = await _ctx.Medicos
                .Where(m => m.EspecialidadId == especialidadId)
                .Select(m => new { m.Id, m.NombresCompletos })
                .ToListAsync();

            return Json(medicos);
        }

        // GET: /Citas/Detalles/5
        public async Task<IActionResult> Detalles(int id)
        {
            var cita = await _ctx.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico).ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            var usuario = await _userManager.GetUserAsync(User);

            if (User.IsInRole("Paciente"))
            {
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente == null || cita.PacienteId != paciente.Id)
                    return Forbid();
            }
            else if (User.IsInRole("Medico"))
            {
                var medico = await _ctx.Medicos.FirstOrDefaultAsync(m => m.Email == usuario.Email);
                if (medico == null || cita.MedicoId != medico.Id)
                    return Forbid();
            }

            return View(cita);
        }

        // GET: /Citas/Editar/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int id)
        {
            var cita = await _ctx.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            ViewBag.Pacientes = await _ctx.Pacientes.OrderBy(p => p.Apellidos).ToListAsync();
            ViewBag.Especialidades = await _ctx.Especialidades.OrderBy(e => e.Nombre).ToListAsync();
            ViewBag.Medicos = await _ctx.Medicos.Include(m => m.Especialidad).ToListAsync();

            return View(cita);
        }

        // POST: /Citas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int id, Cita cita)
        {
            if (id != cita.Id) return BadRequest();

            bool ocupado = await _ctx.Citas.AnyAsync(c =>
                c.MedicoId == cita.MedicoId &&
                c.FechaHora == cita.FechaHora &&
                c.Id != cita.Id &&
                c.Estado != "Cancelada");

            if (ocupado)
            {
                ModelState.AddModelError(nameof(cita.FechaHora), "El médico ya tiene una cita en ese horario.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _ctx.Pacientes.OrderBy(p => p.Apellidos).ToListAsync();
                ViewBag.Especialidades = await _ctx.Especialidades.OrderBy(e => e.Nombre).ToListAsync();
                ViewBag.Medicos = await _ctx.Medicos.Include(m => m.Especialidad).ToListAsync();
                return View(cita);
            }

            _ctx.Update(cita);
            await _ctx.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Citas/Eliminar/5
        [Authorize(Roles = "Admin,Paciente")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cita = await _ctx.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico).ThenInclude(m => m.Especialidad)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null) return NotFound();

            if (User.IsInRole("Paciente"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente == null || cita.PacienteId != paciente.Id)
                    return Forbid();
            }

            return View(cita);
        }

        // POST: /Citas/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Paciente")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var cita = await _ctx.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            if (User.IsInRole("Paciente"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente == null || cita.PacienteId != paciente.Id)
                    return Forbid();
            }

            _ctx.Citas.Remove(cita);
            await _ctx.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}