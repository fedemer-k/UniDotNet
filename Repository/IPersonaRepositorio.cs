using UniDotNet.Models;

namespace UniDotNet.Repository;

/// <summary>
/// Interfaz específica para operaciones CRUD sobre entidades PersonaModel.
/// - Hereda de IRepositorio<PersonaModel>, especializándola para el tipo PersonaModel.
/// - Define métodos adicionales específicos para la entidad PersonaModel.
/// - Permite una implementación concreta en PersonaRepositorio.
/// </summary>
public interface IPersonaRepositorio : IRepositorio<PersonaModel>
{
    // Métodos específicos adicionales
    public (List<dynamic> PersonasConRoles, int Total) ObtenerTodosConPaginacion(int page, int pageSize, string? search = null, bool estado = true);
    public PersonaModel? ObtenerPorIdSinFiltro(int id);
    public PersonaModel? ObtenerPorDni(string dni);
    public IList<PersonaModel> BuscarPorNombre(string nombre);
    public bool ExistePorDni(string dni);
    public bool ExistePorEmail(string email);
}
