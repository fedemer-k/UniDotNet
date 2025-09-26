using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDotNet.Models;

public class PropietarioModel
{
    [Key]
    [Column("id_propietario")]
    public int PropietarioId { get; set; }

    [Required]
    [ForeignKey("Persona")]
    [Column("id_persona")]
    public int PersonaId { get; set; }

    
    // Constructor por defecto
    public PropietarioModel() { }

}