using Microsoft.AspNetCore.Mvc;
using UniDotNet.Models;
using UniDotNet.Repository;

namespace UniDotNet.Controllers;

/// <summary>
/// Controlador para la gestión de personas.
/// Implementa BMC (Baja, Modificación, Consulta) y gestión de roles.
/// </summary>
public class PersonaController : Controller
{
    private readonly IPersonaRepositorio _personaRepo;
    private readonly IPropietarioRepositorio _propietarioRepo;
    private readonly IEmpleadoRepositorio _empleadoRepo;
    private readonly IInquilinoRepositorio _inquilinoRepo;

    public PersonaController(IPersonaRepositorio personaRepo, 
                           IPropietarioRepositorio propietarioRepo, 
                           IEmpleadoRepositorio empleadoRepo, 
                           IInquilinoRepositorio inquilinoRepo)
    {
        _personaRepo = personaRepo;
        _propietarioRepo = propietarioRepo;
        _empleadoRepo = empleadoRepo;
        _inquilinoRepo = inquilinoRepo;
    }

    #region CONSULTA (Listado con paginación y roles)

    /// <summary>
    /// Acción principal: Lista personas con paginación, búsqueda y roles
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Elementos por página</param>
    /// <param name="search">Término de búsqueda</param>
    /// <returns>Vista con lista paginada de personas con roles</returns>
    public IActionResult Index(int page = 1, int pageSize = 10, string search = null)
    {
        try
        {
            var (personasConRoles, total) = _personaRepo.ObtenerTodosConPaginacion(page, pageSize, search);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRegistros = total;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Search = search;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPaginas;
            
            return View(personasConRoles);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la lista de personas: {ex.Message}";
            return View(new List<dynamic>());
        }
    }

    #endregion

    #region CONSULTA (Detalle individual con roles)

    /// <summary>
    /// Muestra los detalles completos de una persona con todos sus roles
    /// </summary>
    /// <param name="id">ID de la persona</param>
    /// <returns>Vista con detalles de la persona y roles</returns>
    public IActionResult Details(int id)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(id);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar roles actuales usando tuplas
            var (propietario, estadoPropietario) = _propietarioRepo.ObtenerPorIdPersona(id);
            var (empleado, estadoEmpleado) = _empleadoRepo.ObtenerPorIdPersona(id);
            var (inquilino, estadoInquilino) = _inquilinoRepo.ObtenerPorIdPersona(id);

            var esPropietario = propietario != null && estadoPropietario == 1;
            var esEmpleado = empleado != null && estadoEmpleado == 1;
            var esInquilino = inquilino != null && estadoInquilino == 1;

            // Crear modelo completo para la vista
            var modelo = new
            {
                PersonaId = persona.PersonaId,
                Dni = persona.Dni,
                Apellido = persona.Apellido,
                Nombre = persona.Nombre,
                Telefono = persona.Telefono,
                Email = persona.Email,
                NombreCompleto = $"{persona.Apellido}, {persona.Nombre}",
                EsPropietario = esPropietario,
                EsEmpleado = esEmpleado,
                EsInquilino = esInquilino,
                Roles = GetRolesString(esPropietario, esInquilino, esEmpleado)
            };

            return View(modelo);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar los detalles de la persona: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region ALTA (No implementada - Solo información)

    /// <summary>
    /// Muestra información sobre por qué no se permite alta directa de personas
    /// </summary>
    /// <returns>Vista informativa</returns>
    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Message = "El alta de personas no está implementada en este módulo por diseño. " +
                         "Las personas se dan de alta automáticamente cuando se crean propietarios, " +
                         "inquilinos o empleados desde sus respectivos módulos. " +
                         "Esta arquitectura garantiza que cada persona tenga al menos un rol asignado " +
                         "y mantiene la consistencia de datos.";
        
