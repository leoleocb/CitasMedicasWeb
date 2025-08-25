using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CitasMedicasWeb.Data;
using CitasMedicasWeb.Models;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return View(); 
        }

    
        var user = await _userManager.GetUserAsync(User);

        if (User.IsInRole("Admin"))
        {
            ViewBag.PacientesCount = await _context.Pacientes.CountAsync();
            ViewBag.MedicosCount = await _context.Medicos.CountAsync();
            ViewBag.EspecialidadesCount = await _context.Especialidades.CountAsync();
            ViewBag.CitasCount = await _context.Citas.CountAsync();
            return View("DashboardAdmin");
        }
        else if (User.IsInRole("Medico"))
        {
            var medico = await _context.Medicos.FirstOrDefaultAsync(m => m.Email == user.Email);
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Where(c => c.MedicoId == medico.Id && c.FechaHora >= DateTime.Now)
                .OrderBy(c => c.FechaHora)
                .Take(5)
                .ToListAsync();
            return View("DashboardMedico", citas);
        }
        else if (User.IsInRole("Paciente"))
        {
            var paciente = await _context.Pacientes.FirstOrDefaultAsync(p => p.Email == user.Email);
            var citas = await _context.Citas
                .Include(c => c.Medico).ThenInclude(m => m.Especialidad)
                .Where(c => c.PacienteId == paciente.Id && c.FechaHora >= DateTime.Now)
                .OrderBy(c => c.FechaHora)
                .Take(5)
                .ToListAsync();
            return View("DashboardPaciente", citas);
        }

        return View();
    }
}