using Microsoft.AspNetCore.Mvc;
using UniDotNet.Models;
using UniDotNet.Repository;

namespace UniDotNet.Controllers;

/// <summary>
/// Controlador para la gestión de empleados.
/// Implementa ABMC (Alta, Baja, Modificación, Consulta) para empleados.
/// </summary>
public class EmpleadoController : Controller
{
    private readonly IEmpleadoRepositorio _empleadoRepo;
    private readonly IPersonaRepositorio _personaRepo;

    public EmpleadoController(IEmpleadoRepositorio empleadoRepo, IPersonaRepositorio personaRepo)
    {
        _empleadoRepo = empleadoRepo;
        _personaRepo = personaRepo;
    }

    #region CONSULTA (Listado con paginación)

    /// <summary>
    /// Acción principal: Lista empleados con paginación y búsqueda
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Elementos por página</param>
    /// <param name="search">Término de búsqueda</param>
    /// <returns>Vista con lista paginada de empleados</returns>
    public IActionResult Index(int page = 1, int pageSize = 10, string search = null)
    {
        try
        {
            var (personas, total) = _empleadoRepo.ObtenerTodosConPaginacion(page, pageSize, search);
            
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
            TempData["Error"] = $"Error al cargar la lista de empleados: {ex.Message}";
            return View(new List<PersonaModel>());
        }
    }

    #endregion

    #region CONSULTA (Detalle individual)

    /// <summary>
    /// Muestra los detalles completos de un empleado específico
    /// </summary>
    /// <param name="id">ID del empleado</param>
    /// <returns>Vista con detalles del empleado</returns>
    public IActionResult Details(int id)
    {
        try
        {
            var empleado = _empleadoRepo.ObtenerPorId(id);
            if (empleado == null)
            {
                TempData["Error"] = "Empleado no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos completos de la persona
            var persona = _personaRepo.ObtenerPorId(empleado.PersonaId);
            if (persona == null)
            {
                TempData["Error"] = "No se encontraron los datos de la persona asociada.";
                return RedirectToAction(nameof(Index));
            }

            // Crear modelo combinado para la vista
            var modelo = new
            {
                EmpleadoId = empleado.EmpleadoId,
                PersonaId = empleado.PersonaId,
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
            TempData["Error"] = $"Error al cargar los detalles del empleado: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region ALTA (Crear nuevo empleado)

    /// <summary>
    /// Muestra el formulario para crear un nuevo empleado
    /// </summary>
    /// <returns>Vista con formulario de alta</returns>
    [HttpGet]
    public IActionResult Create()
    {
        try
        {
            return View(new EmpleadoModel());
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el formulario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa el alta de un nuevo empleado
    /// </summary>
    /// <param name="model">Modelo con datos del empleado</param>
    /// <returns>Redirección según resultado</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(EmpleadoModel model)
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

            // Verificar si ya existe un empleado activo para esta persona
            var (empleadoExistente, estadoEmp) = _empleadoRepo.ObtenerPorIdPersona(model.PersonaId);
            if (empleadoExistente != null && estadoEmp == 1)
            {
                ModelState.AddModelError("PersonaId", "Ya existe un empleado activo para esta persona.");
                return View(model);
            }

            // Dar de alta (puede crear nuevo o reactivar existente)
            int empleadoId = _empleadoRepo.Alta(model);

            TempData["Success"] = $"Empleado creado exitosamente. ID: {empleadoId}";
            return RedirectToAction(nameof(Details), new { id = empleadoId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el empleado: {ex.Message}";
            return View(model);
        }
    }

    #endregion

    #region ALTA (Crear empleado desde persona existente)

    /// <summary>
    /// Crea un empleado directamente desde una persona (para uso desde otras vistas)
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
            var model = new EmpleadoModel { PersonaId = personaId };
            int empleadoId = _empleadoRepo.Alta(model);

            TempData["Success"] = $"Empleado creado exitosamente para {persona.Apellido}, {persona.Nombre}";
            return RedirectToAction(nameof(Details), new { id = empleadoId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el empleado: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region BAJA (Eliminación lógica)

    /// <summary>
    /// Muestra confirmación para dar de baja un empleado
    /// </summary>
    /// <param name="id">ID del empleado</param>
    /// <returns>Vista de confirmación</returns>
    [HttpGet]
    public IActionResult Delete(int id)
    {
        try
        {
            var empleado = _empleadoRepo.ObtenerPorId(id);
            if (empleado == null)
            {
                TempData["Error"] = "Empleado no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Obtener datos de la persona para mostrar en la confirmación
            var persona = _personaRepo.ObtenerPorId(empleado.PersonaId);
            
            var modelo = new
            {
                EmpleadoId = empleado.EmpleadoId,
                PersonaId = empleado.PersonaId,
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
    /// Procesa la baja lógica del empleado
    /// </summary>
    /// <param name="id">ID del empleado</param>
    /// <returns>Redirección con resultado</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var empleado = _empleadoRepo.ObtenerPorId(id);
            if (empleado == null)
            {
                TempData["Error"] = "Empleado no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // Realizar baja lógica
            int resultado = _empleadoRepo.Baja(id);
            
            if (resultado > 0)
            {
                TempData["Success"] = "Empleado dado de baja exitosamente.";
            }
            else
            {
                TempData["Warning"] = "No se pudo dar de baja el empleado.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al dar de baja el empleado: {ex.Message}";
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
            // Obtener empleado por PersonaId
            var (empleado, estado) = _empleadoRepo.ObtenerPorIdPersona(personaId);
            if (empleado == null)
            {
                return Json(new { success = false, message = "Empleado no encontrado." });
            }

            // Realizar baja lógica
            int resultado = _empleadoRepo.Baja(empleado.EmpleadoId);
            
            if (resultado > 0)
            {
                return Json(new { success = true, message = "Empleado dado de baja exitosamente." });
            }
            else
            {
                return Json(new { success = false, message = "No se pudo dar de baja el empleado." });
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
    /// Muestra información sobre por qué no se permite modificar empleados
    /// </summary>
    /// <param name="id">ID del empleado</param>
    /// <returns>Vista informativa</returns>
    [HttpGet]
    public IActionResult Edit(int id)
    {
        ViewBag.EmpleadoId = id;
        ViewBag.Message = "La modificación de empleados no está implementada por diseño. " +
                         "Los empleados solo mantienen la relación EmpleadoId-PersonaId, " +
                         "que no debe modificarse para preservar la integridad referencial. " +
                         "Para cambiar datos personales, modifique directamente la Persona asociada.";
        
        return View();
    }

    #endregion

    #region MÉTODOS AUXILIARES

    /// <summary>
    /// Busca empleados por término de búsqueda (para autocomplete)
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

            var (personas, total) = _empleadoRepo.ObtenerTodosConPaginacion(1, 10, term);
            
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
    /// Verifica si una persona puede ser convertida en empleado
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
            var (empleado, estadoEmpleado) = _empleadoRepo.ObtenerPorIdPersona(personaId);

            var yaEsEmpleado = empleado != null && estadoEmpleado == 1;
            
            return Json(new { 
                valid = !yaEsEmpleado, 
                message = yaEsEmpleado ? "Esta persona ya es empleado." : "Persona válida para ser empleado.",
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
