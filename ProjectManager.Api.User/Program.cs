using ProjectManager.Api.User.Context;
using ProjectManager.Api.User.Repositories;
using Microsoft.EntityFrameworkCore;
using ProjectManager.Api.User.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuracion de la cadena de conexion
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configuracion del contexto de la base de datos
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar el repositorio de usuarios
builder.Services.AddScoped<UsersRepository>();

// Add services to the container.
builder.Services.AddControllers();

// Registrar el servicio de Cloudinary
builder.Services.AddScoped<CloudinaryService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();