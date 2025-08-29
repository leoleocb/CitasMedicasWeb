using System.ComponentModel.DataAnnotations;

namespace CitasMedicasWeb.Models
{
    public class Medico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(120)]
        public string NombresCompletos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CMP es obligatorio")]
        [RegularExpression(@"^\d+$", ErrorMessage = "El CMP debe contener solo números")]
        [StringLength(15)]
        public string CMP { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una especialidad")]
        public int EspecialidadId { get; set; }
        public Especialidad? Especialidad { get; set; }
    }
}