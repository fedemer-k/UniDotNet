using UniDotNet.Models;                      //Para usar InquilinoModel
using Microsoft.Extensions.Configuration;    //Para usar IConfiguration
using MySql.Data.MySqlClient;                //Para usar MySqlConnection, MySqlCommand, etc.

namespace UniDotNet.Repository;

/// <summary> Repositorio concreto para manejar datos de inquilinos. </summary>
public class InquilinoRepositorio : BaseRepositorio, IInquilinoRepositorio
{
    public InquilinoRepositorio(IConfiguration configuration) : base(configuration){}

    /// <summary> Da de alta un inquilino. Si InquilinoId es 0, crea uno nuevo. </summary>
    /// <param name="p">Modelo del inquilino</param>
    /// <returns>ID del inquilino creado</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public int Alta(InquilinoModel p)
    {
        //Primero debo hacer uso de ObtenerPorIdPersona
        //para verificar que no exista ya un inquilino para esa persona y se encuentre desactivado
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
                            UPDATE inquilinos 
                            SET estado = @Estado 
                            WHERE id_inquilino = @InquilinoId
                        ";

                        command.Parameters.AddWithValue("@Estado", 1);
                        command.Parameters.AddWithValue("@InquilinoId", existente.InquilinoId);

                        connection.Open();
                        int result = command.ExecuteNonQuery();
                        connection.Close();
                        return existente.InquilinoId; //Retornamos el ID del inquilino reactivado
                    }
                }
            }
            catch (MySqlException ex)
            {
                throw new Exception($"Error al reactivar el inquilino: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al reactivar el inquilino: {ex.Message}", ex);
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
                        INSERT INTO inquilinos (id_persona, estado) 
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
            throw new Exception($"Error al dar de alta el inquilino: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al dar de alta el inquilino: {ex.Message}", ex);
        }
    }

    /// <summary> Baja lógica del inquilino (actualiza estado a 0) </summary>
    /// <param name="inquilinoId">ID del inquilino</param>
    /// <returns>Número de filas afectadas</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public int Baja(int inquilinoId)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE inquilinos 
                        SET estado = @Estado 
                        WHERE id_inquilino = @InquilinoId
                    ";

                    command.Parameters.AddWithValue("@Estado", 0);
                    command.Parameters.AddWithValue("@InquilinoId", inquilinoId);
                    
                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    connection.Close();
                    return result;
                }
            }
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al dar de baja el inquilino: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al dar de baja el inquilino: {ex.Message}", ex);
        }
    }

    /// <summary> NO IMPLEMENTADO.
    /// No deberia modificar un objeto InquilinoModel 
    /// solo guarda inquilinoId y la personaId, 
    /// son datos que no deberian modificarse</summary>
    /// <param name="p">Modelo del inquilino con datos actualizados</param>
    /// <returns>Número de filas afectadas</returns>
    /// <exception cref="NotImplementedException">Siempre lanza esta excepción.</exception>
    public int Modificacion(InquilinoModel p)
    {
        throw new NotImplementedException();
    }

    /// <summary> METODO POSIBLEMENTE INUTIL
    /// Obtiene todos los inquilinos activos (solo InquilinoModel básico) 
    /// Es practicamente inutil, ya que no devuelve datos completos de la persona</summary>
    /// <returns>Lista de InquilinoModel</returns>
    public IList<InquilinoModel> ObtenerTodos()
    {
        throw new NotImplementedException();
    }

    /// <summary> Obtiene un inquilino por su ID (Solo tendra personaId) </summary>
    /// <param name="id">ID del inquilino</param>
    /// <returns>InquilinoModel o null</returns>
    public InquilinoModel? ObtenerPorId(int id)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT id_inquilino, id_persona 
                        FROM inquilinos 
                        WHERE id_inquilino = @InquilinoId
                    ";

                    command.Parameters.AddWithValue("@InquilinoId", id);
                    
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var inquilino = new InquilinoModel
                            {
                                InquilinoId = reader.GetInt32("id_inquilino"),
                                PersonaId = reader.GetInt32("id_persona")
                            };
                            connection.Close();
                            return inquilino;
                        }
                    }
                    connection.Close();
                }
            }
            
            return null;
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al obtener el inquilino por ID: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al obtener el inquilino por ID: {ex.Message}", ex);
        }
    }

    /// <summary> Obtiene todas las personas inquilinas con paginación y búsqueda, 
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
                            FROM inquilinos inq
                            INNER JOIN personas p ON p.id_persona = inq.id_persona
                            WHERE (p.dni LIKE @search OR
                                        p.apellido LIKE @search OR
                                        p.nombre LIKE @search)
                            AND inq.estado = 1
                        ";
                        countCommand.Parameters.AddWithValue("@search", $"%{search}%");
                    }
                    else //Si no hay búsqueda, cuenta todos los activos
                    {
                        countCommand.CommandText = $@"
                            SELECT COUNT(*) 
                            FROM inquilinos inq
                            INNER JOIN personas p ON p.id_persona = inq.id_persona
                            WHERE inq.estado = 1
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
                            SELECT inq.id_inquilino, inq.id_persona, inq.estado AS EstadoInquilino, 
                                p.dni, p.nombre, p.apellido, p.telefono, p.email
                            FROM inquilinos inq
                            INNER JOIN personas p ON p.id_persona = inq.id_persona
                            WHERE (p.dni LIKE @search OR
                                            p.apellido LIKE @search OR
                                            p.nombre LIKE @search)
                            AND inq.estado = 1
                            ORDER BY p.apellido, p.nombre
                            LIMIT @Offset, @PageSize
                        ";
                        command.Parameters.AddWithValue("@search", $"%{search}%");
                    }
                    else //Si no hay búsqueda, cuenta todos los activos
                    {
                        command.CommandText = $@"
                            SELECT inq.id_inquilino, inq.id_persona, inq.estado AS EstadoInquilino, 
                                p.dni, p.nombre, p.apellido, p.telefono, p.email
                            FROM inquilinos inq
                            INNER JOIN personas p ON p.id_persona = inq.id_persona
                            WHERE inq.estado = 1
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
                                PersonaId = reader.GetInt32("id_persona"), // Nota: pierdes InquilinoId
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                                // Nota: pierdes Estado y InquilinoId
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
            throw new Exception($"Error al obtener inquilinos con persona: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al obtener inquilinos con persona: {ex.Message}", ex);
        }
    }

    /// <summary> Obtiene un inquilino por ID de persona (incluye inactivos) </summary>
    /// <param name="personaId">ID de la persona</param>
    /// <returns>InquilinoModel si existe, null en caso contrario</returns>
    /// <exception cref="Exception">Lanza excepción si ocurre un error.</exception>
    public (InquilinoModel inquilino, int estado) ObtenerPorIdPersona(int personaId)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT id_inquilino, id_persona, estado
                        FROM inquilinos 
                        WHERE id_persona = @PersonaId
                    ";

                    command.Parameters.AddWithValue("@PersonaId", personaId);

                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var inquilino = new InquilinoModel
                            {
                                InquilinoId = reader.GetInt32("id_inquilino"),
                                PersonaId = reader.GetInt32("id_persona")
                            };
                            var estado = reader.GetInt32("estado");
                            return (inquilino, estado);
                        }
                    }
                    connection.Close();
                }
            }
            
            return (null, 0);
        }
        catch (MySqlException ex)
        {
            throw new Exception($"Error al obtener el inquilino por ID: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error inesperado al obtener el inquilino por ID: {ex.Message}", ex);
        }
    }
}