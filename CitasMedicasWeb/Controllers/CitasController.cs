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
        public async Task<IActionResult> Index(string? filtroMedico, string? filtroPaciente, string? filtroEspecialidad, DateTime? fechaInicio, DateTime? fechaFin, string? filtroEstado, string? ver)
        {
            var q = _ctx.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico).ThenInclude(m => m.Especialidad)
                .AsQueryable();

            var usuario = await _userManager.GetUserAsync(User);

            //PACIENTE SUS CITAS
            if (User.IsInRole("Paciente"))
            {
                var paciente = await _ctx.Pacientes.FirstOrDefaultAsync(p => p.Email == usuario.Email);
                if (paciente != null)
                    q = q.Where(c => c.PacienteId == paciente.Id);

                if (ver == "proximas")
                    q = q.Where(c => c.FechaHora >= DateTime.Now);
                else if (ver == "pasadas")
                    q = q.Where(c => c.FechaHora < DateTime.Now);
            }
            // MEDICO AGENDA DEL DIA
            else if (User.IsInRole("Medico"))
            {
                var medico = await _ctx.Medicos.FirstOrDefaultAsync(m => m.Email == usuario.Email);
                if (medico != null)
                    q = q.Where(c => c.MedicoId == medico.Id && c.FechaHora.Date == DateTime.Today);
            }

            // Admin FILTROS
            if (!string.IsNullOrWhiteSpace(filtroMedico))
                q = q.Where(c => c.Medico.NombresCompletos.Contains(filtroMedico));

            if (!string.IsNullOrWhiteSpace(filtroPaciente))
                q = q.Where(c => c.Paciente.Nombres.Contains(filtroPaciente)
                              || c.Paciente.Apellidos.Contains(filtroPaciente));

            if (!string.IsNullOrWhiteSpace(filtroEspecialidad))
                q = q.Where(c => c.Medico.Especialidad.Nombre.Contains(filtroEspecialidad));

            if (fechaInicio.HasValue)
                q = q.Where(c => c.FechaHora >= fechaInicio.Value);

            if (fechaFin.HasValue)
                q = q.Where(c => c.FechaHora <= fechaFin.Value);

            if (!string.IsNullOrWhiteSpace(filtroEstado))
                q = q.Where(c => c.Estado == filtroEstado);

            var lista = await q.OrderBy(c => c.FechaHora).ToListAsync();

            ViewBag.FiltroMedico = filtroMedico;
            ViewBag.FiltroPaciente = filtroPaciente;
            ViewBag.FiltroEspecialidad = filtroEspecialidad;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.FiltroEstado = filtroEstado;
            ViewBag.Ver = ver;

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

            ViewBag.Especialidades = await _ctx.Especialidades.OrderBy(e => e.Nombre).ToListAsync();
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

            // Validaciones
            if (FechaHora < DateTime.Now)
                ModelState.AddModelError("FechaHora", "No se pueden agendar citas en el pasado.");

            bool duplicada = await _ctx.Citas.AnyAsync(c =>
                c.PacienteId == pacienteId &&
                c.MedicoId == MedicoId &&
                c.FechaHora.Date == FechaHora.Date &&
                c.Estado != "Cancelada");

            if (duplicada)
                ModelState.AddModelError("FechaHora", "Ya existe una cita con este médico en esa fecha.");

            if (string.IsNullOrWhiteSpace(Motivo))
                ModelState.AddModelError("Motivo", "El motivo es obligatorio.");

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
                Motivo = Motivo!,
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
                ModelState.AddModelError(nameof(cita.FechaHora), "El médico ya tiene una cita en ese horario.");

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