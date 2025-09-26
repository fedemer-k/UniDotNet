//Importa componentes fundamentales del framework como Console, DateTime, Exception, entre otros.
using System;

//Habilita el uso de estructuras de datos genéricas como List<T>, Dictionary<TKey, TValue>, etc.
using System.Collections.Generic;

//Activa LINQ (Language Integrated Query), facilitando consultas sobre colecciones mediante métodos como .Where(), .Select(), .FirstOrDefault(), etc.
using System.Linq;

//Incluye funcionalidades para programación asíncrona mediante Task, async, await.
using System.Threading.Tasks; //Estaria bueno implementar async en la obtencion de datos

//Establece el espacio de nombres específico del proyecto, organizando clases e interfaces bajo una identificación lógica común

//Importa libreria que accede a la interfaz IConfiguration:
//    lee valores del archivo appsettings.json
using Microsoft.Extensions.Configuration;

namespace UniDotNet.Repository; //A partir de C# 10.0, se puede definir el namespace de esta manera sin llaves ni indentación.

/// <summary>
/// Clase base para repositorios que maneja la configuración y cadena de conexión.
/// - Contiene datos y operaciones comunes que se heredaran a sus hijos pero NO les dice QUÉ operaciones deben hacer.
/// - Centraliza la infraestructura (conexion configuracion).
/// - Evita duplicación de código en repositorios concretos.
/// NOTA: Utiliza IConfiguration para obtener valores desde appsettings.json.
/// </summary>
public abstract class BaseRepositorio{

    //Almacena la configuración inyectada en el constructor y extrae la cadena de conexión.
    protected readonly IConfiguration configuration;
    protected readonly string connectionString;


    //Constructor que recibe la configuración (por inyección de dependencias) y obtiene la cadena de conexión desde appsettings.json.
    protected BaseRepositorio(IConfiguration configuration)
    {
        this.configuration = configuration;
        connectionString = configuration["ConnectionStrings:MySql"];
    }
}