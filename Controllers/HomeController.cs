using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UniDotNet.Models;

namespace UniDotNet.Controllers;

// No entiendo muy bien las Inyecciones de Dependencias,
// es algo nuevo para mi y no le veo el potencial.
// Necesito investigar mas al respecto.

// Pero por lo que entiendo, es darle una orden
// al framework para que me cree un objeto
// de una clase que yo le indique, y que lo haga al crear
// el controlador, para que yo pueda usarlo en los métodos
// de la clase sin tener que crear una instancia de la clase.

//Y porque no lo hago yo mismo? que tendria mas sentido.

//Porque el framework se encarga de crear el objeto 
//y de manejar su ciclo de vida, lo que puede ser útil
//para objetos que consumen muchos recursos o que
//que necesitan ser compartidos entre diferentes partes
// de la aplicación, como conexiones a bases de datos
// o servicios web.


public class HomeController : Controller
{
    //Inyección de dependencias del logger
    //para registrar eventos en la aplicación 
    //y ayudar en la depuración
    private readonly ILogger<HomeController> _logger;

    //Constructor de la clase por defecto
    //Posee un parámetro de tipo ILogger<HomeController>
    //que es inyectado por el framework
    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    //Responde a la solicitud get default
    public IActionResult Index()
    {
        //Devuelve la vista asociada a Views/Home/Index.cshtml
        return View();
    }

    //Responde a la solicitud get /Home/Privacy
    public IActionResult Privacy()
    {
        return View();
        //Devuelve la vista asociada a Views/Home/Privacy.cshtml
    }

    //Responde a la solicitud get /Home/Error
    //Este atributo indica que la respuesta no debe ser almacenada en caché
    //para asegurar que siempre se muestre la información más reciente
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //Devuelve la vista asociada a Views/Home/Error.cshtml
        //y le pasa un modelo de vista ErrorViewModel
    }
}
