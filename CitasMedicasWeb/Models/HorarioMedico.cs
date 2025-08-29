using System.ComponentModel.DataAnnotations;

namespace CitasMedicasWeb.Models
{
    public class HorarioMedico
    {
        public int Id { get; set; }

        [Required]
        public int MedicoId { get; set; }
        public Medico? Medico { get; set; }

        [Required(ErrorMessage = "Debe indicar el día de la semana (1=Lunes ... 7=Domingo)")]
        [Range(1, 7, ErrorMessage = "El día debe estar entre 1 y 7 (1=Lunes ... 7=Domingo)")]
        public int DiaSemana { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        public TimeSpan HoraInicio { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria")]
        public TimeSpan HoraFin { get; set; }
    }
}