using GeoTrack.API.Attributes;
using GeoTrack.API.Infrastructure;
using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Domain.Contracts.CiudadRepository;
using GeoTrack.API.Domain.Contracts.DepartamentoRepository;
using GeoTrack.API.Domain.Contracts.PaisRepository;
using GeoTrack.API.Domain.Services;
using GeoTrack.API.Domain.Services.CiudadService;
using GeoTrack.API.Domain.Services.DepartamentoService;
using GeoTrack.API.Domain.Services.PaisService;
using GeoTrack.API.Extensions;
using GeoTrack.API.Util.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios al contenedor
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger usando extensiones
builder.Services.AddCustomSwagger();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policyBuilder => policyBuilder
            .WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ??
                new[] { "http://localhost:5000", "https://localhost:5001" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// Configurar DbContext
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar autenticación JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key no configurada"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = !string.IsNullOrEmpty(jwtSection["Issuer"]),
        ValidIssuer = jwtSection["Issuer"],
        ValidateAudience = !string.IsNullOrEmpty(jwtSection["Audience"]),
        ValidAudience = jwtSection["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Registrar filtros y atributos personalizados
builder.Services.AddScoped<AccesoAttribute>();
builder.Services.AddScoped<ExceptionAttribute>();
builder.Services.AddScoped<LogAttribute>();
builder.Services.AddScoped<ValidarModeloAttribute>();
builder.Services.AddScoped<JwtAuthorizationAttribute>();

// Registrar servicios de repositorios
builder.Services.AddScoped<IAccesoRepository, AccesoRepository>();
builder.Services.AddScoped<ILogRepository, LogRepository>();
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPaisRepository, PaisRepository>();
builder.Services.AddScoped<IDepartamentoRepository, DepartamentoRepository>();
builder.Services.AddScoped<ICiudadRepository, CiudadRepository>();

// Configurar filtros globales
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionAttribute>();
});

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseCustomSwagger();
}
else
{
    // En producción, usar middleware personalizado para manejo de errores
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCustomCors();

app.UseRouting();

// Middleware de logging
app.UseMiddleware<LoggingMiddleware>();

// Middleware de autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Usar endpoints personalizados
app.UseCustomEndpoints();

// Crear la base de datos y aplicar migraciones en desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();

    // Asegurarse de que la base de datos esté creada y las migraciones aplicadas
    if (dbContext.Database.EnsureCreated())
    {
        Console.WriteLine("Base de datos creada correctamente");
    }

    // Aplicar migraciones pendientes
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        Console.WriteLine("Aplicando migraciones pendientes...");
        dbContext.Database.Migrate();
        Console.WriteLine("Migraciones aplicadas correctamente");
    }
}

app.Run();
