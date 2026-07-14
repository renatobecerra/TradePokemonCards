using System;
using Microsoft.EntityFrameworkCore;
using Backend.Models;

class Program
{
    static void Main()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PokemonMarketContext>();
        optionsBuilder.UseMySQL("Server=localhost;Database=pokemonmarket;Uid=root;Pwd=proyectos_2026;");
        using (var context = new PokemonMarketContext(optionsBuilder.Options))
        {
            try {
                context.Database.ExecuteSqlRaw("UPDATE mensajes SET Fecha = DATE_SUB(Fecha, INTERVAL 4 HOUR) WHERE Fecha > NOW();");
                Console.WriteLine("Fixed future messages.");
            } catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }
    }
}
