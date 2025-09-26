using UniDotNet.Models;                      //Para usar PersonaModel
using Microsoft.Extensions.Configuration;    //Para usar IConfiguration evita usar Microsoft.Extensions.Configuration.IConfiguration
using MySql.Data.MySqlClient;                //Para usar MySqlConnection, MySqlCommand, etc.

namespace UniDotNet.Repository;
/*
    Esto empieza a ser confuso, pero a medida que se agregan interfaces y herencias el código se vuelve mas complejo y ordenado.

    - La clase PersonaRepositorio hereda de BaseRepositorio esto le da el string de conexión y la configuración 
        a todos los repositorios que hereden de esta clase.

    - La interfaz IPersonaRepositorio define los métodos ADICIONALES que la clase PersonaRepositorio debe implementar.

    - La interfaz IPersonaRepositorio hereda de IRepositorio<PersonaModel>
        esta intefaz define los métodos BASE (genericos) que cualquier repositorio debe tener.

    Es realmente necesario este nivel de abstracción?
    Si no se utilizan intefaces, el codigo es mas simple y facil de entender.
    Pero si se utilizan interfaces, el codigo es mas menos flexible, mas estructurado y escalable.
    ademas:
        - Permite definir contratos claros para los repositorios.
        - Facilita la inyección de dependencias y el mocking en pruebas unitarias (ojala se de en lab III)
        - Promueve la reutilización de código y la separación de responsabilidades.
        - Mejora la mantenibilidad del código a largo plazo.
        - Rompe principios SOLID (investigar que mierda es eso)
        - Imposibilita testing con mocks
        - Elimina polimorfismo
        - No permite inyección de dependencias efectiva (se vera mas adelante en lab III)

    NOTA IMPORATANTE: En el proyecto actual no se utilizan hilos ni programación asíncrona.
    Por lo tanto, no es necesario implementar async/await en los métodos de acceso a datos.
    Pero en proyectos reales, es OBLIGATORIO usar async/await para mejorar la concurrencia
    y garantizar el rendimiento optimo para que multiples usuarios puedan acceder simultáneamente
    a la base de datos sin bloquear el hilo principal de la aplicación.

    Por el momento me abstenego de agregar async/await para no complicar mas el aprendizaje, mas bien
    tengo miedo de que se complique y mas adelante se vuelva un dolor de cabeza.
*/

/// <summary>
/// Repositorio concreto para manejar datos de personas.
/// </summary>
public class PersonaRepositorio : BaseRepositorio, IPersonaRepositorio
{
    public PersonaRepositorio(IConfiguration configuration) : base(configuration)
    {
    }

