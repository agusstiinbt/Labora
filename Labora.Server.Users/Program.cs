
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);


builder.AddServiceDefaults();

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


//DBContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//Esta linea declara que todos los endpoints requiern autorizacion. No afecta a los de Identity como Login porque por default ese siempre permite AllowAnonymous. No rompe nada y mejora la seguridad.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

//Si en algun momento queremos autenticacion por fuera de nuestra aplicacion debemos usar AddIdentityServer 
builder.Services.AddIdentityApiEndpoints<LaboraUser>()
    .AddRoles<LaboraRole>()// TODO cambiar por clase personalizada
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Reduce el intervalo de validación del SecurityStamp.
// Esto permite que cambios sensibles de seguridad (cambio de contraseña,
// roles, logins, etc.) invaliden más rápido las cookies existentes,
// forzando la regeneración del ClaimsPrincipal o el logout del usuario.
// Trade-off: mayor frecuencia de consultas al datastore.
// esto arrojaria un error de tipo 404 entonces debemos capturarlo y hacer un login denuevo
builder.Services.Configure<SecurityStampValidatorOptions>(o =>
                   o.ValidationInterval = TimeSpan.FromMinutes(5));

builder.Services.Configure<IdentityOptions>(options =>
{

    //Para mas info https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-10.0

    //Comportamiento Default para bloquear 
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    //Comportamiento Default de las passwords
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;


    //Comportamiento Default de Registro
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedAccount = false;


    //Comportamiento Default para el usuario
    options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    //Comportamiento Default para las cookies. Mas info: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions?view=aspnetcore-10.0

});


//Configuracion para los tokens generados temporalmente para: Cambios de contraseña, Confirmacion de email, cambio de email, etc.
//Estos tokens se envian por email o url, no se guardan en DB.
//Significa todo Token generado por Identity usando DataProtyection vence a las 3 horas de haber sido creado
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3);
});


builder.Services.ConfigureApplicationCookie(options =>
{
    //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    //options.LoginPath = "/Identity/Account/Login";
    options.Cookie.Name = "YourAppCookieName";
    options.Cookie.HttpOnly = true;//El valor predeterminado es verdadero, lo que significa que la cookie solo se pasará a solicitudes HTTP y no estará disponible para JavaScript en la página.
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    // ReturnUrlParameter requires 
    //using Microsoft.AspNetCore.Authentication.Cookies;
    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
    options.SlidingExpiration = true;

});

//Configuraciones de clase para acceder a appsettings
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);


//Transients
builder.Services.AddTransient<IEmailSender, EmailSender>();
//Scopes

//Singletones


//TODO construir un metodo por aqui para inicializar migraciones en el caso de que existan pendientes

var app = builder.Build();

app.MapIdentityApi<LaboraUser>();
//El llamado de MapIdentityApi agregar los siguientes endpoints:
//POST / register
//POST / login
//POST / refresh
//GET / confirmEmail
//POST / resendConfirmationEmail
//POST / forgotPassword
//POST / resetPassword
//POST / manage / 2fa
//GET /manage/info
//POST /manage/info


//Este es un ejemplo de como autenticar un endpoint sin necesidad con minimal APIS
app.MapGet("/weatherforecast", (HttpContext httpContext) =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
        })
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.RequireAuthorization("Admin");

app.MapPost("/logout", async (SignInManager<IdentityUser> signInManager,
    [FromBody] object empty) =>
{
    if (empty != null)
    {
        await signInManager.SignOutAsync();
        return Results.Ok();
    }
    return Results.Unauthorized();
})
.WithOpenApi()
.RequireAuthorization();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
