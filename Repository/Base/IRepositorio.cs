namespace UniDotNet.Repository;
/// <summary>
/// Interfaz genérica para operaciones CRUD.
/// - Define QUE debe hacer cada repositorio, pero NO CÓMO hacerlo.
/// - Establece un contrato que las clases concretas deben implementar.
/// - Permite trabajar con cualquier tipo de entidad T.
/// - Facilita la reutilización y el mantenimiento del código.
/// </summary>
public interface IRepositorio<T>
{
    /// <summary>
    /// Agrega un nuevo elemento.
    /// </summary>
    /// <param name="p">El elemento a agregar.</param>
    /// <returns>id del elemento agregado.</returns>
    int Alta(T p);

    /// <summary>
    /// Elimina un elemento por su ID.
    /// </summary>
    /// <param name="p">El elemento a eliminar.</param>
    /// <returns>id del elemento eliminado.</returns>
    int Baja(int p);

    /// <summary>
    /// Modifica un elemento existente.
    /// </summary>
    /// <param name="p">El elemento a modificar.</param>
    /// <returns>id del elemento modificado.</returns>
    int Modificacion(T p);

    /// <summary>
    /// Obtiene todos los elementos.
    /// </summary>
    /// <returns>Una lista con todos los elementos.</returns>
    IList<T> ObtenerTodos();

    /// <summary>
    /// Obtiene un elemento por su ID.
    /// </summary>
    /// <param name="id">El ID del elemento a obtener.</param>
    /// <returns>El elemento correspondiente al ID, o null si no se encuentra.</returns>
    T? ObtenerPorId(int id);
}
/**
Establece un contrato genérico para operaciones CRUD sobre entidades de tipo T.

POR QUÉ SE LLAMA "CONTRATO":

1. OBLIGATORIEDAD: Si implementas IRepositorio<T>, DEBES proporcionar todos los métodos
2. GARANTÍA: El código que usa IRepositorio<T> SABE que esos métodos estarán disponibles
3. ESPECIFICACIÓN: Define exactamente qué firma deben tener los métodos
4. INTERCAMBIABILIDAD: Cualquier implementación que cumpla el contrato es válida

POR QUÉ ES "GENÉRICO":

1. REUTILIZABLE: Funciona con Propiedad, Usuario, Producto, etc.
2. PARAMETRIZADO: El tipo T se especifica al usar el contrato
3. FLEXIBLE: Una definición sirve para múltiples tipos de datos
4. ABSTRACCIÓN: No está atado a una entidad específica

ANALOGÍA CON CONTRATOS LEGALES:
- Un contrato legal dice "debes hacer X, Y, Z"
- Una interfaz dice "debes implementar métodos A, B, C con estas firmas"
- Ambos establecen obligaciones que deben cumplirse
- Ambos garantizan cierto comportamiento esperado

USING:
- Reducen la verbosidad del código: en lugar de System.Collections.Generic.List<T>, permite escribir únicamente List<T>.
- Estructuran el código: permiten identificar qué funcionalidades se están empleando.
- Simplifican el mantenimiento: al observar un using System.Linq, se identifica inmediatamente la presencia de consultas LINQ en el archivo. 

**/