    /// <summary> Alta de una nueva persona. </summary>
    /// <param name="p">Objeto PersonaModel con los datos de la persona a agregar.</param>
    /// <returns>id del elemento agregado o codigo de error.</returns>
    /// <remarks>
    /// Códigos de error:
    /// -2: DNI ya existe
    /// -3: Email ya existe
    /// </remarks>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public int Alta(PersonaModel p)
    {
        try
        {
            // 1. Verificar duplicados ANTES de insertar
            if (ExistePorDni(p.Dni))
                return -2; // DNI ya existe

            if (ExistePorEmail(p.Email))
                return -3; // Email ya existe

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = @"INSERT INTO personas (dni, apellido, nombre, telefono, email, estado) 
                                VALUES (@dni, @apellido, @nombre, @telefono, @email, @estado);
                                SELECT LAST_INSERT_ID();";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@telefono", p.Telefono);
                    command.Parameters.AddWithValue("@email", p.Email);
                    command.Parameters.AddWithValue("@estado", 1);
                    connection.Open();

                    return Convert.ToInt32(command.ExecuteScalar()); // Devuelve el ID del nuevo registro
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al dar de alta la persona: " + ex.Message);
        }

    }

    /// <summary> Baja lógica de una persona (cambia estado a 0). </summary>
    /// <param name="personaId"></param>
    /// <returns>id del elemento eliminado.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public int Baja(int personaId)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = $@"UPDATE personas SET
                            estado = @Estado
                        WHERE id_persona = @id";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Estado", 0);
                    command.Parameters.AddWithValue("@id", personaId);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al dar de baja la persona: " + ex.Message);
        }
    }

    /// <summary> Modificación de una persona existente. </summary>
    /// <param name="p">Objeto PersonaModel con los datos actualizados de la persona.</param>
    /// <returns>id del elemento modificado.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public int Modificacion(PersonaModel p)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = @"UPDATE personas SET 
                            dni = @dni, 
                            apellido = @apellido, 
                            nombre = @nombre, 
                            telefono = @telefono, 
                            email = @email,
                            estado = 1
                            WHERE id_persona = @id";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@dni", p.Dni);
                    command.Parameters.AddWithValue("@apellido", p.Apellido);
                    command.Parameters.AddWithValue("@nombre", p.Nombre);
                    command.Parameters.AddWithValue("@telefono", p.Telefono);
                    command.Parameters.AddWithValue("@email", p.Email);
                    command.Parameters.AddWithValue("@id", p.PersonaId);

                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }

        }
        catch (Exception ex)
        {
            throw new Exception("Error al modificar a la persona: " + ex.Message);
        }
    }

    /// <summary> Obtiene todas las personas activas (estado = 1). </summary>
    /// <returns>Una lista con todas las personas activas.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public IList<PersonaModel> ObtenerTodos()
    {
        throw new NotImplementedException();
    }

    /// <summary> Obtiene una persona por su ID. </summary>
    /// <param name="id">El ID de la persona a obtener.</param>
    /// <returns>El objeto PersonaModel correspondiente al ID, o null si no se encuentra.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación
    /// o si no se encuentra la persona.</exception>
    public PersonaModel? ObtenerPorId(int id)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = @"SELECT id_persona, dni, apellido, nombre, telefono, email 
                            FROM personas 
                            WHERE id_persona = @id AND estado = 1";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PersonaModel
                            {
                                PersonaId = reader.GetInt32("id_persona"),
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener la persona por ID: " + ex.Message);
        }

        return null;
    }

    /// <summary> Obtiene una persona por ID sin filtrar por estado (incluye inactivas) </summary>
    /// <param name="id">El ID de la persona a obtener.</param>
    /// <returns>El objeto PersonaModel correspondiente al ID, o null si no se encuentra.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación</exception>
    public PersonaModel? ObtenerPorIdSinFiltro(int id)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = @"SELECT id_persona, dni, apellido, nombre, telefono, email 
                            FROM personas 
                            WHERE id_persona = @id";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PersonaModel
                            {
                                PersonaId = reader.GetInt32("id_persona"),
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener la persona por ID sin filtro: " + ex.Message);
        }

        return null;
    }

    /// <summary> Obtiene todas las personas con sus roles asociados, aplicando paginación y búsqueda. </summary>
    /// <param name="page">Número de página (1-based)</param>
    /// <param name="pageSize">Número de elementos por página</param>
    /// <param name="search">Texto a buscar (opcional)</param>
    /// <param name="estado">Estado de las personas a obtener (true=activas, false=dadas de baja)</param>
    /// <returns>Una tupla con la lista de personas y el total de registros encontrados</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación</exception>
    public (List<dynamic> PersonasConRoles, int Total) ObtenerTodosConPaginacion(int page, int pageSize, string? search = null, bool estado = true)
    {
        var personasConRoles = new List<dynamic>();
        int total = 0;

        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                // Para listar con paginación y búsqueda, se hacen dos consultas:
                // 1. Contar total de registros que cumplen el filtro (para paginación)
                // 2. Obtener los registros de la página actual con el filtro aplicado
                connection.Open();

                // 1. Contar total de registros
                using (var countCommand = connection.CreateCommand())
                {
                    int estadoValue = estado ? 1 : 0;
                    
                    if (!string.IsNullOrEmpty(search)) //Si hay búsqueda, cuenta con filtro
                    {
                        countCommand.CommandText = $@"
                            SELECT COUNT(*) 
                            FROM personas p
                            LEFT JOIN propietarios prop ON p.id_persona = prop.id_persona
                            LEFT JOIN inquilinos inq ON p.id_persona = inq.id_persona  
                            LEFT JOIN empleados emp ON p.id_persona = emp.id_persona
                            WHERE (p.dni LIKE @search OR
                                        p.apellido LIKE @search OR
                                        p.nombre LIKE @search)
                            AND p.estado = @estado
                        ";

                        countCommand.Parameters.AddWithValue("@search", $"%{search}%");
                        countCommand.Parameters.AddWithValue("@estado", estadoValue);
                    }
                    else //Si no hay búsqueda, cuenta todos los registros del estado especificado
                    {
                        countCommand.CommandText = @"
                            SELECT COUNT(*) 
                            FROM personas p
                            LEFT JOIN propietarios prop ON p.id_persona = prop.id_persona
                            LEFT JOIN inquilinos inq ON p.id_persona = inq.id_persona  
                            LEFT JOIN empleados emp ON p.id_persona = emp.id_persona
                            WHERE p.estado = @estado
                        ";
                        
                        countCommand.Parameters.AddWithValue("@estado", estadoValue);
                    }

                    total = Convert.ToInt32(countCommand.ExecuteScalar());
                }

                //2. Obtener los registros de la página actual
                using (var command = connection.CreateCommand())
                {
                    int estadoValue = estado ? 1 : 0;
                    
                    if (!string.IsNullOrEmpty(search)) //Si hay búsqueda, cuenta con filtro
                    {
                        command.CommandText = @"
                            SELECT 
                                p.id_persona,
                                p.dni,
                                p.apellido,
                                p.nombre,
                                p.telefono,
                                p.email,
                                CASE WHEN prop.id_propietario IS NOT NULL AND prop.estado = 1 THEN 1 ELSE 0 END as EsPropietario,
                                CASE WHEN inq.id_inquilino IS NOT NULL AND inq.estado = 1 THEN 1 ELSE 0 END as EsInquilino,
                                CASE WHEN emp.id_empleado IS NOT NULL AND emp.estado = 1 THEN 1 ELSE 0 END as EsEmpleado
                            FROM personas p
                            LEFT JOIN propietarios prop ON p.id_persona = prop.id_persona
                            LEFT JOIN inquilinos inq ON p.id_persona = inq.id_persona  
                            LEFT JOIN empleados emp ON p.id_persona = emp.id_persona
                            WHERE (p.dni LIKE @search OR
                                        p.apellido LIKE @search OR
                                        p.nombre LIKE @search)
                            AND p.estado = @estado
                            ORDER BY p.apellido, p.nombre
                            LIMIT @Offset, @PageSize
                        ";

                        // Parámetro de búsqueda con comodines para LIKE
                        command.Parameters.AddWithValue("@search", $"%{search}%");
                        command.Parameters.AddWithValue("@estado", estadoValue);

                    }
                    else //Si no hay búsqueda, obtiene todos los registros del estado especificado
                    {
                        command.CommandText = @"
                            SELECT 
                                p.id_persona,
                                p.dni,
                                p.apellido,
                                p.nombre,
                                p.telefono,
                                p.email,
                                CASE WHEN prop.id_propietario IS NOT NULL AND prop.estado = 1 THEN 1 ELSE 0 END as EsPropietario,
                                CASE WHEN inq.id_inquilino IS NOT NULL AND inq.estado = 1 THEN 1 ELSE 0 END as EsInquilino,
                                CASE WHEN emp.id_empleado IS NOT NULL AND emp.estado = 1 THEN 1 ELSE 0 END as EsEmpleado
                            FROM personas p
                            LEFT JOIN propietarios prop ON p.id_persona = prop.id_persona
                            LEFT JOIN inquilinos inq ON p.id_persona = inq.id_persona  
                            LEFT JOIN empleados emp ON p.id_persona = emp.id_persona
                            WHERE p.estado = @estado
                            ORDER BY p.apellido, p.nombre
                            LIMIT @Offset, @PageSize
                        ";
                        
                        command.Parameters.AddWithValue("@estado", estadoValue);
                    }

                    // Parámetros para paginación
                    command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            personasConRoles.Add(new
                            {
                                PersonaId = reader.GetInt32("id_persona"),
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email"),
                                EsPropietario = reader.GetBoolean("EsPropietario"),
                                EsInquilino = reader.GetBoolean("EsInquilino"),
                                EsEmpleado = reader.GetBoolean("EsEmpleado"),
                                NombreCompleto = $"{reader.GetString("apellido")}, {reader.GetString("nombre")}",
                                Roles = GetRolesString(reader.GetBoolean("EsPropietario"),
                                                    reader.GetBoolean("EsInquilino"),
                                                    reader.GetBoolean("EsEmpleado"))
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener las personas con roles: " + ex.Message);
        }

        return (personasConRoles, total);
    }

    /// <summary> Método auxiliar para generar string con los roles de una persona </summary>
    /// <param name="esPropietario">Indica si la persona es propietario</param>
    /// <param name="esInquilino">Indica si la persona es inquilino</param>
    /// <param name="esEmpleado">Indica si la persona es empleado</param>
    /// <returns>String con los roles separados por comas, o "Sin rol asignado" si no tiene ninguno</returns>
    /// <remarks>
    /// - esPropietario=true, esInquilino=false, esEmpleado=true  => "Propietario, Empleado"
    /// - esPropietario=false, esInquilino=true, esEmpleado=false => "Inquilino"
    /// - esPropietario=false, esInquilino=false, esEmpleado=false => "Sin rol asignado"
    /// </remarks>
    private string GetRolesString(bool esPropietario, bool esInquilino, bool esEmpleado)
    {
        var roles = new List<string>();
        
        if (esPropietario) roles.Add("Propietario");
        if (esInquilino) roles.Add("Inquilino");
        if (esEmpleado) roles.Add("Empleado");
        
        return roles.Count > 0 ? string.Join(", ", roles) : "Sin rol asignado";
    }

    /// <summary> Obtiene una persona por su DNI. </summary>
    /// <param name="dni">El DNI de la persona a obtener.</param>
    /// <returns>El objeto PersonaModel correspondiente al DNI, o null si no se encuentra.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación
    /// o si no se encuentra la persona.</exception>
    public PersonaModel ObtenerPorDni(string dni)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = @"SELECT id_persona, dni, apellido, nombre, telefono, email 
                            FROM personas 
                            WHERE dni = @dni AND estado = 1";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@dni", dni);
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new PersonaModel
                            {
                                PersonaId = reader.GetInt32("id_persona"),
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            };
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al obtener la persona por DNI: " + ex.Message);
        }

        return null;
    }

    /// <summary> Busca personas por nombre o apellido que contengan el texto dado. </summary>
    /// <param name="nombre">El texto a buscar en nombre o apellido.</param>
    /// <returns>Una lista con las personas que coinciden con el criterio de búsqueda.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public IList<PersonaModel> BuscarPorNombre(string nombre)
    {
        var personas = new List<PersonaModel>();

        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = @"SELECT id_persona, dni, apellido, nombre, telefono, email 
                            FROM personas 
                            WHERE (nombre LIKE @nombre OR apellido LIKE @nombre) 
                            AND estado = 1 
                            ORDER BY apellido, nombre";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nombre", $"%{nombre}%");
                    connection.Open();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            personas.Add(new PersonaModel
                            {
                                PersonaId = reader.GetInt32("id_persona"),
                                Dni = reader.GetString("dni"),
                                Apellido = reader.GetString("apellido"),
                                Nombre = reader.GetString("nombre"),
                                Telefono = reader.GetString("telefono"),
                                Email = reader.GetString("email")
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al buscar personas por nombre: " + ex.Message);
        }

        return personas;
    }

    /// <summary> Verifica si existe una persona con el DNI dado. </summary>
    /// <param name="dni">El DNI a verificar.</param>
    /// <returns>True si existe, false en caso contrario.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public bool ExistePorDni(string dni)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = "SELECT COUNT(*) FROM personas WHERE dni = @dni";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@dni", dni);
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al verificar la existencia de la persona por DNI: " + ex.Message);
        }
    }

    /// <summary> Verifica si existe una persona con el email dado. </summary>
    /// <param name="email">El email a verificar.</param>
    /// <returns>True si existe, false en caso contrario.</returns>
    /// <exception cref="Exception">Lanza una excepción si ocurre un error durante la operación.</exception>
    public bool ExistePorEmail(string email)
    {
        try
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                var query = "SELECT COUNT(*) FROM personas WHERE email = @email";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Error al verificar la existencia de la persona por email: " + ex.Message);
        }
    }

}

