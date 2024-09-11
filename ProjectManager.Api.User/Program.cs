using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManager.Api.User.Context;
using ProjectManager.Api.User.Models;
using ProjectManager.Api.User.Repositories;
using ProjectManager.Api.User.Services;
using ProjectManager.Api.User.Utils;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configuración del contexto de la base de datos
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar el repositorio de usuarios
builder.Services.AddScoped<UsersRepository>();

// Registrar JwtSettings desde appsettings.json
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Registrar el servicio de JWT Utils
builder.Services.AddScoped<JwtUtils>();

// Add services to the container.
builder.Services.AddControllers();

// Registrar el servicio de Imgbb en lugar de Cloudinary
builder.Services.AddHttpClient<ImgbbService>(); // HttpClient es necesario para ImgbbService

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policyBuilder => policyBuilder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
});

// Configuración de JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.ASCII.GetBytes(jwtSettings.SecretKey);

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
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Elimina el retraso en la expiración del token
        };
    });

// Configurar Kestrel para deshabilitar HTTPS en desarrollo
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5078); // Configuración para HTTP en puerto 5078
        options.ListenAnyIP(7295, listenOptions => // Configuración para HTTPS en puerto 7295
        {
            listenOptions.UseHttps();
        });
    });
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Deshabilitar redirección HTTPS
// app.UseHttpsRedirection(); // Comentar o eliminar esta línea

app.UseCors("AllowAllOrigins");

app.UseAuthentication(); // Asegúrate de agregar autenticación antes de autorización
app.UseAuthorization();

app.MapControllers();

app.Run();
