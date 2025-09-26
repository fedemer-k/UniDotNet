// DataAnnotations para validaciones que se aplican en modelos y se usan en vistas. (required, stringlength, regex, email, phone, etc)
using System.ComponentModel.DataAnnotations;

namespace UniDotNet.Models;

//Es tentador deja la clase PersonaModel como abstracta 
//e implementar herencias como:
//  - PropietarioModel : PersonaModel
//  - InquilinoModel : PersonaModel
//  - EmpleadoModel : PersonaModel
//Pero no tiene sentido, ya que una persona puede ser varias cosas a la vez.
//Por ejemplo, una alta de propietario requiere datos de persona,
//y esa misma persona puede ser inquilino, lo cual requeriria 
//una alta de inquilino requiriendo otra vez alta de persona con los mismos datos.
//Esto generaria redundancia.

/// <summary>
/// Modelo que representa una persona.
/// Los datos de esta clase se cargan desde la clase RepositorioPersona.cs
/// </summary>
public class PersonaModel
{
    public required int PersonaId { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(8, MinimumLength = 6, ErrorMessage = "El DNI debe tener entre 6 y 8 caracteres.")]
    [RegularExpression("^[0-9]*$", ErrorMessage = "El DNI solo puede contener números.")]
    public required string Dni { get; set; }

    [Required(ErrorMessage = "El Apellido es obligatorio.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "El apellido debe tener entre 4 y 50 caracteres.")]
    [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "El apellido solo puede contener letras y espacios.")]
    public required string Apellido { get; set; }

    [Required(ErrorMessage = "El Nombre es obligatorio.")]
    [StringLength(50, MinimumLength = 4, ErrorMessage = "El nombre debe tener entre 4 y 50 caracteres.")]
    [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public required string Nombre { get; set; }

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "El teléfono no es válido")]
    public required string Telefono { get; set; }

    [Required(ErrorMessage = "El Email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El Email no tiene un formato válido.")]
    public required string Email { get; set; }

    /* Estoy dudando de implementar la direccion o no.
     * Por un lado, es un dato que puede ser util en muchos casos.
     * Por otro lado, complica la carga de datos y puede ser un dato que no siempre se tenga.
     * Por ahora lo dejo comentado.
     * Si se implementa, hay que agregarlo a los constructores y al ToString().
    [Required(ErrorMessage = "La direccion es obligatoria")]
    [StringLength(255, MinimumLength = 5, ErrorMessage = "La direccion debe tener entre 5 y 255 caracteres.")]
    public string Direccion { get; set; } = "";
    */

    //No tiene ningun sentido cargar a los objetos con estados propios de la base de datos.
    //Debido a que estado se utiliza en db y es util para el borrado logico en db
    //no tiene ninguna utilidad en el paradigma orientado a objetos.
    //public bool Estado { get; set; }

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
        //Direccion = direccion;
    }

    //Metodo ToString
    public override string ToString()
    {
        return $"{Apellido} + {Nombre}";
    }

}
