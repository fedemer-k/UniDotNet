using UniDotNet.Models;                      //Para usar PropietarioModel
using Microsoft.Extensions.Configuration;    //Para usar IConfiguration
using MySql.Data.MySqlClient;                //Para usar MySqlConnection, MySqlCommand, etc.

namespace UniDotNet.Repository;

/// <summary> Repositorio concreto para manejar datos de propietarios. </summary>
public class PropietarioRepositorio : BaseRepositorio, IPropietarioRepositorio
{
    public PropietarioRepositorio(IConfiguration configuration) : base(configuration){}

    /// <summary> Da de alta un propietario. Si PropietarioId es 0, crea uno nuevo. </summary>
    /// <param name="p">Modelo del propietario</param>
    /// <returns>ID del propietario creado</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public int Alta(PropietarioModel p)
    {
        //Primero debo hacer uso de ObtenerPorIdPersona
        //para verificar que no exista ya un propietario para esa persona y se encuentre desactivado
        //Si existe y esta desactivado, lo reactivo (Baja logica inversa)
        var (existente, estado) = ObtenerPorIdPersona(p.PersonaId);

        if (existente != null)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand command = new MySqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            UPDATE propietarios 
                            SET estado = @Estado 
                            WHERE id_propietario = @PropietarioId
                        ";

                        command.Parameters.AddWithValue("@Estado", 1);
                        command.Parameters.AddWithValue("@PropietarioId", existente.PropietarioId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();
                        connection.Close();
                        return existente.PropietarioId; //Retornamos el ID del propietario reactivado
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Error al reactivar el propietario: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al reactivar el propietario: {ex.Message}", ex);
            }
        }

