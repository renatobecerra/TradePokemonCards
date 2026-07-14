using Backend.Models;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Implementations
{
    public class InventarioService : IInventarioService
    {
        private readonly PokemonMarketContext _context;

        public InventarioService(PokemonMarketContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> ObtenerVendedoresAsync(string idTgc)
        {
            var vendedores = await _context.InventarioUsuarios
                .Include(i => i.IdUsuarioNavigation)
                .Include(i => i.IdItemNavigation)
                .Where(i => i.IdItemNavigation.id_tgc == idTgc)
                .Select(i => new {
                    IdInventarioUser = i.IdInventarioUser,
                    IdUsuario = i.IdUsuario,
                    Nombre = i.IdUsuarioNavigation.Nombre,
                    Apellido = i.IdUsuarioNavigation.Apellido,
                    Correo = i.IdUsuarioNavigation.Correo,
                    Telefono = i.IdUsuarioNavigation.Telefono,
                    Foto = i.IdUsuarioNavigation.ImgPerfil,
                    Calificacion = i.IdUsuarioNavigation.Calificacion,
                    EstadoPresencia = i.IdUsuarioNavigation.EstadoPresencia,
                    Estado = i.EstadoFisico,
                    Cantidad = i.Cantidad,
                    Precio = i.IdItemNavigation.precio,
                    NombreCarta = i.IdItemNavigation.Nombre,
                    RarezaCarta = i.IdItemNavigation.Rareza,
                    EdicionCarta = i.IdItemNavigation.Edicion,
                    ImgLinkCarta = i.IdItemNavigation.ImgLink
                })
                .ToListAsync();
            return vendedores;
        }

        public async Task<int?> ObtenerPrecioPromedioAsync(string idTgc)
        {
            var precios = await _context.InventarioUsuarios
                .Include(i => i.IdItemNavigation)
                .Where(i => i.IdItemNavigation.id_tgc == idTgc && i.IdItemNavigation.precio != null && i.IdItemNavigation.precio > 0)
                .Select(i => i.IdItemNavigation.precio.Value)
                .ToListAsync();

            if (precios.Count == 0)
            {
                return null;
            }

            double promedio = precios.Average();
            return (int)Math.Round(promedio);
        }

        public async Task<IEnumerable<object>> ObtenerInventarioAsync(int idUsuario)
        {
            var items = await _context.InventarioUsuarios
                .Where(i => i.IdUsuario == idUsuario)
                .Include(i => i.IdItemNavigation)
                .Select(i => new {
                    IdInventarioUser = i.IdInventarioUser,
                    IdItem = i.IdItem,
                    Nombre = i.IdItemNavigation.Nombre,
                    Estado = i.EstadoFisico,
                    Rareza = i.IdItemNavigation.Rareza,
                    Edicion = i.IdItemNavigation.Edicion,
                    ImgLink = i.IdItemNavigation.ImgLink,
                    IdTgc = i.IdItemNavigation.id_tgc,
                    Precio = i.IdItemNavigation.precio,
                    Cantidad = i.Cantidad
                })
                .ToListAsync();
            return items;
        }

        public async Task<(bool exito, string mensaje)> AgregarAlInventarioAsync(CrearItemDto datos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existe = await _context.InventarioUsuarios
                    .Include(iu => iu.IdItemNavigation)
                    .FirstOrDefaultAsync(iu => 
                        iu.IdUsuario == datos.IdUsuario && 
                        iu.EstadoFisico == datos.Estado &&
                        (datos.IdTgc != null ? iu.IdItemNavigation.id_tgc == datos.IdTgc : iu.IdItemNavigation.Nombre == datos.Nombre && iu.IdItemNavigation.Edicion == datos.Edicion)
                    );

                int cantidadAAgregar = datos.Cantidad ?? 1;

                if (existe != null)
                {
                    existe.Cantidad = (existe.Cantidad ?? 0) + cantidadAAgregar;
                    existe.IdItemNavigation.precio = (int?)(datos.Precio);
                    _context.InventarioUsuarios.Update(existe);
                }
                else
                {
                    var nuevoInventario = new Inventario
                    {
                        Nombre = datos.Nombre,
                        Rareza = datos.Rareza,
                        Edicion = datos.Edicion,
                        ImgLink = datos.ImgLink,
                        id_tgc = datos.IdTgc,
                        precio = (int?)(datos.Precio)
                    };
                    
                    _context.Inventarios.Add(nuevoInventario);
                    await _context.SaveChangesAsync();

                    var vinculo = new InventarioUsuario
                    {
                        IdUsuario = datos.IdUsuario,
                        IdItem = nuevoInventario.IdItem,
                        EstadoFisico = datos.Estado,
                        Cantidad = cantidadAAgregar,
                        FechaObtencion = DateTime.Now
                    };

                    _context.InventarioUsuarios.Add(vinculo);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Operación realizada con éxito.");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<(bool exito, string mensaje)> EditarItemAsync(int idInventarioUser, CrearItemDto datos)
        {
            var iu = await _context.InventarioUsuarios
                .Include(i => i.IdItemNavigation)
                .FirstOrDefaultAsync(i => i.IdInventarioUser == idInventarioUser);

            if (iu == null) return (false, "Ítem no encontrado.");

            iu.EstadoFisico = datos.Estado;
            iu.Cantidad = datos.Cantidad ?? iu.Cantidad;
            iu.IdItemNavigation.precio = (int?)(datos.Precio);

            await _context.SaveChangesAsync();
            return (true, "Carta actualizada con éxito.");
        }

        public async Task<(bool exito, string mensaje)> EliminarItemAsync(int idInventarioUser)
        {
            var iu = await _context.InventarioUsuarios.FindAsync(idInventarioUser);
            if (iu == null) return (false, "Ítem no encontrado.");

            if (iu.Cantidad > 1)
            {
                iu.Cantidad--;
                _context.InventarioUsuarios.Update(iu);
            }
            else
            {
                _context.InventarioUsuarios.Remove(iu);
            }
            await _context.SaveChangesAsync();

            return (true, "Carta eliminada de tu inventario.");
        }
    }
}