        return View();
    }

    #endregion

    #region MODIFICACIÓN

    /// <summary>
    /// Muestra el formulario para modificar una persona
    /// </summary>
    /// <param name="id">ID de la persona</param>
    /// <returns>Vista con formulario de modificación</returns>
    [HttpGet]
    public IActionResult Edit(int id)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(id);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            return View(persona);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el formulario de edición: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Procesa la modificación de una persona
    /// </summary>
    /// <param name="model">Modelo con datos actualizados</param>
    /// <returns>Redirección según resultado</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(PersonaModel model)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verificar que la persona existe
            var personaExistente = _personaRepo.ObtenerPorId(model.PersonaId);
            if (personaExistente == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar duplicados solo si cambió el DNI o email
            if (personaExistente.Dni != model.Dni)
            {
                if (_personaRepo.ExistePorDni(model.Dni))
                {
                    ModelState.AddModelError("Dni", "Ya existe una persona con este DNI.");
                    return View(model);
                }
            }

            if (personaExistente.Email != model.Email)
            {
                if (_personaRepo.ExistePorEmail(model.Email))
                {
                    ModelState.AddModelError("Email", "Ya existe una persona con este email.");
                    return View(model);
                }
            }

            // Modificar la persona
            int resultado = _personaRepo.Modificacion(model);

            if (resultado > 0)
            {
                TempData["Success"] = "Persona modificada exitosamente.";
                return RedirectToAction(nameof(Details), new { id = model.PersonaId });
            }
            else
            {
                TempData["Warning"] = "No se realizaron cambios en la persona.";
                return View(model);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al modificar la persona: {ex.Message}";
            return View(model);
        }
    }

    #endregion

    #region BAJA (Con baja en cascada de roles)

    /// <summary>
    /// Muestra confirmación para dar de baja una persona y todos sus roles
    /// </summary>
    /// <param name="id">ID de la persona</param>
    /// <returns>Vista de confirmación</returns>
    [HttpGet]
    public IActionResult Delete(int id)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(id);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Verificar roles para mostrar en la confirmación usando tuplas
            var (propietario, estadoPropietario) = _propietarioRepo.ObtenerPorIdPersona(id);
            var (empleado, estadoEmpleado) = _empleadoRepo.ObtenerPorIdPersona(id);
            var (inquilino, estadoInquilino) = _inquilinoRepo.ObtenerPorIdPersona(id);

            var esPropietario = propietario != null && estadoPropietario == 1;
            var esEmpleado = empleado != null && estadoEmpleado == 1;
            var esInquilino = inquilino != null && estadoInquilino == 1;

            var modelo = new
            {
                PersonaId = persona.PersonaId,
                NombreCompleto = $"{persona.Apellido}, {persona.Nombre}",
                Dni = persona.Dni,
                Email = persona.Email,
                EsPropietario = esPropietario,
                EsEmpleado = esEmpleado,
                EsInquilino = esInquilino,
                Roles = GetRolesString(esPropietario, esInquilino, esEmpleado)
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
    /// Procesa la baja lógica de la persona y todos sus roles
    /// </summary>
    /// <param name="id">ID de la persona</param>
    /// <returns>Redirección con resultado</returns>
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(id);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            // Dar de baja solo la persona (conservar roles activos)
            int resultado = _personaRepo.Baja(id);
            
            if (resultado > 0)
            {
                TempData["Success"] = $"Persona dada de baja exitosamente. Los roles se mantienen activos.";
            }
            else
            {
                TempData["Warning"] = "No se pudo dar de baja la persona.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al dar de baja la persona: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region GESTIÓN DE ROLES (Alta/Baja individual)

    /// <summary>
    /// Convierte una persona en propietario
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección al Index</returns>
    [HttpPost]
    public IActionResult AltaPropietario(int personaId)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(personaId);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction("Index");
            }

            var model = new PropietarioModel { PersonaId = personaId };
            int propietarioId = _propietarioRepo.Alta(model);

            TempData["Success"] = $"{persona.Apellido}, {persona.Nombre} es ahora propietario.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Convierte una persona en empleado
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección al Index</returns>
    [HttpPost]
    public IActionResult AltaEmpleado(int personaId)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(personaId);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction("Index");
            }

            var model = new EmpleadoModel { PersonaId = personaId };
            int empleadoId = _empleadoRepo.Alta(model);

            TempData["Success"] = $"{persona.Apellido}, {persona.Nombre} es ahora empleado.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Convierte una persona en inquilino
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección al Index</returns>
    [HttpPost]
    public IActionResult AltaInquilino(int personaId)
    {
        try
        {
            var persona = _personaRepo.ObtenerPorId(personaId);
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction("Index");
            }

            var model = new InquilinoModel { PersonaId = personaId };
            int inquilinoId = _inquilinoRepo.Alta(model);

            TempData["Success"] = $"{persona.Apellido}, {persona.Nombre} es ahora inquilino.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Da de baja el rol de propietario de una persona
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección al Index</returns>
    [HttpPost]
    public IActionResult BajaPropietario(int personaId)
    {
        try
        {
            var (propietario, estado) = _propietarioRepo.ObtenerPorIdPersona(personaId);
            if (propietario == null || estado != 1)
            {
                TempData["Warning"] = "La persona no es propietario activo.";
                return RedirectToAction("Index");
            }

            int resultado = _propietarioRepo.Baja(propietario.PropietarioId);

            if (resultado > 0)
            {
                var persona = _personaRepo.ObtenerPorId(personaId);
                TempData["Success"] = persona != null ? $"{persona.Apellido}, {persona.Nombre} ya no es propietario." : "Propietario dado de baja exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo dar de baja el propietario.";
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Da de baja el rol de empleado de una persona
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección al Index</returns>
    [HttpPost]
    public IActionResult BajaEmpleado(int personaId)
    {
        try
        {
            var (empleado, estado) = _empleadoRepo.ObtenerPorIdPersona(personaId);
            if (empleado == null || estado != 1)
            {
                TempData["Warning"] = "La persona no es empleado activo.";
                return RedirectToAction("Index");
            }

            int resultado = _empleadoRepo.Baja(empleado.EmpleadoId);

            if (resultado > 0)
            {
                var persona = _personaRepo.ObtenerPorId(personaId);
                TempData["Success"] = persona != null ? $"{persona.Apellido}, {persona.Nombre} ya no es empleado." : "Empleado dado de baja exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo dar de baja el empleado.";
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Da de baja el rol de inquilino de una persona
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Redirección al Index</returns>
    [HttpPost]
    public IActionResult BajaInquilino(int personaId)
    {
        try
        {
            var (inquilino, estado) = _inquilinoRepo.ObtenerPorIdPersona(personaId);
            if (inquilino == null || estado != 1)
            {
                TempData["Warning"] = "La persona no es inquilino activo.";
                return RedirectToAction("Index");
            }

            int resultado = _inquilinoRepo.Baja(inquilino.InquilinoId);

            if (resultado > 0)
            {
                var persona = _personaRepo.ObtenerPorId(personaId);
                TempData["Success"] = persona != null ? $"{persona.Apellido}, {persona.Nombre} ya no es inquilino." : "Inquilino dado de baja exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo dar de baja el inquilino.";
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    #endregion

    #region RECUPERACIÓN DE PERSONAS

    /// <summary>
    /// Lista personas dadas de baja que pueden ser recuperadas
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Elementos por página</param>
    /// <param name="search">Término de búsqueda</param>
    /// <returns>Vista con lista paginada de personas dadas de baja</returns>
    public IActionResult Recuperar(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            // Usar el método modificado con estado = false para obtener personas dadas de baja
            var (personasConRoles, total) = _personaRepo.ObtenerTodosConPaginacion(page, pageSize, search, estado: false);
            
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRegistros = total;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)total / pageSize);
            ViewBag.Search = search;
            ViewBag.HasPrevious = page > 1;
            ViewBag.HasNext = page < ViewBag.TotalPaginas;

            return View(personasConRoles);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar personas dadas de baja: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Recupera una persona dada de baja (la reactiva usando Modificacion)
    /// </summary>
    /// <param name="personaId">ID de la persona a recuperar</param>
    /// <returns>Redirección a la vista Recuperar</returns>
    [HttpPost]
    public IActionResult RecuperarPersona(int personaId)
    {
        try
        {
            // Obtener la persona sin filtrar por estado
            var persona = _personaRepo.ObtenerPorIdSinFiltro(personaId);
            
            if (persona == null)
            {
                TempData["Error"] = "Persona no encontrada.";
                return RedirectToAction("Recuperar");
            }

            // Usar el método Modificacion que automáticamente establece estado = 1
            int resultado = _personaRepo.Modificacion(persona);

            if (resultado > 0)
            {
                TempData["Success"] = $"{persona.Apellido}, {persona.Nombre} ha sido recuperado exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo recuperar la persona.";
            }

            return RedirectToAction("Recuperar");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al recuperar persona: {ex.Message}";
            return RedirectToAction("Recuperar");
        }
    }

    #endregion

    #region MÉTODOS AUXILIARES

    /// <summary>
    /// Busca personas por término de búsqueda (para autocomplete)
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

            var personas = _personaRepo.BuscarPorNombre(term);
            
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
    /// Obtiene los roles actuales de una persona
    /// </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>JSON con roles actuales</returns>
    [HttpGet]
    public IActionResult GetRoles(int personaId)
    {
        try
        {
            var (propietario, estadoPropietario) = _propietarioRepo.ObtenerPorIdPersona(personaId);
            var (empleado, estadoEmpleado) = _empleadoRepo.ObtenerPorIdPersona(personaId);
            var (inquilino, estadoInquilino) = _inquilinoRepo.ObtenerPorIdPersona(personaId);

            var esPropietario = propietario != null && estadoPropietario == 1;
            var esEmpleado = empleado != null && estadoEmpleado == 1;
            var esInquilino = inquilino != null && estadoInquilino == 1;

            return Json(new
            {
                personaId = personaId,
                esPropietario = esPropietario,
                esEmpleado = esEmpleado,
                esInquilino = esInquilino,
                roles = GetRolesString(esPropietario, esInquilino, esEmpleado)
            });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Genera string con los roles de una persona
    /// </summary>
    /// <param name="esPropietario">Es propietario</param>
    /// <param name="esInquilino">Es inquilino</param>
    /// <param name="esEmpleado">Es empleado</param>
    /// <returns>String con roles separados por comas</returns>
    private string GetRolesString(bool esPropietario, bool esInquilino, bool esEmpleado)
    {
        var roles = new List<string>();
        
        if (esPropietario) roles.Add("Propietario");
        if (esInquilino) roles.Add("Inquilino");
        if (esEmpleado) roles.Add("Empleado");
        
        return roles.Any() ? string.Join(", ", roles) : "Sin rol asignado";
    }

    #endregion
}