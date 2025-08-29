using System.ComponentModel.DataAnnotations;

namespace CitasMedicasWeb.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El paciente es obligatorio")]
        public int PacienteId { get; set; }
        public Paciente? Paciente { get; set; }

        [Required(ErrorMessage = "El médico es obligatorio")]
        public int MedicoId { get; set; }
        public Medico? Medico { get; set; }

        [Required(ErrorMessage = "La fecha y hora son obligatorias")]
        [DataType(DataType.DateTime)]
        [CustomValidation(typeof(Cita), nameof(ValidarFecha))]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "El motivo es obligatorio")]
        [StringLength(200, ErrorMessage = "El motivo no puede superar los 200 caracteres")]
        public string Motivo { get; set; } = string.Empty;

        [StringLength(30)]
        public string Estado { get; set; } = "Pendiente";

 
        public static ValidationResult? ValidarFecha(DateTime fecha, ValidationContext context)
        {
            if (fecha < DateTime.Now)
                return new ValidationResult("No se pueden registrar citas en el pasado");
            return ValidationResult.Success;
        }
    }
}