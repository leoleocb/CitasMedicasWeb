using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitasMedicasWeb.Models
{
    public class Paciente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(120)]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(120)]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos")]
        public string Documento { get; set; } = string.Empty;

        [RegularExpression(@"^9\d{8}$", ErrorMessage = "El celular debe tener 9 dígitos y empezar con 9")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }
    }
}