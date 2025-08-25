using System.ComponentModel.DataAnnotations;

namespace CitasMedicasWeb.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }
        public Paciente? Paciente { get; set; }

        [Required]
        public int MedicoId { get; set; }
        public Medico? Medico { get; set; }

        [Required, DataType(DataType.DateTime)]
        public DateTime FechaHora { get; set; }

        [StringLength(200)]
        public string? Motivo { get; set; }

        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";
    }
}