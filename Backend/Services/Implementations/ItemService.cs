using Backend.Models;
using Backend.DTOs;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services.Implementations
{
    public class ItemService : IItemService
    {
        private readonly PokemonMarketContext _context;

        public ItemService(PokemonMarketContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventario>> ObtenerCatalogoAsync()
        {
            return await _context.Inventarios.ToListAsync();
        }

        public async Task<(bool exito, string mensaje)> GuardarItemAsync(GuardarItemDto datos)
        {
            var yaGuardado = await _context.Guardados
                .AnyAsync(g => g.IdUsuario == datos.IdUsuario && g.IdItem == datos.IdInventario);

            if (yaGuardado)
            {
                return (false, "Este ítem ya está en tu lista de guardados.");
            }

            var nuevoGuardado = new Guardado 
            {
                IdUsuario = datos.IdUsuario,
                IdItem = datos.IdInventario,
                FechaGuardado = DateTime.Now
            };

            _context.Guardados.Add(nuevoGuardado);
            await _context.SaveChangesAsync();

            return (true, "Ítem guardado en tu lista con éxito.");
        }

        public async Task<IEnumerable<object>> ObtenerGuardadosAsync(int idUsuario)
        {
            var guardados = await _context.Guardados
                .Where(g => g.IdUsuario == idUsuario)
                .Include(g => g.IdItemNavigation)
                .Select(g => new {
                    IdLista = g.IdLista,
                    IdItem = g.IdItem,
                    Nombre = g.IdItemNavigation.Nombre,
                    Rareza = g.IdItemNavigation.Rareza,
                    Edicion = g.IdItemNavigation.Edicion,
                    ImgLink = g.IdItemNavigation.ImgLink,
                    IdTgc = g.IdItemNavigation.id_tgc,
                    Precio = g.IdItemNavigation.precio,
                    FechaGuardado = g.FechaGuardado
                })
                .ToListAsync();
            
            return guardados;
        }

        public async Task<(bool exito, string mensaje)> EliminarGuardadoAsync(int idUsuario, int idItem)
        {
            var guardado = await _context.Guardados
                .FirstOrDefaultAsync(g => g.IdUsuario == idUsuario && g.IdItem == idItem);

            if (guardado == null)
            {
                return (false, "Ítem guardado no encontrado.");
            }

            _context.Guardados.Remove(guardado);
            await _context.SaveChangesAsync();

            return (true, "Ítem removido de tus guardados con éxito.");
        }

        public async Task<(bool exito, string mensaje, int? idItem)> GuardarTgcItemAsync(GuardarTgcDto datos)
        {
            // InMemory provider doesn't support transactions in the same way, but it's safe to use normally. 
            // In unit tests we'll mock the service, so we can keep the transaction here.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var item = await _context.Inventarios
                    .FirstOrDefaultAsync(i => i.id_tgc == datos.IdTgc);

                if (item == null)
                {
                    item = new Inventario
                    {
                        Nombre = datos.Nombre,
                        Rareza = datos.Rareza,
                        Edicion = datos.Edicion,
                        ImgLink = datos.ImgLink,
                        id_tgc = datos.IdTgc,
                        precio = datos.Precio
                    };
                    _context.Inventarios.Add(item);
                    await _context.SaveChangesAsync();
                }

                var yaGuardado = await _context.Guardados
                    .AnyAsync(g => g.IdUsuario == datos.IdUsuario && g.IdItem == item.IdItem);

                if (yaGuardado)
                {
                    return (false, "Esta carta ya está en tus deseados.", null);
                }

                var nuevoGuardado = new Guardado
                {
                    IdUsuario = datos.IdUsuario,
                    IdItem = item.IdItem,
                    FechaGuardado = DateTime.Now
                };

                _context.Guardados.Add(nuevoGuardado);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Carta agregada a tus deseados con éxito.", item.IdItem);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

        public async Task<IEnumerable<object>> ObtenerTopRegistrosAsync()
        {
            var topItems = await _context.Guardados
                .GroupBy(g => new
                {
                    g.IdItem,
                    g.IdItemNavigation.Nombre,
                    g.IdItemNavigation.Rareza,
                    g.IdItemNavigation.Edicion,
                    g.IdItemNavigation.ImgLink,
                    Precio = g.IdItemNavigation.precio,
                    IdTgc = g.IdItemNavigation.id_tgc
                })
                .Select(g => new
                {
                    IdItem = g.Key.IdItem,
                    Nombre = g.Key.Nombre,
                    Rareza = g.Key.Rareza,
                    Edicion = g.Key.Edicion,
                    ImgLink = g.Key.ImgLink,
                    Precio = g.Key.Precio,
                    IdTgc = g.Key.IdTgc,
                    Count = g.Count(),
                    EsTopReal = true
                })
                .OrderByDescending(x => x.Count)
                .Take(6)
                .ToListAsync();

            if (topItems.Count > 0)
            {
                return topItems;
            }
            else
            {
                var recentItems = await _context.Inventarios
                    .OrderByDescending(i => i.IdItem)
                    .Take(6)
                    .Select(i => new
                    {
                        IdItem = i.IdItem,
                        Nombre = i.Nombre,
                        Rareza = i.Rareza,
                        Edicion = i.Edicion,
                        ImgLink = i.ImgLink,
                        Precio = i.precio,
                        IdTgc = i.id_tgc,
                        Count = 0,
                        EsTopReal = false
                    })
                    .ToListAsync();

                return recentItems;
            }
        }
    }
}
