using Microsoft.AspNetCore.Mvc;
using UniDotNet.Models;
using UniDotNet.Repository;

namespace UniDotNet.Controllers;

/// <summary>
/// Controlador para la gestión de inquilinos.
/// Implementa ABMC (Alta, Baja, Modificación, Consulta) para inquilinos.
/// </summary>
public class InquilinoController : Controller
{
    private readonly IInquilinoRepositorio _inquilinoRepo;
    private readonly IPersonaRepositorio _personaRepo;

    public InquilinoController(IInquilinoRepositorio inquilinoRepo, IPersonaRepositorio personaRepo)
    {
        _inquilinoRepo = inquilinoRepo;
        _personaRepo = personaRepo;
    }

    #region CONSULTA (Listado con paginación)

    /// <summary>
    /// Acción principal: Lista inquilinos con paginación y búsqueda
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Elementos por página</param>
    /// <param name="search">Término de búsqueda</param>
    /// <returns>Vista con lista paginada de inquilinos</returns>
    public IActionResult Index(int page = 1, int pageSize = 10, string search = null)
    {
        try
        {
            var (personas, total) = _inquilinoRepo.ObtenerTodosConPaginacion(page, pageSize, search);
            
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
            TempData["Error"] = $"Error al cargar la lista de inquilinos: {ex.Message}";
            return View(new List<PersonaModel>());
        }
    }

    #endregion

    #region CONSULTA (Detalle individual)

