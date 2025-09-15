//Define que la clase ErrorViewModel 
//pertenece al espacio de nombres UniDotNet.Models
namespace UniDotNet.Models;

//clase por defecto ErrorViewModel (modelo para manejar errores).
public class ErrorViewModel
{
    //Propiedad que almacena el ID de la solicitud HTTP (puede ser null gracias al ?).
    public string? RequestId { get; set; }

    //Propiedad solo de lectura "=>" con getter automatico.
    /* Equivalente a:
    public bool ShowRequestId
    {
        get { return !string.IsNullOrEmpty(RequestId); }
    }
    sirve para determinar si se debe mostrar el RequestId en la vista de error.
    */
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
