using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDotNet.Models;

public class EmpleadoModel
{
    [Key]
    [Column("id_empleado")]
    public int EmpleadoId { get; set; }

    [Required]
    [ForeignKey("Persona")]
    [Column("id_persona")]
    public int PersonaId { get; set; }
    

    // Constructor por defecto
    public EmpleadoModel() { }
}