/**
El símbolo ":" (dos puntos)
    Establece relaciones de herencia e implementación

    - Herencia de clase: 
        Cuando aparece después del nombre de la clase seguido de otra clase, 
        establece una relación de herencia donde la clase derivada hereda 
        todos los miembros (campos, propiedades, métodos) de la clase base.
    - Implementación de interfaz: 
        Cuando aparece seguido de una interfaz, establece un contrato que obliga 
        a la clase a implementar todos los métodos abstractos definidos en esa interfaz.

El símbolo "," (coma)
    Separador de múltiples relaciones
    Permite que una clase establezca múltiples relaciones simultáneamente:

    - Una clase puede heredar de una sola clase base
    - Una clase puede implementar múltiples interfaces

    Ejemplo:
        La sintaxis es: ClaseDerivada : ClaseBase, IInterfaz1, IInterfaz2

    NOTA: 
        Restricción importante: 
            En C#, solo se permite herencia simple de clases,
            (no soporta herencia múltiple) pero implementación múltiple de interfaces.

El ": base(configuration)"
    Invocación del constructor de la clase base

    - base es una palabra clave que hace referencia a la clase padre
    - base(configuration) invoca explícitamente el constructor de la clase base
    - Se ejecuta antes que el cuerpo del constructor de la clase derivada
    - Propaga el parámetro configuration hacia el constructor de la clase padre
    - Es obligatorio cuando la clase base no tiene un constructor sin parámetros

La forma de instanciar de cris era la correcta para uso de C# moderno.
En su momento no pude entenderlo, pero ahora sí. De todas formas,
me quedo con esta ya que es mas facil de leer, entender y a futuro explicar.

Forma moderna
"public class PersonaRepositoryImpl(IConfiguration configuration) : BaseRepository(configuration), IPersonaRepository"
Esta forma es correcta y válida en C# 12.0 o 9.0 (falta verificar cual) y versiones posteriores.
 */