    /// <summary>
    /// Muestra los detalles completos de un inquilino específico
    /// </summary>
    /// <param name="id">ID del inquilino</param>
    /// <returns>Vista con detalles del inquilino</returns>
    public IActionResult Details(int id)
    {
        try
        {
            var inquilino = _inquilinoRepo.ObtenerPorId(id);
            if (inquilino == null)
            {
                TempData["Error"] = "Inquilino no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos completos de la persona
            var persona = _personaRepo.ObtenerPorId(inquilino.PersonaId);
            if (persona == null)
            {
                TempData["Error"] = "No se encontraron los datos de la persona asociada.";
                return RedirectToAction(nameof(Index));
            }

            // Crear modelo combinado para la vista
            var modelo = new
            {
                InquilinoId = inquilino.InquilinoId,
                PersonaId = inquilino.PersonaId,
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
            TempData["Error"] = $"Error al cargar los detalles del inquilino: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Muestra los detalles de un inquilino usando PersonaId
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Vista con detalles del inquilino</returns>
    public IActionResult DetailsByPersonaId(int personaId)
    {
        try
        {
            // Obtener inquilino por PersonaId
            var (inquilino, estado) = _inquilinoRepo.ObtenerPorIdPersona(personaId);
            if (inquilino == null)
            {
                TempData["Error"] = "Inquilino no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos completos de la persona
            var persona = _personaRepo.ObtenerPorId(inquilino.PersonaId);
            if (persona == null)
            {
                TempData["Error"] = "No se encontraron los datos de la persona asociada.";
                return RedirectToAction(nameof(Index));
            }

            // Crear modelo combinado para la vista
            var modelo = new
            {
                InquilinoId = inquilino.InquilinoId,
                PersonaId = inquilino.PersonaId,
                Dni = persona.Dni,
                Apellido = persona.Apellido,
                Nombre = persona.Nombre,
                Telefono = persona.Telefono,
                Email = persona.Email,
                NombreCompleto = $"{persona.Apellido}, {persona.Nombre}",
                Estado = estado
            };

            return View("Details", modelo);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar los detalles del inquilino: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region ALTA (Crear nuevo inquilino)

    /// <summary>
    /// Muestra el formulario para crear un nuevo inquilino
    /// </summary>
    /// <returns>Vista con formulario de alta</returns>
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            return View(new InquilinoModel());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa el alta de un nuevo inquilino
    /// </summary>
    /// <param name="model">Modelo con datos del inquilino</param>
    /// <returns>Redirección según resultado</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(InquilinoModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la persona existe
            var persona = _personaRepo.ObtenerPorId(model.PersonaId);
            if (persona == null)
            {
                ModelState.AddModelError("PersonaId", "La persona seleccionada no existe.");
                return View(model);
            }

            // Verificar si ya existe un inquilino activo para esta persona
            var (inquilinoExistente, estadoInq) = _inquilinoRepo.ObtenerPorIdPersona(model.PersonaId);
            if (inquilinoExistente != null && estadoInq == 1)
            {
                ModelState.AddModelError("PersonaId", "Ya existe un inquilino activo para esta persona.");
                return View(model);
            }

            // Dar de alta (puede crear nuevo o reactivar existente)
            int inquilinoId = _inquilinoRepo.Alta(model);

            TempData["Success"] = $"Inquilino creado exitosamente. ID: {inquilinoId}";
            return RedirectToAction(nameof(Details), new { id = inquilinoId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el inquilino: {ex.Message}";
            return View(model);
        }
    }

    #endregion

    #region ALTA (Crear inquilino desde persona existente)

    /// <summary>
    /// Crea un inquilino directamente desde una persona (para uso desde otras vistas)
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
            var model = new InquilinoModel { PersonaId = personaId };
            int inquilinoId = _inquilinoRepo.Alta(model);

            TempData["Success"] = $"Inquilino creado exitosamente para {persona.Apellido}, {persona.Nombre}";
            return RedirectToAction(nameof(Details), new { id = inquilinoId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el inquilino: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region BAJA (Eliminación lógica)

    /// <summary>
    /// Muestra confirmación para dar de baja un inquilino
    /// </summary>
    /// <param name="id">ID del inquilino</param>
    /// <returns>Vista de confirmación</returns>
    [HttpGet]
    public IActionResult Delete(int id)
    {
        try
        {
            var inquilino = _inquilinoRepo.ObtenerPorId(id);
            if (inquilino == null)
            {
                TempData["Error"] = "Inquilino no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos de la persona para mostrar en la confirmación
            var persona = _personaRepo.ObtenerPorId(inquilino.PersonaId);
            
            var modelo = new
            {
                InquilinoId = inquilino.InquilinoId,
                PersonaId = inquilino.PersonaId,
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
    /// Procesa la baja lógica del inquilino
    /// </summary>
    /// <param name="id">ID del inquilino</param>
    /// <returns>Redirección con resultado</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var inquilino = _inquilinoRepo.ObtenerPorId(id);
            if (inquilino == null)
            {
                TempData["Error"] = "Inquilino no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Realizar baja lógica
            int resultado = _inquilinoRepo.Baja(id);
            
            if (resultado > 0)
            {
                TempData["Success"] = "Inquilino dado de baja exitosamente.";
            }
            else
            {
                TempData["Warning"] = "No se pudo dar de baja el inquilino.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al dar de baja el inquilino: {ex.Message}";
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
            // Obtener inquilino por PersonaId
            var (inquilino, estado) = _inquilinoRepo.ObtenerPorIdPersona(personaId);
            if (inquilino == null)
            {
                return Json(new { success = false, message = "Inquilino no encontrado." });
            }

            // Realizar baja lógica
            int resultado = _inquilinoRepo.Baja(inquilino.InquilinoId);
            
            if (resultado > 0)
            {
                return Json(new { success = true, message = "Inquilino dado de baja exitosamente." });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo dar de baja el inquilino." });
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
    /// Muestra información sobre por qué no se permite modificar inquilinos
    /// </summary>
    /// <param name="id">ID del inquilino</param>
    /// <returns>Vista informativa</returns>
    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewBag.InquilinoId = id;
        ViewBag.Message = "La modificación de inquilinos no está implementada por diseño. " +
                         "Los inquilinos solo mantienen la relación InquilinoId-PersonaId, " +
                         "que no debe modificarse para preservar la integridad referencial. " +
                         "Para cambiar datos personales, modifique directamente la Persona asociada.";
        
        return View();
    }

    #endregion

    #region MÉTODOS AUXILIARES

    /// <summary>
    /// Busca inquilinos por término de búsqueda (para autocomplete)
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

            var (personas, total) = _inquilinoRepo.ObtenerTodosConPaginacion(1, 10, term);
            
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
    /// Verifica si una persona puede ser convertida en inquilino
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
            var (inquilino, estadoInquilino) = _inquilinoRepo.ObtenerPorIdPersona(personaId);

            var yaEsInquilino = inquilino != null && estadoInquilino == 1;
            
            return Json(new { 
                valid = !yaEsInquilino, 
                message = yaEsInquilino ? "Esta persona ya es inquilino." : "Persona válida para ser inquilino.",
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