        try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    using (MySqlCommand command = new MySqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                        INSERT INTO propietarios (id_persona, estado) 
                        VALUES (@PersonaId, @Estado);
                        SELECT LAST_INSERT_ID();
                    ";

                        command.Parameters.AddWithValue("@PersonaId", p.PersonaId);
                        command.Parameters.AddWithValue("@Estado", 1); // Activo por defecto

                        connection.Open();
                        var result = command.ExecuteScalar();
                        connection.Close();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Error al dar de alta el propietario: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al dar de alta el propietario: {ex.Message}", ex);
            }
    }

    /// <summary> Baja lógica del propietario (actualiza estado a 0) </summary>
    /// <param name="propietarioId">ID del propietario</param>
    /// <returns>Número de filas afectadas</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public int Baja(int propietarioId)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE propietarios 
                        SET estado = @Estado 
                        WHERE id_propietario = @PropietarioId
                    ";

                    command.Parameters.AddWithValue("@Estado", 0);
                    command.Parameters.AddWithValue("@PropietarioId", propietarioId);
                    
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();
                    return result;
                }
            }
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al dar de baja el propietario: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al dar de baja el propietario: {ex.Message}", ex);
        }
    }

    /// <summary> NO IMPLEMENTADO.
    /// No deberia modificar un objeto PropietarioModel 
    /// solo guarda propietarioId y la personaId, 
    /// son datos que no deberian modificarse</summary>
    /// <param name="p">Modelo del propietario con datos actualizados</param>
    /// <returns>Número de filas afectadas</returns>
    /// <exception cref="NotImplementedException">Siempre lanza esta excepción.</exception>
    public int Modificacion(PropietarioModel p)
    {
        throw new NotImplementedException();
    }

    /// <summary> METODO POSIBLEMENTE INUTIL
    /// Obtiene todos los propietarios activos (solo PropietarioModel básico) 
    /// Es practicamente inutil, ya que no devuelve datos completos de la persona</summary>
    /// <returns>Lista de PropietarioModel</returns>
    public IList<PropietarioModel> ObtenerTodos()
    {
        throw new NotImplementedException();
    }

    /// <summary> Obtiene un propietario por su ID (Solo tendra personaId) </summary>
    /// <param name="id">ID del propietario</param>
    /// <returns>PropietarioModel o null</returns>
    public PropietarioModel? ObtenerPorId(int id)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT id_propietario, id_persona 
                        FROM propietarios 
                        WHERE id_propietario = @PropietarioId
                    ";

                    command.Parameters.AddWithValue("@PropietarioId", id);
                    
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var propietario = new PropietarioModel
                            {
                                PropietarioId = reader.GetInt32("id_propietario"),
                                PersonaId = reader.GetInt32("id_persona")
                            };
                            connection.Close();
                            return propietario;
                        }
                    }
                    connection.Close();
                }
            }
            
            return null;
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al obtener el propietario por ID: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al obtener el propietario por ID: {ex.Message}", ex);
        }
    }

    /// <summary> Obtiene todas las personas propietarias con paginación y búsqueda, 
    /// incluyendo datos de persona </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="search">Término de búsqueda opcional</param>
    /// <returns>Tupla con lista de PersonaModel con datos de persona y total de registros</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public (List<PersonaModel> Personas, int Total) ObtenerTodosConPaginacion(int page, int pageSize, string search = null)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // Para listar con paginación y búsqueda, se hacen dos consultas:
                // 1. Contar total de registros que cumplen el filtro (para paginación)
                // 2. Obtener los registros de la página actual con el filtro aplicado
                int total;
                connection.Open();

                // 1. Contar total de registros
                using (var countCommand = connection.CreateCommand())
                {
                    if (!string.IsNullOrEmpty(search)) //Si hay búsqueda, cuenta con filtro
                    {
                        countCommand.CommandText = $@"
                            SELECT COUNT(*) 
                            FROM propietarios pr
                            INNER JOIN personas p ON p.id_persona = pr.id_persona
                            WHERE (p.dni LIKE @search OR
                                        p.apellido LIKE @search OR
                                        p.nombre LIKE @search)
                            AND pr.estado = 1
                        ";
                        countCommand.Parameters.AddWithValue("@search", $"%{search}%");
                    }
                    else //Si no hay búsqueda, cuenta todos los activos
                    {
                        countCommand.CommandText = $@"
                            SELECT COUNT(*) 
                            FROM propietarios pr
                            INNER JOIN personas p ON p.id_persona = pr.id_persona
                            WHERE pr.estado = 1
                        ";
                    }

                    total = Convert.ToInt32(countCommand.ExecuteScalar());
                }

                // 2. Obtener los registros de la página actual con el filtro aplicado
                var personas = new List<PersonaModel>();

                using (var command = connection.CreateCommand())
                {
                    if (!string.IsNullOrEmpty(search)) //Si hay búsqueda, cuenta con filtro
                    {
                        command.CommandText = $@"
                            SELECT pr.id_propietario, pr.id_persona, pr.estado AS EstadoPropietario, 
                                p.dni, p.nombre, p.apellido, p.telefono, p.email
                            FROM propietarios pr
                            INNER JOIN personas p ON p.id_persona = pr.id_persona
                            WHERE (p.dni LIKE @search OR
                                            p.apellido LIKE @search OR
                                            p.nombre LIKE @search)
                            AND pr.estado = 1
                            ORDER BY p.apellido, p.nombre
                            LIMIT @Offset, @PageSize
                        ";
                        command.Parameters.AddWithValue("@search", $"%{search}%");
                    }
                    else //Si no hay búsqueda, cuenta todos los activos
                    {
                        command.CommandText = $@"
                            SELECT pr.id_propietario, pr.id_persona, pr.estado AS EstadoPropietario, 
                                p.dni, p.nombre, p.apellido, p.telefono, p.email
                            FROM propietarios pr
                            INNER JOIN personas p ON p.id_persona = pr.id_persona
                            WHERE pr.estado = 1
                            ORDER BY p.apellido, p.nombre
                            LIMIT @Offset, @PageSize
                        ";
                    }

                    command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            personas.Add(new PersonaModel
                            {
                                PersonaId = reader.GetInt32("id_persona"), // Nota: pierdes PropietarioId
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                                // Nota: pierdes Estado y PropietarioId
                            });
                        }
                    }
                    
                    connection.Close();
                }

                return (personas, total);
            }
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al obtener propietarios con persona: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al obtener propietarios con persona: {ex.Message}", ex);
        }
    }

    /// <summary> Verifica si existe un propietario para una persona específica </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>true si existe, false en caso contrario</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public (PropietarioModel propietario, int estado) ObtenerPorIdPersona(int personaId)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT id_propietario, id_persona , estado
                        FROM propietarios 
                        WHERE id_persona = @PersonaId
                    ";

                    command.Parameters.AddWithValue("@PersonaId", personaId);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var propietario = new PropietarioModel
                            {
                                PropietarioId = reader.GetInt32("id_propietario"),
                                PersonaId = reader.GetInt32("id_persona")
                            };
                            var estado = reader.GetInt32("estado");
                            return (propietario, estado);
                        }
                    }
                    connection.Close();
                }
            }

            return (null, 0);
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al obtener el propietario por ID: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al obtener el propietario por ID: {ex.Message}", ex);
        }
    }
}