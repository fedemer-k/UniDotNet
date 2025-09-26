using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniDotNet.Models;

public class InquilinoModel
{
    [Key]
    [Column("id_inquilino")]
    public int InquilinoId { get; set; }

    [Required]
    [ForeignKey("Persona")]
    [Column("id_persona")]
    public int PersonaId { get; set; }

    // Constructor por defecto
    public InquilinoModel() { }
    
}