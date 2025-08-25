using System.ComponentModel.DataAnnotations;

namespace CitasMedicasWeb.Models
{
    public class Medico
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string NombresCompletos { get; set; } = string.Empty;

        [Required, StringLength(15)]
        public string CMP { get; set; } = string.Empty;

        [Required, EmailAddress]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        public int EspecialidadId { get; set; }
        public Especialidad? Especialidad { get; set; }
    }
}