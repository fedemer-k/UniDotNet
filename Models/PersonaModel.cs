using System.ComponentModel.DataAnnotations;

namespace UniDotNet.Models;

public class PersonaModel
{
    //RequestID sirve para identificar la solicitud en los logs
    public string? RequestId { get; set; }

    //ShowRequestId indica si se debe mostrar el RequestID en la vista de error
    public required int PersonaId { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [RegularExpression(@"^\d{7,8}$", ErrorMessage = "El DNI debe contener 7 u 8 dígitos numéricos.")] //Insertare una expresión regular para validar los campos cuando termine todo inmueble
    public required string Dni { get; set; }

    [Required(ErrorMessage = "El Apellido es obligatorio.")]
    public required string Apellido { get; set; }

    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    public required string Nombre { get; set; }

    [Required(ErrorMessage = "El Teléfono es obligatorio.")]
    public required string Telefono { get; set; }

    [Required(ErrorMessage = "El Email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El Email no tiene un formato válido.")]
    public required string Email { get; set; }

    //Esto se deja solo para no romper otras funcionalidades, PERO
    //No tiene ningun sentido cargar a los objetos con estados propios de la base de datos.
    //Debido a que estado se utiliza en db y es util para el borrado logico en db
    //no tiene ninguna utilidad en el paradigma orientado a objetos.
    //public bool Estado { get; set; }

    // Propiedades no mapeadas para roles
    //[NotMapped]
    //public List<string> TipoPersona { get; set; } = new();

    // Constructor por defecto
    public PersonaModel() { }

    // Constructor completo
    public PersonaModel(int personaId, string dni, string apellido, string nombre, string telefono, string email)
    {
        PersonaId = personaId;
        Dni = dni;
        Apellido = apellido;
        Nombre = nombre;
        Telefono = telefono;
        Email = email;
        //TipoPersona = new List<string>();
    }
    
    //Metodo ToString
    public override string ToString()
    {
        return $"{Apellido} + {Nombre}";
    }

}
