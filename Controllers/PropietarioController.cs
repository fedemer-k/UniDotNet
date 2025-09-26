using Microsoft.AspNetCore.Mvc;
using UniDotNet.Models;
using UniDotNet.Repository;

namespace UniDotNet.Controllers;

/// <summary>
/// Controlador para la gestión de propietarios.
/// Implementa ABMC (Alta, Baja, Modificación, Consulta) para propietarios.
/// </summary>
public class PropietarioController : Controller
{
    private readonly IPropietarioRepositorio _propietarioRepo;
    private readonly IPersonaRepositorio _personaRepo;

    public PropietarioController(IPropietarioRepositorio propietarioRepo, IPersonaRepositorio personaRepo)
    {
        _propietarioRepo = propietarioRepo;
        _personaRepo = personaRepo;
    }

    #region CONSULTA (Listado con paginación)

    /// <summary>
    /// Acción principal: Lista propietarios con paginación y búsqueda
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Elementos por página</param>
    /// <param name="search">Término de búsqueda</param>
    /// <returns>Vista con lista paginada de propietarios</returns>
    public IActionResult Index(int page = 1, int pageSize = 10, string search = null)
    {
        try
        {
            var (personas, total) = _propietarioRepo.ObtenerTodosConPaginacion(page, pageSize, search);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRegistros = total;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Search = search;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPaginas;
            
            return View(personas);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la lista de propietarios: {ex.Message}";
            return View(new List<PersonaModel>());
        }
    }

    #endregion

    #region CONSULTA (Detalle individual)

