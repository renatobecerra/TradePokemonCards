//Cod extra
using Backend.Models;
using Microsoft.EntityFrameworkCore;
//Fin cod extra

var builder = WebApplication.CreateBuilder(args);

//LINEA NUEVA AGREGADA 
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // URL por defecto de Angular
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
//FIN DE LINEA AGREGADA

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//PASO DE PRUEBAAAA PARA BACKEND
// Lee la cadena de conexión desde appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registra el contexto usando esa cadena en MySQL
builder.Services.AddDbContext<PokemonMarketContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
builder.Services.AddControllers();
//FIN SECCION DE COD EXTRA

var app = builder.Build();

//Nueva LINEA 
app.UseCors("PermitirAngular");
//FIN NUEVA LINEA 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//lInea extra
app.MapControllers();
//Fin linea extra
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
