using Backend.Models;
using Backend.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IItemService
    {
        Task<IEnumerable<Inventario>> ObtenerCatalogoAsync();
        Task<(bool exito, string mensaje)> GuardarItemAsync(GuardarItemDto datos);
        Task<IEnumerable<object>> ObtenerGuardadosAsync(int idUsuario);
        Task<(bool exito, string mensaje)> EliminarGuardadoAsync(int idUsuario, int idItem);
        Task<(bool exito, string mensaje, int? idItem)> GuardarTgcItemAsync(GuardarTgcDto datos);
        Task<IEnumerable<object>> ObtenerTopRegistrosAsync();
    }
}
