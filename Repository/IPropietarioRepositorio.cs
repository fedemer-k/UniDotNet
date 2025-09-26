using UniDotNet.Models;

namespace UniDotNet.Repository;

/// <summary> Interfaz específica para operaciones CRUD de Propietario, extendiendo IRepositorio genérico. </summary>
public interface IPropietarioRepositorio : IRepositorio<PropietarioModel>
{
    /// <summary> Obtiene todos los propietarios con paginación y búsqueda, incluyendo datos de persona </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="search">Término de búsqueda opcional</param>
    /// <returns>Tupla con lista de PersonaModel con datos de persona y total de registros</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    (List<PersonaModel> Personas, int Total) ObtenerTodosConPaginacion(int page, int pageSize, string search = null);

    /// <summary> Obtiene un propietario por ID de persona (incluye inactivos) </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>Tupla con el propietario (Posible null) y su estado</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    (PropietarioModel propietario, int estado) ObtenerPorIdPersona(int personaId);
}