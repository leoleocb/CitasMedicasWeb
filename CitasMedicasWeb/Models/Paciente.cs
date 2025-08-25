using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CitasMedicasWeb.Models
{
    public class Paciente
    {
        public int Id { get; set; }
        [Required, StringLength(120)] public string Nombres { get; set; } = string.Empty;
        [Required, StringLength(120)] public string Apellidos { get; set; } = string.Empty;
        [Required, StringLength(12)] public string Documento { get; set; } = string.Empty;
        [StringLength(15)] public string? Telefono { get; set; }
        [EmailAddress] public string? Email { get; set; }

        [DataType(DataType.Date)]
        [Column(TypeName ="date")]
        public DateTime? FechaNacimiento { get; set; }
    }
}