    /// <summary>
    /// Muestra los detalles completos de un propietario específico
    /// </summary>
    /// <param name="id">ID del propietario</param>
    /// <returns>Vista con detalles del propietario</returns>
    public IActionResult Details(int id)
    {
        try
        {
            var propietario = _propietarioRepo.ObtenerPorId(id);
            if (propietario == null)
            {
                TempData["Error"] = "Propietario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos completos de la persona
            var persona = _personaRepo.ObtenerPorId(propietario.PersonaId);
            if (persona == null)
            {
                TempData["Error"] = "No se encontraron los datos de la persona asociada.";
                return RedirectToAction(nameof(Index));
            }

            // Crear modelo combinado para la vista
            var modelo = new
            {
                PropietarioId = propietario.PropietarioId,
                PersonaId = propietario.PersonaId,
                Dni = persona.Dni,
                Apellido = persona.Apellido,
                Nombre = persona.Nombre,
                Telefono = persona.Telefono,
                Email = persona.Email,
                NombreCompleto = $"{persona.Apellido}, {persona.Nombre}"
            };

            return View(modelo);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar los detalles del propietario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region ALTA (Crear nuevo propietario)

    /// <summary>
    /// Muestra el formulario para crear un nuevo propietario
    /// </summary>
    /// <returns>Vista con formulario de alta</returns>
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            return View(new PersonaModel 
            { 
                PersonaId = 0, 
                Dni = "", 
                Apellido = "", 
                Nombre = "", 
                Telefono = "", 
                Email = "" 
            });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa el alta de un nuevo propietario
    /// </summary>
    /// <param name="model">Modelo con datos del propietario</param>
    /// <returns>Redirección según resultado</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(PersonaModel personaModel)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(personaModel);
            }

            // 1. CREAR LA PERSONA PRIMERO
            int personaId = _personaRepo.Alta(personaModel);
            
            // Verificar si se creó correctamente
            if (personaId <= 0)
            {
                switch (personaId)
                {
                    case -2:
                        ModelState.AddModelError("Dni", "Ya existe una persona con este DNI.");
                        break;
                    case -3:
                        ModelState.AddModelError("Email", "Ya existe una persona con este email.");
                        break;
                    default:
                        ModelState.AddModelError("", "Error al crear la persona.");
                        break;
                }
                return View(personaModel);
            }

            // 2. CREAR EL PROPIETARIO CON EL ID DE LA PERSONA CREADA
            var propietarioModel = new PropietarioModel { PersonaId = personaId };
            int propietarioId = _propietarioRepo.Alta(propietarioModel);

            TempData["Success"] = $"Propietario creado exitosamente. ID: {propietarioId}";
            return RedirectToAction(nameof(Details), new { id = propietarioId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el propietario: {ex.Message}";
            return View(personaModel);
        }
    }

    #endregion

    #region ALTA (Crear propietario desde persona existente)

    /// <summary>
    /// Crea un propietario directamente desde una persona (para uso desde otras vistas)
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección con resultado</returns>
    [HttpPost]
    public IActionResult CreateFromPersona(int personaId)
    {
        try
        {
            // Verificar que la persona existe
            var persona = _personaRepo.ObtenerPorId(personaId);
            if (persona == null)
            {
                TempData["Error"] = "La persona especificada no existe.";
                return RedirectToAction(nameof(Index));
            }

            // Crear modelo y dar de alta
            var model = new PropietarioModel { PersonaId = personaId };
            int propietarioId = _propietarioRepo.Alta(model);

            TempData["Success"] = $"Propietario creado exitosamente para {persona.Apellido}, {persona.Nombre}";
            return RedirectToAction(nameof(Details), new { id = propietarioId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el propietario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region BAJA (Eliminación lógica)

    /// <summary>
    /// Muestra confirmación para dar de baja un propietario
    /// </summary>
    /// <param name="id">ID del propietario</param>
    /// <returns>Vista de confirmación</returns>
    [HttpGet]
    public IActionResult Delete(int id)
    {
        try
        {
            var propietario = _propietarioRepo.ObtenerPorId(id);
            if (propietario == null)
            {
                TempData["Error"] = "Propietario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos de la persona para mostrar en la confirmación
            var persona = _personaRepo.ObtenerPorId(propietario.PersonaId);
            
            var modelo = new
            {
                PropietarioId = propietario.PropietarioId,
                PersonaId = propietario.PersonaId,
                NombreCompleto = persona != null ? $"{persona.Apellido}, {persona.Nombre}" : "Datos no disponibles",
                Dni = persona?.Dni ?? "No disponible"
            };

            return View(modelo);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la confirmación de baja: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa la baja lógica del propietario
    /// </summary>
    /// <param name="id">ID del propietario</param>
    /// <returns>Redirección con resultado</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var propietario = _propietarioRepo.ObtenerPorId(id);
            if (propietario == null)
            {
                TempData["Error"] = "Propietario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Realizar baja lógica
            int resultado = _propietarioRepo.Baja(id);
            
            if (resultado > 0)
            {
                TempData["Success"] = "Propietario dado de baja exitosamente.";
            }
            else
            {
                TempData["Warning"] = "No se pudo dar de baja el propietario.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al dar de baja el propietario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region BAJA (Desde listado via AJAX)

    /// <summary>
    /// Baja rápida desde el listado usando PersonaId
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>JSON con resultado</returns>
    [HttpPost]
    public IActionResult DeleteByPersonaId(int personaId)
    {
        try
        {
            // Obtener propietario por PersonaId
            var (propietario, estado) = _propietarioRepo.ObtenerPorIdPersona(personaId);
            if (propietario == null)
            {
                return Json(new { success = false, message = "Propietario no encontrado." });
            }

            // Realizar baja lógica
            int resultado = _propietarioRepo.Baja(propietario.PropietarioId);
            
            if (resultado > 0)
            {
                return Json(new { success = true, message = "Propietario dado de baja exitosamente." });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo dar de baja el propietario." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error al dar de baja: {ex.Message}" });
        }
    }

    #endregion

    #region MODIFICACIÓN (No implementada)

    /// <summary>
    /// Muestra información sobre por qué no se permite modificar propietarios
    /// </summary>
    /// <param name="id">ID del propietario</param>
    /// <returns>Vista informativa</returns>
    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewBag.PropietarioId = id;
        ViewBag.Message = "La modificación de propietarios no está implementada por diseño. " +
                         "Los propietarios solo mantienen la relación PropietarioId-PersonaId, " +
                         "que no debe modificarse para preservar la integridad referencial. " +
                         "Para cambiar datos personales, modifique directamente la Persona asociada.";
        
        return View();
    }

    #endregion

    #region MÉTODOS AUXILIARES

    /// <summary>
    /// Busca propietarios por término de búsqueda (para autocomplete)
    /// </summary>
    /// <param name="term">Término de búsqueda</param>
    /// <returns>JSON con resultados</returns>
    [HttpGet]
    public IActionResult Search(string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return Json(new List<object>());
            }

            var (personas, total) = _propietarioRepo.ObtenerTodosConPaginacion(1, 10, term);
            
            var resultados = personas.Select(p => new
            {
                PersonaId = p.PersonaId,
                Dni = p.Dni,
                NombreCompleto = $"{p.Apellido}, {p.Nombre}",
                Email = p.Email
            });

            return Json(resultados);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verifica si una persona puede ser convertida en propietario
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>JSON con resultado de validación</returns>
    [HttpGet]
    public IActionResult ValidatePersona(int personaId)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(personaId);
            if (persona == null)
            {
                return Json(new { valid = false, message = "Persona no encontrada." });
            }

            // Verificar roles actuales usando tuplas
            var (propietario, estadoPropietario) = _propietarioRepo.ObtenerPorIdPersona(personaId);

            var yaEsPropietario = propietario != null && estadoPropietario == 1;
            
            return Json(new { 
                valid = !yaEsPropietario, 
                message = yaEsPropietario ? "Esta persona ya es propietario." : "Persona válida para ser propietario.",
                persona = new {
                    NombreCompleto = $"{persona.Apellido}, {persona.Nombre}",
                    Dni = persona.Dni,
                    Email = persona.Email
                }
            });
        }
        catch (Exception ex)
        {
            return Json(new { valid = false, message = ex.Message });
        }
    }

    #endregion
}