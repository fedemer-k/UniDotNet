using UniDotNet.Models;

namespace UniDotNet.Repository;

/// <summary> Interfaz específica para operaciones CRUD de Inquilino, extendiendo IRepositorio genérico. </summary>
public interface IInquilinoRepositorio : IRepositorio<InquilinoModel>
{
    /// <summary> Obtiene todos los inquilinos con paginación y búsqueda, incluyendo datos de persona </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="search">Término de búsqueda opcional</param>
    /// <returns>Tupla con lista de PersonaModel con datos de persona y total de registros</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    (List<PersonaModel> Personas, int Total) ObtenerTodosConPaginacion(int page, int pageSize, string search = null);

    /// <summary> Obtiene un inquilino por ID de persona (incluye inactivos) </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Tupla con el inquilino (Posible null) y su estado</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    (InquilinoModel inquilino, int estado) ObtenerPorIdPersona(int personaId);
}