using UniDotNet.Repository;

// Se crea el builder, es el inicializador y configurador 
//del host de la aplicacion web ASP.NET Core
// cargando las configuraciones desde appsettings.json y variables de entorno
// y configurando servicios como logging? y DI? (Dependency Injection).
var builder = WebApplication.CreateBuilder(args);

//Registra los servicios necesarios para usar el patrón MVC, 
//incluyendo soporte para controladores y vistas Razor.
builder.Services.AddControllersWithViews();

// Configuración de inyección de dependencias para los repositorios
builder.Services.AddScoped<IPersonaRepositorio, PersonaRepositorio>();
builder.Services.AddScoped<IPropietarioRepositorio, PropietarioRepositorio>();
builder.Services.AddScoped<IInquilinoRepositorio, InquilinoRepositorio>();
builder.Services.AddScoped<IEmpleadoRepositorio, EmpleadoRepositorio>();

// Configuración de sesiones para soporte de TempData en controladores
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuración de caché en memoria para mejor rendimiento
builder.Services.AddMemoryCache();

//Se crea la instancia de la aplicación web (WebApplication).
//Ya esta lista para configurar el pipeline de solicitudes HTTP
//y definir las rutas y middlewares necesarios.
var app = builder.Build();

// Aqui se configura el pipeline de solicitudes HTTP
// y se definen middlewares y rutas.

// Para cambiar entre entornos de desarrollo y producción
// se puede usar la variable de entorno ASPNETCORE_ENVIRONMENT
// windows CMD -> set ASPNETCORE_ENVIRONMENT=Development
// linux terminal -> export ASPNETCORE_ENVIRONMENT=Development
// luego ejecutar dotnet run
// para producción set ASPNETCORE_ENVIRONMENT=Production
// o no definir la variable de entorno (Production es el valor por defecto)

// Si la aplicación no está en modo desarrollo, 
// se configura para manejar errores redirigiendo a una página de error 
// personalizada y habilitando HSTS (HTTP Strict Transport Security)
if (!app.Environment.IsDevelopment())
{
    // Configura el middleware para manejar excepciones no controladas
    // redirigiendo a la acción Error del controlador Home/Error
    //sin mostrar detalles tecnicos del error al usuario final
    app.UseExceptionHandler("/Home/Error");
    // Habilita middleware de seguridad HSTS Strict Transport Security
    // para agregar encabezados HTTP que indican a los navegadores 
    // que solo deben comunicarse con el servidor a través de HTTPS
    // esto dura 30 dias por defecto.
    app.UseHsts();
}

app.UseHttpsRedirection();
// Redirige automáticamente las solicitudes HTTP a HTTPS.

app.UseStaticFiles();
// Habilita el acceso a archivos estáticos (CSS, JS, imágenes) desde wwwroot.

app.UseRouting();
// Habilita el sistema de enrutamiento para routear URLs a controladores
//Los controladores a routear son los que heredan de Controller, y estan
//en la carpeta Controllers. El router se fija en el nombre del controlador
//y en el nombre de la acción (método) para mapear las URLs entrantes
//a los controladores y acciones correspondientes.

// Middleware de sesiones debe ir antes de la autorización
app.UseSession();

app.UseAuthorization();
// Esto se usa en autenticacion, para establecer políticas de autorización
// y controlar el acceso a recursos protegidos en la aplicación web.

//app.MapStaticAssets();
// ni idea que hace esto, lo dejo comentado por las dudas
// mas adelante con mas tiempo investigo bien.

/* Define la ruta por defecto para los controladores.
Si no se especifica un controlador o acción en la URL,
se usará el controlador "Home" y la acción "Index" por defecto.
El parámetro {id?} es opcional y puede ser utilizado para pasar un ID */
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
    ).WithStaticAssets();
/* WithStaticAssets (Novedad en .NET 9):
   Habilita automáticamente el servido de archivos 
   estáticos (como UseStaticFiles()) para esa ruta, 
   optimizando el rendimiento en escenarios específicos. */

// Inicia la aplicación web y comienza a escuchar solicitudes HTTP entrantes
app.Run();
