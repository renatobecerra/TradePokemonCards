using Backend.Models;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PokemonMarketContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PokemonMarketContext>();
    try { db.Database.ExecuteSqlRaw("ALTER TABLE usuario ADD COLUMN MotivoBaneo VARCHAR(255) NULL;"); } catch {}
    try { db.Database.ExecuteSqlRaw("ALTER TABLE usuario ADD COLUMN FechaDesbaneo DATETIME NULL;"); } catch {}
    try {
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS transacciones (
                IdTransaccion INT AUTO_INCREMENT PRIMARY KEY,
                IdVendedor INT NOT NULL,
                IdComprador INT NOT NULL,
                IdInventarioUser INT NOT NULL,
                Precio INT NULL,
                Fecha DATETIME NOT NULL,
                Estado VARCHAR(50) NOT NULL,
                CONSTRAINT FK_Transacciones_Vendedor FOREIGN KEY (IdVendedor) REFERENCES usuario(ID_Usuarios),
                CONSTRAINT FK_Transacciones_Comprador FOREIGN KEY (IdComprador) REFERENCES usuario(ID_Usuarios),
                CONSTRAINT FK_Transacciones_InventarioUser FOREIGN KEY (IdInventarioUser) REFERENCES inventario_usuario(id_inventario_user)
            );
        ");
    } catch (Exception ex) {
        Console.WriteLine("Error creando tabla transacciones: " + ex.Message);
    }
}

app.UseCors("PermitirAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
